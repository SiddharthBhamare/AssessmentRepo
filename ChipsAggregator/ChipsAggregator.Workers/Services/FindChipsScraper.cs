using ChipsAggregator.Domain.Models;
using ChipsAggregator.Worker.Abstractions;
using HtmlAgilityPack;

namespace ChipsAggregator.Worker.Services
{
    public class FindChipsScraper : IFindChipsScraper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FindChipsScraper> _logger;

        // Define constants for recurring strings like URL base and XPaths
        private const string BaseUrl = "https://www.findchips.com/search/";
        private const string DistributorRowXPath = "//tbody/tr[contains(@class, 'row')]";
        private const string SellerNameXPath = ".//td[contains(@class, 'td-mfg')]//span";
        private const string MoqXPath = ".//td[contains(@class, 'td-stock')]";
        private const string OfferUrlXPath = ".//td[contains(@class, 'td-buy')]//a[contains(@class, 'buy-button')]";
        private const string PriceListXPath = ".//td[contains(@class, 'td-price')]//ul[@class='price-list']//li";
        private const string QuantityXPath = ".//span[@class='label']";
        private const string UnitPriceXPath = ".//span[@class='value']";
        private const string SeeMoreButtonXPath = ".//button[contains(text(), 'See More')]";

        public FindChipsScraper(HttpClient httpClient, ILogger<FindChipsScraper> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Scrapes FindChips.com for offers related to a given part number.
        /// </summary>
        /// <param name="partNumber">The part number to search for.</param>
        /// <returns>A list of filtered offers (top 5 distributors, top 5 offers per distributor).</returns>
        public async Task<List<Offer>> ScrapeAsync(string partNumber)
        {
            // Use a more specific list type since we know we are returning Offers
            var filteredOffers = new List<Offer>();

            try
            {
                var html = await FetchHtmlAsync(partNumber);
                var distributorRows = ParseDistributorRows(html, partNumber);

                if (distributorRows == null || !distributorRows.Any())
                {
                    _logger.LogWarning("No distributor rows found for part: {PartNumber}. Check XPath '{XPath}' and the scraped HTML.", partNumber, DistributorRowXPath);
                    return filteredOffers; // Return empty list as no data was found
                }

                var allOffers = ExtractAllOffers(distributorRows, partNumber);
                filteredOffers = FilterOffers(allOffers);

                _logger.LogInformation("Finished scraping and filtering. Total filtered offers: {FilteredOfferCount}", filteredOffers.Count);

                return filteredOffers;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed while scraping part: {PartNumber}. Status Code: {StatusCode}", partNumber, httpEx.StatusCode);
                // Depending on requirements, you might return an empty list or re-throw
                throw;
            }
            catch (Exception ex) // Catch other potential exceptions during parsing or processing
            {
                _logger.LogError(ex, "An unexpected error occurred while scraping data for part: {PartNumber}", partNumber);
                throw; // Re-throw the exception after logging
            }
        }

        /// <summary>
        /// Fetches the HTML content for the given part number from FindChips.com.
        /// </summary>
        private async Task<string> FetchHtmlAsync(string partNumber)
        {
            var url = $"{BaseUrl}{partNumber}";
            _logger.LogInformation("Attempting to fetch HTML from URL: {Url}", url);

            // Use ConfigureAwait(false) to potentially improve performance by not needing
            // to marshal back to the original context (useful in non-UI applications).
            var html = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            // Optionally save HTML for debugging purposes.
            // This block can be made conditional on a debug flag or configuration.
            try
            {
                // Ensure the file name is safe
                var safePartNumber = Path.GetInvalidFileNameChars().Aggregate(partNumber, (current, c) => current.Replace(c, '_'));
                var fileName = $"scraped_html_{safePartNumber}.html";
                File.WriteAllText(fileName, html);
                _logger.LogInformation("Saved scraped HTML to {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save scraped HTML for part: {PartNumber}", partNumber);
            }

            return html;
        }

        /// <summary>
        /// Parses the HTML string and returns the collection of distributor row nodes.
        /// </summary>
        private HtmlNodeCollection ParseDistributorRows(string html, string partNumber)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var distributorRows = doc.DocumentNode.SelectNodes(DistributorRowXPath);

            if (distributorRows != null)
            {
                _logger.LogInformation("Found {RowCount} potential distributor rows based on XPath '{XPath}'.", distributorRows.Count, DistributorRowXPath);
            }
            else
            {
                _logger.LogInformation("No nodes found for XPath '{XPath}'.", DistributorRowXPath);
            }

            return distributorRows;
        }

