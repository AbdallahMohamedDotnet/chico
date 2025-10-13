using ChicoDesktopApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ChicoDesktopApp.Repositories
{
    public class ProductRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public ProductRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Add new product (INV-01)
        public int AddProduct(Product product)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Products 
                    (ProductName, ProductNameArabic, CategoryId, SerialNumber, Barcode, 
                     PurchasePrice, SalePrice, CurrentStock, MinimumStock, Description, IsActive)
                    VALUES ($name, $nameAr, $catId, $serial, $barcode, 
                            $purchasePrice, $salePrice, $stock, $minStock, $desc, $active);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("$name", product.ProductName);
                command.Parameters.AddWithValue("$nameAr", product.ProductNameArabic ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$catId", product.CategoryId);
                command.Parameters.AddWithValue("$serial", product.SerialNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$barcode", product.Barcode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$purchasePrice", product.PurchasePrice);
                command.Parameters.AddWithValue("$salePrice", product.SalePrice);
                command.Parameters.AddWithValue("$stock", product.CurrentStock);
                command.Parameters.AddWithValue("$minStock", product.MinimumStock);
                command.Parameters.AddWithValue("$desc", product.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$active", product.IsActive ? 1 : 0);

                var productId = Convert.ToInt32(command.ExecuteScalar());

                // Log stock movement
                LogStockMovement(connection, productId, "INITIAL", product.CurrentStock, "Product", productId, "Initial stock");

                transaction.Commit();
                return productId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Update product
        public void UpdateProduct(Product product)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Products 
                SET ProductName = $name,
                    ProductNameArabic = $nameAr,
                    CategoryId = $catId,
                    SerialNumber = $serial,
                    Barcode = $barcode,
                    PurchasePrice = $purchasePrice,
                    SalePrice = $salePrice,
                    MinimumStock = $minStock,
                    Description = $desc,
                    IsActive = $active,
                    UpdatedDate = CURRENT_TIMESTAMP
                WHERE Id = $id
            ";
            command.Parameters.AddWithValue("$name", product.ProductName);
            command.Parameters.AddWithValue("$nameAr", product.ProductNameArabic ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$catId", product.CategoryId);
            command.Parameters.AddWithValue("$serial", product.SerialNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$barcode", product.Barcode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$purchasePrice", product.PurchasePrice);
            command.Parameters.AddWithValue("$salePrice", product.SalePrice);
            command.Parameters.AddWithValue("$minStock", product.MinimumStock);
            command.Parameters.AddWithValue("$desc", product.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$active", product.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("$id", product.Id);

            command.ExecuteNonQuery();
        }

        // Update stock (INV-02)
        public void UpdateStock(int productId, int quantity, string movementType, string referenceType, int referenceId)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Products 
                    SET CurrentStock = CurrentStock + $quantity,
                        UpdatedDate = CURRENT_TIMESTAMP
                    WHERE Id = $id
                ";
                command.Parameters.AddWithValue("$quantity", quantity);
                command.Parameters.AddWithValue("$id", productId);
                command.ExecuteNonQuery();

                // Log stock movement
                LogStockMovement(connection, productId, movementType, Math.Abs(quantity), referenceType, referenceId, null);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void LogStockMovement(SqliteConnection connection, int productId, string movementType, 
            int quantity, string referenceType, int referenceId, string? notes)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO StockMovements 
                (ProductId, MovementType, Quantity, ReferenceType, ReferenceId, Notes)
                VALUES ($productId, $type, $qty, $refType, $refId, $notes)
            ";
            command.Parameters.AddWithValue("$productId", productId);
            command.Parameters.AddWithValue("$type", movementType);
            command.Parameters.AddWithValue("$qty", quantity);
            command.Parameters.AddWithValue("$refType", referenceType);
            command.Parameters.AddWithValue("$refId", referenceId);
            command.Parameters.AddWithValue("$notes", notes ?? (object)DBNull.Value);
            command.ExecuteNonQuery();
        }

        // Get all products (INV-03)
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.*, c.Name as CategoryName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.IsActive = 1
                ORDER BY p.ProductName
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        // Search products (INV-03)
        public List<Product> SearchProducts(string searchTerm)
        {
            var products = new List<Product>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.*, c.Name as CategoryName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.IsActive = 1 
                  AND (p.ProductName LIKE $search 
                       OR p.SerialNumber LIKE $search 
                       OR p.Barcode LIKE $search)
                ORDER BY p.ProductName
            ";
            command.Parameters.AddWithValue("$search", $"%{searchTerm}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        // Get low stock products (INV-04)
        public List<Product> GetLowStockProducts()
        {
            var products = new List<Product>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.*, c.Name as CategoryName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.IsActive = 1 AND p.CurrentStock <= p.MinimumStock
                ORDER BY p.CurrentStock ASC
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        // Get product by ID
        public Product? GetProductById(int id)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.*, c.Name as CategoryName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Id = $id
            ";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapProduct(reader);
            }

            return null;
        }

        private Product MapProduct(SqliteDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                ProductName = reader.GetString(1),
                ProductNameArabic = reader.IsDBNull(2) ? null : reader.GetString(2),
                CategoryId = reader.GetInt32(3),
                SerialNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                Barcode = reader.IsDBNull(5) ? null : reader.GetString(5),
                PurchasePrice = reader.GetDecimal(6),
                SalePrice = reader.GetDecimal(7),
                CurrentStock = reader.GetInt32(8),
                MinimumStock = reader.GetInt32(9),
                Description = reader.IsDBNull(10) ? null : reader.GetString(10),
                IsActive = reader.GetInt32(11) == 1,
                CreatedDate = DateTime.Parse(reader.GetString(12)),
                UpdatedDate = reader.IsDBNull(13) ? null : DateTime.Parse(reader.GetString(13)),
                CategoryName = reader.IsDBNull(14) ? "" : reader.GetString(14)
            };
        }

        // Delete product
        public void DeleteProduct(int productId)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Products WHERE Id = $id";
            command.Parameters.AddWithValue("$id", productId);

            command.ExecuteNonQuery();
        }
    }
}
