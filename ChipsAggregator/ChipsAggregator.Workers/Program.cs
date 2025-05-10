using ChipsAggregator.ScraperWorker;
using ChipsAggregator.Worker.Abstractions;
using ChipsAggregator.Worker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace ChipsAggregator.Worker
{
    public class Program
    {
        public static async Task Main(string[] args) // Changed to async Main
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddHostedService<ExportWorker>();
                    services.AddSingleton<IFindChipsScraper, FindChipsScraper>();
                    services.AddSingleton<IExcelExporter, ExcelExporter>();
                })
                .Build()
                .Run();
        }

    }
}
