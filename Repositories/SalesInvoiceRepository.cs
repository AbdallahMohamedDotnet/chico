using ChicoDesktopApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ChicoDesktopApp.Repositories
{
    public class SalesInvoiceRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public SalesInvoiceRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Add new sales invoice with items
        public int AddSalesInvoice(SalesInvoice invoice)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert invoice
                var invoiceCmd = connection.CreateCommand();
                invoiceCmd.Transaction = transaction;
                invoiceCmd.CommandText = @"
                    INSERT INTO SalesInvoices (InvoiceNumber, InvoiceDate, CustomerName, CustomerPhone, 
                                               Subtotal, DiscountAmount, TotalAmount, TotalProfit, Notes, CreatedDate)
                    VALUES (@InvoiceNumber, @InvoiceDate, @CustomerName, @CustomerPhone, 
                            @Subtotal, @DiscountAmount, @TotalAmount, @TotalProfit, @Notes, @CreatedDate);
                    SELECT last_insert_rowid();";

                invoiceCmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                invoiceCmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                invoiceCmd.Parameters.AddWithValue("@CustomerName", invoice.CustomerName ?? (object)DBNull.Value);
                invoiceCmd.Parameters.AddWithValue("@CustomerPhone", invoice.CustomerPhone ?? (object)DBNull.Value);
                invoiceCmd.Parameters.AddWithValue("@Subtotal", invoice.Subtotal);
                invoiceCmd.Parameters.AddWithValue("@DiscountAmount", invoice.DiscountAmount);
                invoiceCmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
                invoiceCmd.Parameters.AddWithValue("@TotalProfit", invoice.TotalProfit);
                invoiceCmd.Parameters.AddWithValue("@Notes", invoice.Notes ?? (object)DBNull.Value);
                invoiceCmd.Parameters.AddWithValue("@CreatedDate", invoice.CreatedDate);

                int invoiceId = Convert.ToInt32(invoiceCmd.ExecuteScalar());

                // Insert invoice items and update product stock
                foreach (var item in invoice.Items)
                {
                    // Insert item
                    var itemCmd = connection.CreateCommand();
                    itemCmd.Transaction = transaction;
                    itemCmd.CommandText = @"
                        INSERT INTO SalesInvoiceItems (InvoiceId, ProductId, ProductName, Quantity, UnitPrice, UnitCost, TotalPrice, Profit)
                        VALUES (@InvoiceId, @ProductId, @ProductName, @Quantity, @UnitPrice, @UnitCost, @TotalPrice, @Profit)";

                    itemCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@UnitCost", item.UnitCost);
                    itemCmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                    itemCmd.Parameters.AddWithValue("@Profit", item.Profit);
                    itemCmd.ExecuteNonQuery();

                    // Update product stock (decrement)
                    var stockCmd = connection.CreateCommand();
                    stockCmd.Transaction = transaction;
                    stockCmd.CommandText = @"
                        UPDATE Products 
                        SET CurrentStock = CurrentStock - @Quantity, 
                            UpdatedDate = @UpdatedDate
                        WHERE Id = @ProductId";

                    stockCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                    stockCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    stockCmd.ExecuteNonQuery();

                    // Insert stock movement record
                    var movementCmd = connection.CreateCommand();
                    movementCmd.Transaction = transaction;
                    movementCmd.CommandText = @"
                        INSERT INTO StockMovements (ProductId, MovementType, Quantity, ReferenceType, ReferenceId, MovementDate, Notes)
                        VALUES (@ProductId, 'OUT', @Quantity, 'SALE', @InvoiceId, @MovementDate, @Notes)";

                    movementCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    movementCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    movementCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    movementCmd.Parameters.AddWithValue("@MovementDate", invoice.InvoiceDate);
                    movementCmd.Parameters.AddWithValue("@Notes", $"بيع فاتورة رقم {invoice.InvoiceNumber}");
                    movementCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return invoiceId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Get all sales invoices
        public List<SalesInvoice> GetAllSalesInvoices()
        {
            var invoices = new List<SalesInvoice>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, InvoiceDate, CustomerName, CustomerPhone, 
                       Subtotal, DiscountAmount, TotalAmount, TotalProfit, Notes, CreatedDate
                FROM SalesInvoices
                ORDER BY InvoiceDate DESC, Id DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoices.Add(new SalesInvoice
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    InvoiceDate = reader.GetDateTime(2),
                    CustomerName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CustomerPhone = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Subtotal = reader.GetDecimal(5),
                    DiscountAmount = reader.GetDecimal(6),
                    TotalAmount = reader.GetDecimal(7),
                    TotalProfit = reader.GetDecimal(8),
                    Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CreatedDate = reader.GetDateTime(10)
                });
            }

            return invoices;
        }

        // Get sales invoice by ID with items
        public SalesInvoice? GetSalesInvoiceById(int id)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, InvoiceDate, CustomerName, CustomerPhone, 
                       Subtotal, DiscountAmount, TotalAmount, TotalProfit, Notes, CreatedDate
                FROM SalesInvoices
                WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            SalesInvoice? invoice = null;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    invoice = new SalesInvoice
                    {
                        Id = reader.GetInt32(0),
                        InvoiceNumber = reader.GetString(1),
                        InvoiceDate = reader.GetDateTime(2),
                        CustomerName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CustomerPhone = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Subtotal = reader.GetDecimal(5),
                        DiscountAmount = reader.GetDecimal(6),
                        TotalAmount = reader.GetDecimal(7),
                        TotalProfit = reader.GetDecimal(8),
                        Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
                        CreatedDate = reader.GetDateTime(10)
                    };
                }
            }

            if (invoice != null)
            {
                // Get invoice items
                var itemsCmd = connection.CreateCommand();
                itemsCmd.CommandText = @"
                    SELECT Id, InvoiceId, ProductId, ProductName, Quantity, UnitPrice, UnitCost, TotalPrice, Profit
                    FROM SalesInvoiceItems
                    WHERE InvoiceId = @InvoiceId";
                itemsCmd.Parameters.AddWithValue("@InvoiceId", id);

                using var itemsReader = itemsCmd.ExecuteReader();
                while (itemsReader.Read())
                {
                    invoice.Items.Add(new SalesInvoiceItem
                    {
                        Id = itemsReader.GetInt32(0),
                        InvoiceId = itemsReader.GetInt32(1),
                        ProductId = itemsReader.GetInt32(2),
                        ProductName = itemsReader.GetString(3),
                        Quantity = itemsReader.GetInt32(4),
                        UnitPrice = itemsReader.GetDecimal(5),
                        UnitCost = itemsReader.GetDecimal(6),
                        TotalPrice = itemsReader.GetDecimal(7),
                        Profit = itemsReader.GetDecimal(8)
                    });
                }
            }

            return invoice;
        }

        // Get sales invoices by date range
        public List<SalesInvoice> GetSalesInvoicesByDateRange(DateTime startDate, DateTime endDate)
        {
            var invoices = new List<SalesInvoice>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, InvoiceDate, CustomerName, CustomerPhone, 
                       Subtotal, DiscountAmount, TotalAmount, TotalProfit, Notes, CreatedDate
                FROM SalesInvoices
                WHERE InvoiceDate BETWEEN @StartDate AND @EndDate
                ORDER BY InvoiceDate DESC, Id DESC";
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoices.Add(new SalesInvoice
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    InvoiceDate = reader.GetDateTime(2),
                    CustomerName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CustomerPhone = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Subtotal = reader.GetDecimal(5),
                    DiscountAmount = reader.GetDecimal(6),
                    TotalAmount = reader.GetDecimal(7),
                    TotalProfit = reader.GetDecimal(8),
                    Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CreatedDate = reader.GetDateTime(10)
                });
            }

            return invoices;
        }

        // Generate next invoice number
        public string GenerateInvoiceNumber()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM SalesInvoices";
            int count = Convert.ToInt32(command.ExecuteScalar());

            return $"INV-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
        }

        // Get total sales for date range
        public decimal GetTotalSales(DateTime startDate, DateTime endDate)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COALESCE(SUM(TotalAmount), 0)
                FROM SalesInvoices
                WHERE InvoiceDate BETWEEN @StartDate AND @EndDate";
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1));

            return Convert.ToDecimal(command.ExecuteScalar());
        }

        // Get total profit for date range
        public decimal GetTotalProfit(DateTime startDate, DateTime endDate)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COALESCE(SUM(TotalProfit), 0)
                FROM SalesInvoices
                WHERE InvoiceDate BETWEEN @StartDate AND @EndDate";
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1));

            return Convert.ToDecimal(command.ExecuteScalar());
        }

        // Delete sales invoice (with stock restoration)
        public void DeleteSalesInvoice(int invoiceId)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get invoice items first to restore stock
                var getItemsCmd = connection.CreateCommand();
                getItemsCmd.Transaction = transaction;
                getItemsCmd.CommandText = "SELECT ProductId, Quantity FROM SalesInvoiceItems WHERE InvoiceId = @InvoiceId";
                getItemsCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

                var itemsToRestore = new List<(int ProductId, int Quantity)>();
                using (var reader = getItemsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemsToRestore.Add((reader.GetInt32(0), reader.GetInt32(1)));
                    }
                }

                // Restore stock for each item
                foreach (var (productId, quantity) in itemsToRestore)
                {
                    var restoreCmd = connection.CreateCommand();
                    restoreCmd.Transaction = transaction;
                    restoreCmd.CommandText = @"
                        UPDATE Products 
                        SET CurrentStock = CurrentStock + @Quantity, 
                            UpdatedDate = @UpdatedDate
                        WHERE Id = @ProductId";
                    restoreCmd.Parameters.AddWithValue("@Quantity", quantity);
                    restoreCmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                    restoreCmd.Parameters.AddWithValue("@ProductId", productId);
                    restoreCmd.ExecuteNonQuery();
                }

                // Delete stock movements
                var deleteMovementsCmd = connection.CreateCommand();
                deleteMovementsCmd.Transaction = transaction;
                deleteMovementsCmd.CommandText = "DELETE FROM StockMovements WHERE ReferenceType = 'SALE' AND ReferenceId = @InvoiceId";
                deleteMovementsCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                deleteMovementsCmd.ExecuteNonQuery();

                // Delete invoice items
                var deleteItemsCmd = connection.CreateCommand();
                deleteItemsCmd.Transaction = transaction;
                deleteItemsCmd.CommandText = "DELETE FROM SalesInvoiceItems WHERE InvoiceId = @InvoiceId";
                deleteItemsCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                deleteItemsCmd.ExecuteNonQuery();

                // Delete invoice
                var deleteInvoiceCmd = connection.CreateCommand();
                deleteInvoiceCmd.Transaction = transaction;
                deleteInvoiceCmd.CommandText = "DELETE FROM SalesInvoices WHERE Id = @InvoiceId";
                deleteInvoiceCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                deleteInvoiceCmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
