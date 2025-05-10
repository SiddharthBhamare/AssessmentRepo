using ChipsAggregator.Message.Infrastructure.Abstractions;
using ChipsAggregator.Message.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ChipsAggregator.Message.Infrastructure.Publisher
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly string _hostName;
        private readonly string _queueName;
        private readonly ILogger<RabbitMqPublisher> _logger;
        public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
        {
            _hostName = configuration["RabbitMq:HostName"];
            _queueName = configuration["RabbitMq:QueueName"];
            _logger = logger;
        }

        public async Task PublishExportRequestAsync(ExportRequest request)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _hostName };
                using var connection = factory.CreateConnectionAsync().Result;
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var message = JsonSerializer.Serialize(request);
                var body = Encoding.UTF8.GetBytes(message);
                var basicProperties = new BasicProperties();
                await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: basicProperties, body: body);
            }
            catch (Exception ex) {

                Console.WriteLine(ex.ToString());
                _logger.LogError(ex.ToString());
                throw;
            }
        }
    }
}
