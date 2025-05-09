using ProductApi.Domain.Models;
using ProductApi.Domain.Repositories;
using System.Collections.Concurrent;

namespace ProductApi.Infrastructure.Repositories
{
    // In-memory implementation of the Product Repository
    // Note: With in-memory, changes are immediate. The UoW here is more conceptual
    // and for architectural consistency with a real database scenario.
    public class InMemoryProductRepository : IProductRepository
    {
        // In-memory storage (using ConcurrentDictionary for thread safety)
        // This static dictionary acts as our "in-memory database"
        private static readonly ConcurrentDictionary<Guid, Product> _products = new ConcurrentDictionary<Guid, Product>();

        public Task AddAsync(Product product)
        {
            if (!_products.TryAdd(product.ProductID, product))
            {
                // Handle case where product with this ID already exists (optional based on requirements)
                throw new InvalidOperationException($"Product with ID {product.ProductID} already exists.");
            }
            // In a real DB, this would add to a context/ DbSet, not save immediately
            return Task.CompletedTask;
        }

        public Task<Product> GetByIdAsync(Guid id)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            // Return a copy to prevent external modification without using the repository
            return Task.FromResult(_products.Values.ToList().AsEnumerable());
        }

        public Task UpdateAsync(Product product)
        {
            // In-memory update: replace the existing product with the updated one
            // This simulates updating an entity tracked by a context
            if (!_products.ContainsKey(product.ProductID))
            {
                throw new KeyNotFoundException($"Product with ID {product.ProductID} not found.");
            }
            _products[product.ProductID] = product; // Overwrite the existing entry
                                                    // In a real DB, this might be a no-op if the entity is already tracked and modified
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _products.TryRemove(id, out _);
            // In a real DB, this would mark an entity for deletion in the context
            return Task.CompletedTask;
        }
    }
}
