using ChipsAggregator.Domain.Models;

namespace ChipsAggregator.Worker.Abstractions
{
    public interface IExcelExporter
    {
        Task ExportToExcelAsync(List<Offer> offers);
    }
}
