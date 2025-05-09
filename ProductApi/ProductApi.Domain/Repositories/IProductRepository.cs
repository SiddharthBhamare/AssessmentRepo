using ProductApi.Domain.Models;

namespace ProductApi.Domain.Repositories
{
    // Repository interface for abstracting data access
    // Repository methods here focus on getting/adding/removing from the collection,
    // saving changes is handled by the Unit of Work.
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task<Product> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();
        // Update is often implicit when using an ORM and UoW,
        // but we keep it explicit here for the in-memory example
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
    }
}
