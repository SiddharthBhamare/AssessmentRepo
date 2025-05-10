using ChipsAggregator.Domain.Models;
using ChipsAggregator.Worker.Abstractions;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace ChipsAggregator.Worker.Services
{
    public class ExcelExporter : IExcelExporter
    {
        private readonly ILogger<ExcelExporter> _logger;
        public ExcelExporter(ILogger<ExcelExporter> logger)
        {
            _logger = logger;
        }
        public async Task ExportToExcelAsync(List<Offer> offers)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("DistributorOffers");

                worksheet.Cell(1, 1).Value = "Distributor Name";
                worksheet.Cell(1, 2).Value = "Seller Name";
                worksheet.Cell(1, 3).Value = "MOQ";
                worksheet.Cell(1, 4).Value = "SPQ";
                worksheet.Cell(1, 5).Value = "Unit Price";
                worksheet.Cell(1, 6).Value = "Currency";
                worksheet.Cell(1, 7).Value = "Offer URL";
                worksheet.Cell(1, 8).Value = "Timestamp";

                for (int i = 0; i < offers.Count; i++)
                {
                    var offer = offers[i];
                    worksheet.Cell(i + 2, 1).Value = offer.DistributorName;
                    worksheet.Cell(i + 2, 2).Value = offer.SellerName;
                    worksheet.Cell(i + 2, 3).Value = offer.MOQ;
                    worksheet.Cell(i + 2, 4).Value = offer.SPQ;
                    worksheet.Cell(i + 2, 5).Value = offer.UnitPrice;
                    worksheet.Cell(i + 2, 6).Value = offer.Currency;
                    worksheet.Cell(i + 2, 7).Value = offer.OfferUrl;
                    worksheet.Cell(i + 2, 8).Value = offer.Timestamp;
                }

                var filePath = Path.Combine("Exports", $"Offers_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
                Directory.CreateDirectory("Exports");
                workbook.SaveAs(filePath);
            }
            catch (Exception ex) { 
                _logger.LogError(ex.ToString());
                throw;
            }
        }
    }
}
