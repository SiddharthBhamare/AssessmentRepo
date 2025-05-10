using ChipsAggregator.Message.Infrastructure.Abstractions;
using ChipsAggregator.Message.Infrastructure.Publisher;

namespace ChipsAggregator.Service
{
    public class Program
    {
        public static async Task Main(string[] args) // Changed to async Main
        {
            var builder = WebApplication.CreateBuilder(args);
            // Load configuration
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            // Register services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Dependency injection
            builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

            var app = builder.Build();

            // Enable Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
