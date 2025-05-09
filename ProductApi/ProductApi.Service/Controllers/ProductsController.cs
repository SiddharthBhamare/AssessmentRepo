using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.Abstractions;
using ProductApi.Domain.Models;

namespace ProductApi.Service.Controllers
{
    [ApiController]
    [Route("[controller]")] // Base route for this controller will be /products
    public class ProductsController : ControllerBase
    {
        private readonly IProductApplicationService _productApplicationService;

        // Inject the application service via the constructor
        public ProductsController(IProductApplicationService productApplicationService)
        {
            _productApplicationService = productApplicationService;
        }

        // GET: /products
        // Supports filtering by category, min_price, max_price and pagination
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string category,
            [FromQuery] double? min_price,
            [FromQuery] double? max_price,
            [FromQuery] int page = 1, // Default page is 1
            [FromQuery] int pageSize = 10) // Default page size is 10
        {
            // Controller delegates to the application service
            var result = await _productApplicationService.GetProductsAsync(category, min_price, max_price, page, pageSize);
            return Ok(result); // 200 OK
        }

        // GET: /products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            // Controller delegates to the application service
            var product = await _productApplicationService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(product); // 200 OK
        }

        // POST: /products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product productDto) // Using Product as DTO for simplicity
        {
            // Model validation is automatically handled by [ApiController]
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            try
            {
                // Controller delegates to the application service, passing raw data or a command object
                var createdProduct = await _productApplicationService.CreateProductAsync(
                    productDto.Name,
                    productDto.Category,
                    productDto.Quantity,
                    productDto.Price
                );

                // Return 201 Created with the location of the new resource
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.ProductID }, createdProduct);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle domain validation errors
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle other domain errors
            }
        }

        // PUT: /products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product productDto) // Using Product as DTO for simplicity
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            try
            {
                // Controller delegates to the application service
                var product = await _productApplicationService.UpdateProductAsync(
                    id,
                    productDto.Name,
                    productDto.Category,
                    productDto.Quantity,
                    productDto.Price
                );

                if (product == null)
                {
                    return NotFound(); // 404 Not Found
                }

                return Ok(product); // 200 OK
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle domain validation errors
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle other domain errors
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // Product not found during update in repository
            }
        }

        // DELETE: /products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            // Controller delegates to the application service
            var deleted = await _productApplicationService.DeleteProductAsync(id);
            if (!deleted)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(new { message = "Product deleted successfully" }); // 200 OK
        }

        // Example endpoint using a method that leverages the UoW
        [HttpPost("{id}/order")]
        public async Task<IActionResult> ProcessOrder(Guid id, [FromBody] int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest(new { error = "Quantity must be positive." });
            }

            try
            {
                await _productApplicationService.ProcessOrderForProductAsync(id, quantity);
                return Ok(new { message = $"Order processed for product {id}, quantity {quantity}." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Product with ID {id} not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle insufficient quantity
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle invalid quantity
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                return StatusCode(500, new { error = "An error occurred while processing the order.", details = ex.Message });
            }
        }
    }
}
