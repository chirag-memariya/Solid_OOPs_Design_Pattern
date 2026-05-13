using System;
using System.Collections.Generic;
using System.Linq;

namespace YourProject.BusinessLogic
{
    // Represents a simple Product entity
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }

    // Business logic for managing Products
    public class ProductService
    {
        // This would typically be a database context or repository,
        // but for simplicity, we'll use an in-memory list.
        private List<Product> _products;
        private int _nextId = 1;

        public ProductService()
        {
            _products = new List<Product>();
            // Seed some initial data
            AddProduct(new Product { Name = "Laptop", Price = 1200.00m, StockQuantity = 50 });
            AddProduct(new Product { Name = "Mouse", Price = 25.00m, StockQuantity = 200 });
            AddProduct(new Product { Name = "Keyboard", Price = 75.50m, StockQuantity = 100 });
        }

        /// <summary>
        /// Adds a new product to the system.
        /// </summary>
        /// <param name="product">The product to add.</param>
        /// <returns>The added product with its new ID.</returns>
        public Product AddProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Product name cannot be empty.", nameof(product.Name));
            }

            product.Id = _nextId++;
            _products.Add(product);
            Console.WriteLine($"Product '{product.Name}' added with ID {product.Id}");
            return product;
        }

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The product if found, otherwise null.</returns>
        public Product GetProductById(int id)
        {
            Console.WriteLine($"Attempting to retrieve product with ID {id}");
            return _products.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        public IEnumerable<Product> GetAllProducts()
        {
            Console.WriteLine("Retrieving all products.");
            return _products;
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="updatedProduct">The product with updated information.</param>
        /// <returns>True if the product was updated, false otherwise.</returns>
        public bool UpdateProduct(Product updatedProduct)
        {
            if (updatedProduct == null)
            {
                throw new ArgumentNullException(nameof(updatedProduct));
            }
            if (string.IsNullOrWhiteSpace(updatedProduct.Name))
            {
                throw new ArgumentException("Product name cannot be empty.", nameof(updatedProduct.Name));
            }

            Product existingProduct = _products.FirstOrDefault(p => p.Id == updatedProduct.Id);

            if (existingProduct != null)
            {
                // *** BUG INTRODUCED HERE: A property is intentionally missed during update ***
                existingProduct.Name = updatedProduct.Name;
                existingProduct.Price = updatedProduct.Price;
                Console.WriteLine($"Product with ID {updatedProduct.Id} updated.");
                return true;
            }
            Console.WriteLine($"Product with ID {updatedProduct.Id} not found for update.");
            return false;
        }

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>True if the product was deleted, false otherwise.</returns>
        public bool DeleteProduct(int id)
        {
            Product productToDelete = _products.FirstOrDefault(p => p.Id == id);
            if (productToDelete != null)
            {
                _products.Remove(productToDelete);
                Console.WriteLine($"Product with ID {id} deleted.");
                return true;
            }
            Console.WriteLine($"Product with ID {id} not found for deletion.");
            return false;
        }

        /// <summary>
        /// Reduces the stock quantity of a product after a sale.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="quantity">The quantity to reduce.</param>
        /// <returns>True if stock was reduced, false otherwise (e.g., insufficient stock).</returns>
        public bool ReduceStock(int productId, int quantity)
        {
            Product product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                Console.WriteLine($"Product with ID {productId} not found for stock reduction.");
                return false;
            }

            if (product.StockQuantity >= quantity)
            {
                product.StockQuantity -= quantity;
                Console.WriteLine($"Stock for product '{product.Name}' (ID: {productId}) reduced by {quantity}. New stock: {product.StockQuantity}");
                return true;
            }
            Console.WriteLine($"Insufficient stock for product '{product.Name}' (ID: {productId}). Requested: {quantity}, Available: {product.StockQuantity}");
            return false;
        }
    }
}