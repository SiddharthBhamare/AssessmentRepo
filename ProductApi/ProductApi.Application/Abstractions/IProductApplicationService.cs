using ProductApi.Domain.Models;

namespace ProductApi.Application.Abstractions
{
    public interface IProductApplicationService
    {
        Task<Product> CreateProductAsync(string name, string category, int quantity, double price);
        Task<PagedResult<Product>> GetProductsAsync(string category, double? minPrice, double? maxPrice, int page, int pageSize);
        Task<Product> GetProductByIdAsync(Guid id);
        Task<Product> UpdateProductAsync(Guid id, string name, string category, int quantity, double price);
        Task<bool> DeleteProductAsync(Guid id);

        // Example of a method that might involve multiple domain operations within one UoW
        Task ProcessOrderForProductAsync(Guid productId, int quantityOrdered);
    }
}
