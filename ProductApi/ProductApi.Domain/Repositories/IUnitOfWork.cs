namespace ProductApi.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Expose repositories managed by this Unit of Work
        IProductRepository Products { get; } // Example: If UoW manages multiple repositories

        // Method to commit all changes within the unit of work
        Task<int> CommitAsync();
    }
}
