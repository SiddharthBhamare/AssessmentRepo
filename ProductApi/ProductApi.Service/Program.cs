using Microsoft.OpenApi.Models;
using ProductApi.Application.Abstractions;
using ProductApi.Application.Services;
using ProductApi.Domain.Repositories;
using ProductApi.Infrastructure.Repositories;

namespace ProductApi.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Configure Services (equivalent to Startup.ConfigureServices) ---

            // Standard controller setup
            builder.Services.AddControllers();

            // Register the Repository and Unit of Work for dependency injection
            // Use AddSingleton for the in-memory repository as the dictionary is static
            builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

            // Use AddScoped for the Unit of Work, as it typically represents a single business transaction scope
            // In this in-memory case, it manages the lifetime of the repository instance within the request scope.
            builder.Services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

            // Register the Application Service, which depends on the Unit of Work
            builder.Services.AddScoped<IProductApplicationService, ProductApplicationService>();

            // Add Swagger/OpenAPI support (Optional, but good for API documentation)
            builder.Services.AddEndpointsApiExplorer(); // Needed for Swagger with minimal APIs, good practice here too
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
                //To Enable swagger authorization enable these settings and configure AddSecurityDefinition and AddSecurityRequirement 
            });

            // --- Build the app ---
            var app = builder.Build();

            // --- Configure the HTTP request pipeline (equivalent to Startup.Configure) ---

            var env = app.Environment; // Get the hosting environment

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Enable Swagger UI in development
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Use Authorization and Authentication if needed in a real app
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapControllers(); // Map controller endpoints

            app.Run();
        }
    }
}
