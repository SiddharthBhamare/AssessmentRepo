using ChipsAggregator.Message.Infrastructure.Models;
using ChipsAggregator.Worker.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ChipsAggregator.ScraperWorker
{
    public class ExportWorker : BackgroundService
    {
        private readonly IChannel _channel;
        private readonly IFindChipsScraper _scraper;
        private readonly IExcelExporter _exporter;
        private readonly string _queueName;
        public ExportWorker(IFindChipsScraper scraper, IExcelExporter exporter, IConfiguration configuration)
        {
            var hostName = configuration["RabbitMq:HostName"];
            _queueName = configuration["RabbitMq:QueueName"];
            var factory = new ConnectionFactory() { HostName = hostName };
            var connection = factory.CreateConnectionAsync().Result;
            _channel = connection.CreateChannelAsync().Result;
            _scraper = scraper;
            _exporter = exporter;

            _channel.QueueDeclareAsync(_queueName, false, false, false, null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var request = JsonSerializer.Deserialize<ExportRequest>(Encoding.UTF8.GetString(body));
                var offers = await _scraper.ScrapeAsync(request.PartNumber);
                await _exporter.ExportToExcelAsync(offers);
            };

            _channel.BasicConsumeAsync(_queueName, true, consumer);
            return Task.CompletedTask;
        }
    }
}
