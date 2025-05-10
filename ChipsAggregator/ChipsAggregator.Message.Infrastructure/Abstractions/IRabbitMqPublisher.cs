using ChipsAggregator.Message.Infrastructure.Models;

namespace ChipsAggregator.Message.Infrastructure.Abstractions
{
    public interface IRabbitMqPublisher
    {
        Task PublishExportRequestAsync(ExportRequest request);
    }
}