        /// <summary>
        /// Extracts all offer details from the collection of distributor row nodes.
        /// Each row can potentially yield multiple offers due to price breaks.
        /// </summary>
        private List<Offer> ExtractAllOffers(HtmlNodeCollection distributorRows, string partNumber)
        {
            var allOffers = new List<Offer>();

            foreach (var row in distributorRows)
            {
                // Extract distributor name from data attribute
                var distributorName = row.GetAttributeValue("data-distributor_name", "").Trim();
                _logger.LogDebug("Processing row for distributor: {DistributorName}", distributorName);

                // Extract common data points for the row
                var sellerName = GetNodeText(row, SellerNameXPath, "Seller Name");
                var moq = GetNodeText(row, MoqXPath, "MOQ");
                var spq = "N/A"; // SPQ is consistently N/A based on the original code's implementation
                var offerUrl = GetOfferUrl(row);

                // Extract Price Information, handling multiple price breaks
                var priceNodes = row.SelectNodes(PriceListXPath);

                if (priceNodes != null && priceNodes.Any())
                {
                    _logger.LogDebug("  - Found {PriceNodeCount} price list items for distributor {DistributorName}.", priceNodes.Count, distributorName);
                    foreach (var priceNode in priceNodes)
                    {
                        // Skip the "See More" list item if it exists
                        if (priceNode.SelectSingleNode(SeeMoreButtonXPath) != null)
                        {
                            _logger.LogDebug("    - Skipping 'See More' list item for distributor {DistributorName}.", distributorName);
                            continue;
                        }

                        var quantity = GetNodeText(priceNode, QuantityXPath, "Quantity");
                        var (unitPrice, currency) = GetPriceAndCurrency(priceNode);

                        _logger.LogDebug("    - Price Break: Quantity='{Quantity}', UnitPrice='{UnitPrice}', Currency='{Currency}' for distributor {DistributorName}.", quantity, unitPrice, currency, distributorName);

                        allOffers.Add(new Offer
                        {
                            DistributorName = distributorName,
                            SellerName = sellerName,
                            MOQ = moq,
                            SPQ = spq, // SPQ remains constant as per original
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            Currency = currency,
                            OfferUrl = offerUrl // Offer URL is the same for all price breaks in a row
                        });
                    }
                }
                else
                {
                    // Handle cases where no price list is found, perhaps due to different HTML structure or no stock
                    _logger.LogWarning("No price list found in expected locations for a row for part: {PartNumber} from distributor {Distributor}. Row HTML snippet: {RowHtml}", partNumber, distributorName, row.OuterHtml.Substring(0, Math.Min(row.OuterHtml.Length, 500)));

                    // Add an offer with default/placeholder price information
                    allOffers.Add(new Offer
                    {
                        DistributorName = distributorName,
                        SellerName = sellerName,
                        MOQ = moq,
                        SPQ = spq, // SPQ remains constant
                        Quantity = "N/A",
                        UnitPrice = "N/A",
                        Currency = "$", // Default currency
                        OfferUrl = offerUrl
                    });
                }
            }
            _logger.LogInformation("Finished extracting raw offers. Total raw offers: {RawOfferCount}", allOffers.Count);
            return allOffers;
        }

        /// <summary>
        /// Safely extracts InnerText from a node based on an XPath, handling nulls.
        /// </summary>
        private string GetNodeText(HtmlNode parentNode, string xpath, string fieldName)
        {
            var node = parentNode.SelectSingleNode(xpath);
            if (node != null)
            {
                var text = node.InnerText?.Trim() ?? "";
                _logger.LogDebug("  - Extracted '{FieldName}': '{Text}' using XPath '{XPath}'", fieldName, text, xpath);
                return text;
            }
            _logger.LogDebug("  - {FieldName} Node NOT Found using XPath '{XPath}'", fieldName, xpath);
            return "N/A"; // Returning "N/A" for missing data points as in original logic
        }

