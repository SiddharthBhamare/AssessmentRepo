using System.ComponentModel.DataAnnotations;

namespace ProductApi.Domain.Models
{
    public class Product
    {
        public Guid ProductID { get; private set; } // Private setter enforces creation via factory or constructor
        [Required]
        public string Name { get; private set; } // Private setters enforce domain rules
        [Required]
        public string Category { get; private set; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; private set; }
        [Range(0, double.MaxValue)]
        public double Price { get; private set; }

        // Private constructor for ORM or internal creation
        private Product() { }

        // Public factory method or constructor for creating new products, potentially with validation
        public Product(Guid productID, string name, string category, int quantity, double price)
        {
            if (productID == Guid.Empty)
            {
                throw new ArgumentException("ProductID cannot be empty.", nameof(productID));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category cannot be null or whitespace.", nameof(category));
            }
            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
            }
            if (price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
            }


            ProductID = productID;
            Name = name;
            Category = category;
            Quantity = quantity;
            Price = price;
        }

        // Method to update product details (domain behavior)
        public void UpdateDetails(string name, string category, int quantity, double price)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category cannot be null or whitespace.", nameof(category));
            }
            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
            }
            if (price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
            }

            Name = name;
            Category = category;
            Quantity = quantity;
            Price = price;
        }

        // Example of domain behavior: decreasing quantity
        public void DecreaseQuantity(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Decrease amount cannot be negative.");
            }
            if (Quantity < amount)
            {
                throw new InvalidOperationException("Insufficient quantity.");
            }
            Quantity -= amount;
        }

        // Example of domain behavior: increasing quantity
        public void IncreaseQuantity(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Increase amount cannot be negative.");
            }
            Quantity += amount;
        }
    }
}
