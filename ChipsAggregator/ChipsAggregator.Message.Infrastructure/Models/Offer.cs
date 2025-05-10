namespace ChipsAggregator.Domain.Models
{
    public class Offer
    {
        public string DistributorName { get; set; }
        public string SellerName { get; set; }
        public string MOQ { get; set; }
        public string SPQ { get; set; }
        public string UnitPrice { get; set; }
        public string Currency { get; set; }
        public string OfferUrl { get; set; }
        public string Quantity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now; // Add timestamp

        public override string ToString()
        {
            return $"Offer(Distributor: {DistributorName}, Seller: {SellerName}, MOQ: {MOQ}, SPQ: {SPQ}, Price: {UnitPrice} {Currency}, URL: {OfferUrl})";
        }
    }
}