        /// <summary>
        /// Extracts and converts the offer URL, handling relative URLs.
        /// </summary>
        private string GetOfferUrl(HtmlNode rowNode)
        {
            var offerUrlNode = rowNode.SelectSingleNode(OfferUrlXPath);
            var offerUrl = "";
            if (offerUrlNode != null)
            {
                offerUrl = offerUrlNode.GetAttributeValue("href", "").Trim();
                _logger.LogDebug("  - Offer URL Node Found. Extracted: '{OfferUrl}' using XPath '{XPath}'", offerUrl, OfferUrlXPath);

                // Ensure the URL is absolute if it's relative
                if (!string.IsNullOrEmpty(offerUrl) && !Uri.IsWellFormedUriString(offerUrl, UriKind.Absolute))
                {
                    if (Uri.TryCreate(new Uri("https://www.findchips.com"), offerUrl, out Uri absoluteUri))
                    {
                        offerUrl = absoluteUri.ToString();
                        _logger.LogDebug("  - Converted relative URL to absolute: '{OfferUrl}'", offerUrl);
                    }
                    else
                    {
                        _logger.LogWarning("Could not create absolute URL from relative URL: {RelativeUrl}", offerUrl);
                        offerUrl = ""; // Clear invalid URL
                    }
                }
            }
            else
            {
                _logger.LogDebug("  - Offer URL Node NOT Found using XPath '{XPath}'", OfferUrlXPath);
            }
            return offerUrl;
        }

        /// <summary>
        /// Extracts price and currency from the price list item node using data attributes.
        /// </summary>
        private (string unitPrice, string currency) GetPriceAndCurrency(HtmlNode priceNode)
        {
            var unitPriceNode = priceNode.SelectSingleNode(UnitPriceXPath);
            var unitPrice = "N/A";
            var currency = "$"; // Default currency as in original logic

            if (unitPriceNode != null)
            {
                // Extract from data-baseprice attribute
                unitPrice = unitPriceNode.GetAttributeValue("data-baseprice", "N/A").Trim();
                // Extract from data-basecurrency attribute
                currency = unitPriceNode.GetAttributeValue("data-basecurrency", "$").Trim();
                _logger.LogDebug("  - Extracted Price from Data Attributes: UnitPrice='{UnitPrice}', Currency='{Currency}'", unitPrice, currency);
            }
            else
            {
                _logger.LogDebug("  - Unit Price Node NOT Found using XPath '{XPath}' for price list item.", UnitPriceXPath);
            }

            return (unitPrice, currency);
        }

        /// <summary>
        /// Filters the list of all offers to include only the top 5 distributors and the top 5 offers per distributor.
        /// Preserves the original filtering logic.
        /// </summary>
        private List<Offer> FilterOffers(List<Offer> allOffers)
        {
            _logger.LogInformation("Starting offer filtering process.");
            var filteredOffers = new List<Offer>();

            // Group offers by distributor name, ignoring entries with empty distributor names
            var offersByDistributor = allOffers
                .Where(o => !string.IsNullOrWhiteSpace(o.DistributorName))
                .GroupBy(o => o.DistributorName)
                .ToList();

            _logger.LogInformation("Grouped offers into {DistributorCount} distributors.", offersByDistributor.Count);

            // Select the top 5 distributor groups
            var top5Distributors = offersByDistributor.Take(5);
            _logger.LogInformation("Selected top {Limit} distributors for final list.", 5);

            // For each selected distributor, take the top 5 offers and add them to the filtered list
            foreach (var distributorGroup in top5Distributors)
            {
                var top5OffersForDistributor = distributorGroup.Take(5).ToList(); // Materialize the take operation
                _logger.LogInformation("Adding up to {OffersForDistributorCount} offers for distributor: {DistributorName}", top5OffersForDistributor.Count, distributorGroup.Key);
                filteredOffers.AddRange(top5OffersForDistributor);
            }

            _logger.LogInformation("Finished filtering offers. Total filtered offers: {FilteredOfferCount}", filteredOffers.Count);

            return filteredOffers;
        }
    }
}