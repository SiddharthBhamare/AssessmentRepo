using ProductApi.Application.Abstractions;
using ProductApi.Domain.Models;
using ProductApi.Domain.Repositories;

namespace ProductApi.Application.Services
{
    public class ProductApplicationService : IProductApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        // We might still need direct access to a repository for query operations that don't
        // modify state and thus don't require the UoW's Commit.
        // However, often the UoW exposes the repositories it manages.
        private readonly IProductRepository _productRepository;


        // Inject the Unit of Work
        public ProductApplicationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            // Access repositories via the Unit of Work
            _productRepository = _unitOfWork.Products;
        }

        public async Task<Product> CreateProductAsync(string name, string category, int quantity, double price)
        {
            // Application service handles creating the domain object
            var product = new Product(Guid.NewGuid(), name, category, quantity, price);

            // Add to the repository (changes are tracked by the UoW conceptually)
            await _productRepository.AddAsync(product);

            // Commit the unit of work to save changes
            await _unitOfWork.CommitAsync();

            return product;
        }

        public async Task<PagedResult<Product>> GetProductsAsync(string category, double? minPrice, double? maxPrice, int page, int pageSize)
        {
            // Read operations often don't require the UoW's Commit,
            // but we still use the repository exposed by the UoW for consistency.
            var allProducts = await _productRepository.GetAllAsync();

            var query = allProducts.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<Product> GetProductByIdAsync(Guid id)
        {
            // Delegate to the repository via the UoW
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product> UpdateProductAsync(Guid id, string name, string category, int quantity, double price)
        {
            // Application service retrieves the domain object
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return null; // Product not found
            }

            // Call domain method to update details (domain behavior)
            product.UpdateDetails(name, category, quantity, price);

            // Mark for update (in-memory repo updates immediately, real DB repo marks entity)
            await _productRepository.UpdateAsync(product);

            // Commit the unit of work to save changes
            await _unitOfWork.CommitAsync();

            return product;
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            // Retrieve the domain object first (optional, but good practice to ensure it exists)
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return false; // Product not found
            }

            // Delete via the repository
            await _productRepository.DeleteAsync(id);

            // Commit the unit of work to save changes
            await _unitOfWork.CommitAsync();

            return true;
        }

        // Example of a method that might involve multiple domain operations within one UoW
        public async Task ProcessOrderForProductAsync(Guid productId, int quantityOrdered)
        {
            if (quantityOrdered <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantityOrdered), "Quantity ordered must be positive.");
            }

            // Retrieve the product aggregate
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // Perform domain behavior (decrease quantity)
            product.DecreaseQuantity(quantityOrdered);

            // The product entity is now modified. The repository (or ORM tracked by UoW)
            // is aware of this change.

            // In a real scenario, you might also interact with other repositories here,
            // e.g., adding an OrderItem to an Order repository.
            // await _orderRepository.AddOrderItemAsync(orderId, productId, quantityOrdered);

            // Commit the unit of work to save ALL changes atomically
            await _unitOfWork.CommitAsync();

            Console.WriteLine($"Processed order for {quantityOrdered} units of Product ID {productId}. New quantity: {product.Quantity}");
        }
    }
}
