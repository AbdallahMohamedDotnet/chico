using ChicoDesktopApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ChicoDesktopApp.Repositories
{
    public class ReportRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public ReportRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        #region Sales Reports

        /// <summary>
        /// Get today's sales total
        /// </summary>
        public decimal GetTodaysSales()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT COALESCE(SUM(TotalAmount), 0)
                FROM SalesInvoices
                WHERE DATE(InvoiceDate) = DATE('now', 'localtime')";

            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Get today's profit
        /// </summary>
        public decimal GetTodaysProfit()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT COALESCE(SUM(TotalProfit), 0)
                FROM SalesInvoices
                WHERE DATE(InvoiceDate) = DATE('now', 'localtime')";

            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Get sales by date range
        /// </summary>
        public (decimal totalSales, decimal totalProfit, int invoiceCount) GetSalesByDateRange(DateTime startDate, DateTime endDate)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    COALESCE(SUM(TotalAmount), 0) as TotalSales,
                    COALESCE(SUM(TotalProfit), 0) as TotalProfit,
                    COUNT(*) as InvoiceCount
                FROM SalesInvoices
                WHERE DATE(InvoiceDate) BETWEEN @startDate AND @endDate";

            cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    Convert.ToDecimal(reader["TotalSales"]),
                    Convert.ToDecimal(reader["TotalProfit"]),
                    Convert.ToInt32(reader["InvoiceCount"])
                );
            }

            return (0, 0, 0);
        }

        /// <summary>
        /// Get daily sales summary for a date range
        /// </summary>
        public List<(DateTime date, decimal sales, decimal profit, int count)> GetDailySalesSummary(DateTime startDate, DateTime endDate)
        {
            var results = new List<(DateTime, decimal, decimal, int)>();

            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    DATE(InvoiceDate) as SaleDate,
                    COALESCE(SUM(TotalAmount), 0) as TotalSales,
                    COALESCE(SUM(TotalProfit), 0) as TotalProfit,
                    COUNT(*) as InvoiceCount
                FROM SalesInvoices
                WHERE DATE(InvoiceDate) BETWEEN @startDate AND @endDate
                GROUP BY DATE(InvoiceDate)
                ORDER BY SaleDate DESC";

            cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add((
                    DateTime.Parse(reader["SaleDate"].ToString()!),
                    Convert.ToDecimal(reader["TotalSales"]),
                    Convert.ToDecimal(reader["TotalProfit"]),
                    Convert.ToInt32(reader["InvoiceCount"])
                ));
            }

            return results;
        }

        #endregion

        #region Purchase Reports

        /// <summary>
        /// Get purchase expenses by date range
        /// </summary>
        public (decimal totalPurchases, int invoiceCount) GetPurchasesByDateRange(DateTime startDate, DateTime endDate)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    COALESCE(SUM(TotalAmount), 0) as TotalPurchases,
                    COUNT(*) as InvoiceCount
                FROM PurchaseInvoices
                WHERE DATE(InvoiceDate) BETWEEN @startDate AND @endDate";

            cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    Convert.ToDecimal(reader["TotalPurchases"]),
                    Convert.ToInt32(reader["InvoiceCount"])
                );
            }

            return (0, 0);
        }

        #endregion

        #region Inventory Reports

        /// <summary>
        /// Get total inventory value
        /// </summary>
        public (decimal purchaseValue, decimal saleValue, int productCount) GetInventoryValue()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    COALESCE(SUM(CurrentStock * PurchasePrice), 0) as PurchaseValue,
                    COALESCE(SUM(CurrentStock * SalePrice), 0) as SaleValue,
                    COUNT(*) as ProductCount
                FROM Products
                WHERE CurrentStock > 0";

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    Convert.ToDecimal(reader["PurchaseValue"]),
                    Convert.ToDecimal(reader["SaleValue"]),
                    Convert.ToInt32(reader["ProductCount"])
                );
            }

            return (0, 0, 0);
        }

        /// <summary>
        /// Get low stock products report
        /// </summary>
        public List<Product> GetLowStockReport()
        {
            var products = new List<Product>();

            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT p.*, c.Name as CategoryName, c.NameArabic as CategoryNameArabic
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.CurrentStock <= p.MinimumStock
                ORDER BY (p.CurrentStock - p.MinimumStock) ASC, p.ProductName";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    ProductNameArabic = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Barcode = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CategoryId = reader.GetInt32(4),
                    CurrentStock = reader.GetInt32(5),
                    MinimumStock = reader.GetInt32(6),
                    PurchasePrice = reader.GetDecimal(7),
                    SalePrice = reader.GetDecimal(8),
                    Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CategoryName = reader.IsDBNull(11) ? "Unknown" : reader.GetString(11),
                    CategoryNameArabic = reader.IsDBNull(12) ? null : reader.GetString(12)
                });
            }

            return products;
        }

        /// <summary>
        /// Get out of stock products
        /// </summary>
        public List<Product> GetOutOfStockProducts()
        {
            var products = new List<Product>();

            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT p.*, c.Name as CategoryName, c.NameArabic as CategoryNameArabic
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.CurrentStock = 0
                ORDER BY p.ProductName";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    ProductNameArabic = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Barcode = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CategoryId = reader.GetInt32(4),
                    CurrentStock = reader.GetInt32(5),
                    MinimumStock = reader.GetInt32(6),
                    PurchasePrice = reader.GetDecimal(7),
                    SalePrice = reader.GetDecimal(8),
                    Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CategoryName = reader.IsDBNull(11) ? "Unknown" : reader.GetString(11),
                    CategoryNameArabic = reader.IsDBNull(12) ? null : reader.GetString(12)
                });
            }

            return products;
        }

        /// <summary>
        /// Get top selling products
        /// </summary>
        public List<(Product product, int totalSold, decimal totalRevenue)> GetTopSellingProducts(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            var results = new List<(Product, int, decimal)>();

            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var cmd = connection.CreateCommand();
            
            string dateFilter = "";
            if (startDate.HasValue && endDate.HasValue)
            {
                dateFilter = "AND DATE(si.InvoiceDate) BETWEEN @startDate AND @endDate";
                cmd.Parameters.AddWithValue("@startDate", startDate.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@endDate", endDate.Value.ToString("yyyy-MM-dd"));
            }

            cmd.CommandText = $@"
                SELECT 
                    p.*,
                    c.Name as CategoryName,
                    c.NameArabic as CategoryNameArabic,
                    SUM(sii.Quantity) as TotalSold,
                    SUM(sii.Subtotal) as TotalRevenue
                FROM SalesInvoiceItems sii
                INNER JOIN Products p ON sii.ProductId = p.Id
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                INNER JOIN SalesInvoices si ON sii.InvoiceId = si.Id
                WHERE 1=1 {dateFilter}
                GROUP BY p.Id
                ORDER BY TotalSold DESC
                LIMIT @topCount";

            cmd.Parameters.AddWithValue("@topCount", topCount);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var product = new Product
                {
                    Id = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    ProductNameArabic = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Barcode = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CategoryId = reader.GetInt32(4),
                    CurrentStock = reader.GetInt32(5),
                    MinimumStock = reader.GetInt32(6),
                    PurchasePrice = reader.GetDecimal(7),
                    SalePrice = reader.GetDecimal(8),
                    Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CategoryName = reader.IsDBNull(11) ? "Unknown" : reader.GetString(11),
                    CategoryNameArabic = reader.IsDBNull(12) ? null : reader.GetString(12)
                };

                int totalSold = Convert.ToInt32(reader["TotalSold"]);
                decimal totalRevenue = Convert.ToDecimal(reader["TotalRevenue"]);

                results.Add((product, totalSold, totalRevenue));
            }

            return results;
        }

        #endregion

        #region Summary Reports

        /// <summary>
        /// Get comprehensive dashboard statistics
        /// </summary>
        public DashboardStats GetDashboardStatistics()
        {
            var stats = new DashboardStats();

            using var connection = _dbHelper.GetConnection();
            connection.Open();

            // Today's sales
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    COALESCE(SUM(TotalAmount), 0) as TodaySales,
                    COALESCE(SUM(TotalProfit), 0) as TodayProfit,
                    COUNT(*) as TodayInvoices
                FROM SalesInvoices
                WHERE DATE(InvoiceDate) = DATE('now', 'localtime')";

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    stats.TodaysSales = Convert.ToDecimal(reader["TodaySales"]);
                    stats.TodaysProfit = Convert.ToDecimal(reader["TodayProfit"]);
                    stats.TodaysInvoiceCount = Convert.ToInt32(reader["TodayInvoices"]);
                }
            }

            // This month's sales
            cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    COALESCE(SUM(TotalAmount), 0) as MonthSales,
                    COALESCE(SUM(TotalProfit), 0) as MonthProfit
                FROM SalesInvoices
                WHERE strftime('%Y-%m', InvoiceDate) = strftime('%Y-%m', 'now', 'localtime')";

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    stats.MonthSales = Convert.ToDecimal(reader["MonthSales"]);
                    stats.MonthProfit = Convert.ToDecimal(reader["MonthProfit"]);
                }
            }

            // Product statistics
            cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    COUNT(*) as TotalProducts,
                    SUM(CASE WHEN CurrentStock <= MinimumStock THEN 1 ELSE 0 END) as LowStockCount,
                    SUM(CASE WHEN CurrentStock = 0 THEN 1 ELSE 0 END) as OutOfStockCount
                FROM Products";

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    stats.TotalProducts = Convert.ToInt32(reader["TotalProducts"]);
                    stats.LowStockProducts = Convert.ToInt32(reader["LowStockCount"]);
                    stats.OutOfStockProducts = Convert.ToInt32(reader["OutOfStockCount"]);
                }
            }

            // Inventory value
            cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    COALESCE(SUM(CurrentStock * PurchasePrice), 0) as InventoryValue
                FROM Products";

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    stats.InventoryValue = Convert.ToDecimal(reader["InventoryValue"]);
                }
            }

            return stats;
        }

        #endregion
    }

    /// <summary>
    /// Dashboard statistics model
    /// </summary>
    public class DashboardStats
    {
        public decimal TodaysSales { get; set; }
        public decimal TodaysProfit { get; set; }
        public int TodaysInvoiceCount { get; set; }
        
        public decimal MonthSales { get; set; }
        public decimal MonthProfit { get; set; }
        
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        
        public decimal InventoryValue { get; set; }
    }
}
