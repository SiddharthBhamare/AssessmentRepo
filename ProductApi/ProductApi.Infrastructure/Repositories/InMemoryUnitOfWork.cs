using ProductApi.Domain.Repositories;

namespace ProductApi.Infrastructure.Repositories
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        public IProductRepository Products { get; }

        // The UoW holds references to the repositories it manages
        public InMemoryUnitOfWork(IProductRepository productRepository)
        {
            Products = productRepository;
        }

        // In an in-memory scenario, CommitAsync doesn't perform actual database saves.
        // It's here for architectural consistency and future database integration.
        public Task<int> CommitAsync()
        {
            // Simulate saving changes. In a real DB UoW, this would call SaveChangesAsync()
            // on the database context.
            Console.WriteLine("InMemoryUnitOfWork: Committing changes (no actual persistence).");
            return Task.FromResult(0); // Return 0 changes saved
        }

        // Dispose method (required by IDisposable)
        public void Dispose()
        {
            // Clean up resources if necessary. In this in-memory case, there's nothing to dispose.
            Console.WriteLine("InMemoryUnitOfWork: Disposed.");
        }
    }
}
