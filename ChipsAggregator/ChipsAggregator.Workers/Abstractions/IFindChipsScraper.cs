using ChipsAggregator.Domain.Models;

namespace ChipsAggregator.Worker.Abstractions
{
    public interface IFindChipsScraper
    {
        public Task<List<Offer>> ScrapeAsync(string partNumber);
    }
}
