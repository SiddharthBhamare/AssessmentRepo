using ChipsAggregator.Message.Infrastructure.Abstractions;
using ChipsAggregator.Message.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChipsAggregator.Service.Controllers
{

    [ApiController]
    [Route("[controller]")] // Base route will be /search
    public class ExportController : ControllerBase
    {
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly ILogger<ExportController> _logger;
        public ExportController(IRabbitMqPublisher rabbitMqPublisher, ILogger<ExportController> logger)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
            _logger = logger;
        }
        [HttpPost("start-export")]
        public async Task<IActionResult> StartExport([FromBody] ExportRequest request)
        {
            try
            {
                await _rabbitMqPublisher.PublishExportRequestAsync(request);
                return Ok("Export started. Please check the status later.");
            }
            catch (Exception ex) {
                _logger.LogError("Error to start export",ex);
                throw;
            }
        }
    }
}
