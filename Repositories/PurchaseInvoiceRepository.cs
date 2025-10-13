using ChicoDesktopApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ChicoDesktopApp.Repositories
{
    public class PurchaseInvoiceRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public PurchaseInvoiceRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Add new purchase invoice with items
        public int AddPurchaseInvoice(PurchaseInvoice invoice)
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
                    INSERT INTO PurchaseInvoices (InvoiceNumber, InvoiceDate, SupplierName, SupplierPhone, 
                                                   TotalAmount, Notes, CreatedDate)
                    VALUES (@InvoiceNumber, @InvoiceDate, @SupplierName, @SupplierPhone, 
                            @TotalAmount, @Notes, @CreatedDate);
                    SELECT last_insert_rowid();";

                invoiceCmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                invoiceCmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                invoiceCmd.Parameters.AddWithValue("@SupplierName", invoice.SupplierName ?? (object)DBNull.Value);
                invoiceCmd.Parameters.AddWithValue("@SupplierPhone", invoice.SupplierPhone ?? (object)DBNull.Value);
                invoiceCmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
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
                        INSERT INTO PurchaseInvoiceItems (InvoiceId, ProductId, ProductName, Quantity, UnitCost, TotalCost)
                        VALUES (@InvoiceId, @ProductId, @ProductName, @Quantity, @UnitCost, @TotalCost)";

                    itemCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@UnitCost", item.UnitCost);
                    itemCmd.Parameters.AddWithValue("@TotalCost", item.TotalCost);
                    itemCmd.ExecuteNonQuery();

                    // Update product stock (increment) and update purchase price
                    var stockCmd = connection.CreateCommand();
                    stockCmd.Transaction = transaction;
                    stockCmd.CommandText = @"
                        UPDATE Products 
                        SET CurrentStock = CurrentStock + @Quantity,
                            PurchasePrice = @UnitCost,
                            UpdatedDate = @UpdatedDate
                        WHERE Id = @ProductId";

                    stockCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@UnitCost", item.UnitCost);
                    stockCmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                    stockCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    stockCmd.ExecuteNonQuery();

                    // Insert stock movement record
                    var movementCmd = connection.CreateCommand();
                    movementCmd.Transaction = transaction;
                    movementCmd.CommandText = @"
                        INSERT INTO StockMovements (ProductId, MovementType, Quantity, ReferenceType, ReferenceId, MovementDate, Notes)
                        VALUES (@ProductId, 'IN', @Quantity, 'PURCHASE', @InvoiceId, @MovementDate, @Notes)";

                    movementCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    movementCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    movementCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    movementCmd.Parameters.AddWithValue("@MovementDate", invoice.InvoiceDate);
                    movementCmd.Parameters.AddWithValue("@Notes", $"شراء فاتورة رقم {invoice.InvoiceNumber}");
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

        // Get all purchase invoices
        public List<PurchaseInvoice> GetAllPurchaseInvoices()
        {
            var invoices = new List<PurchaseInvoice>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, InvoiceDate, SupplierName, SupplierPhone, 
                       TotalAmount, Notes, CreatedDate
                FROM PurchaseInvoices
                ORDER BY InvoiceDate DESC, Id DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoices.Add(new PurchaseInvoice
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    InvoiceDate = reader.GetDateTime(2),
                    SupplierName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    SupplierPhone = reader.IsDBNull(4) ? null : reader.GetString(4),
                    TotalAmount = reader.GetDecimal(5),
                    Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
                    CreatedDate = reader.GetDateTime(7)
                });
            }

            return invoices;
        }

        // Get purchase invoice by ID with items
        public PurchaseInvoice? GetPurchaseInvoiceById(int id)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, InvoiceDate, SupplierName, SupplierPhone, 
                       TotalAmount, Notes, CreatedDate
                FROM PurchaseInvoices
                WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            PurchaseInvoice? invoice = null;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    invoice = new PurchaseInvoice
                    {
                        Id = reader.GetInt32(0),
                        InvoiceNumber = reader.GetString(1),
                        InvoiceDate = reader.GetDateTime(2),
                        SupplierName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        SupplierPhone = reader.IsDBNull(4) ? null : reader.GetString(4),
                        TotalAmount = reader.GetDecimal(5),
                        Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
                        CreatedDate = reader.GetDateTime(7)
                    };
                }
            }

            if (invoice != null)
            {
                // Get invoice items
                var itemsCmd = connection.CreateCommand();
                itemsCmd.CommandText = @"
                    SELECT Id, InvoiceId, ProductId, ProductName, Quantity, UnitCost, TotalCost
                    FROM PurchaseInvoiceItems
                    WHERE InvoiceId = @InvoiceId";
                itemsCmd.Parameters.AddWithValue("@InvoiceId", id);

                using var itemsReader = itemsCmd.ExecuteReader();
                while (itemsReader.Read())
                {
                    invoice.Items.Add(new PurchaseInvoiceItem
                    {
                        Id = itemsReader.GetInt32(0),
                        InvoiceId = itemsReader.GetInt32(1),
                        ProductId = itemsReader.GetInt32(2),
                        ProductName = itemsReader.GetString(3),
                        Quantity = itemsReader.GetInt32(4),
                        UnitCost = itemsReader.GetDecimal(5),
                        TotalCost = itemsReader.GetDecimal(6)
                    });
                }
            }

            return invoice;
        }

        // Get purchase invoices by date range
        public List<PurchaseInvoice> GetPurchaseInvoicesByDateRange(DateTime startDate, DateTime endDate)
        {
            var invoices = new List<PurchaseInvoice>();
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, InvoiceNumber, InvoiceDate, SupplierName, SupplierPhone, 
                       TotalAmount, Notes, CreatedDate
                FROM PurchaseInvoices
                WHERE InvoiceDate BETWEEN @StartDate AND @EndDate
                ORDER BY InvoiceDate DESC, Id DESC";
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoices.Add(new PurchaseInvoice
                {
                    Id = reader.GetInt32(0),
                    InvoiceNumber = reader.GetString(1),
                    InvoiceDate = reader.GetDateTime(2),
                    SupplierName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    SupplierPhone = reader.IsDBNull(4) ? null : reader.GetString(4),
                    TotalAmount = reader.GetDecimal(5),
                    Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
                    CreatedDate = reader.GetDateTime(7)
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
            command.CommandText = "SELECT COUNT(*) FROM PurchaseInvoices";
            int count = Convert.ToInt32(command.ExecuteScalar());

            return $"PUR-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
        }

        // Get total purchases for date range
        public decimal GetTotalPurchases(DateTime startDate, DateTime endDate)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COALESCE(SUM(TotalAmount), 0)
                FROM PurchaseInvoices
                WHERE InvoiceDate BETWEEN @StartDate AND @EndDate";
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1));

            return Convert.ToDecimal(command.ExecuteScalar());
        }

        // Delete purchase invoice (with stock deduction)
        public void DeletePurchaseInvoice(int invoiceId)
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get invoice items first to deduct stock
                var getItemsCmd = connection.CreateCommand();
                getItemsCmd.Transaction = transaction;
                getItemsCmd.CommandText = "SELECT ProductId, Quantity FROM PurchaseInvoiceItems WHERE InvoiceId = @InvoiceId";
                getItemsCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

                var itemsToDeduct = new List<(int ProductId, int Quantity)>();
                using (var reader = getItemsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemsToDeduct.Add((reader.GetInt32(0), reader.GetInt32(1)));
                    }
                }

                // Deduct stock for each item
                foreach (var (productId, quantity) in itemsToDeduct)
                {
                    var deductCmd = connection.CreateCommand();
                    deductCmd.Transaction = transaction;
                    deductCmd.CommandText = @"
                        UPDATE Products 
                        SET CurrentStock = CurrentStock - @Quantity, 
                            UpdatedDate = @UpdatedDate
                        WHERE Id = @ProductId";
                    deductCmd.Parameters.AddWithValue("@Quantity", quantity);
                    deductCmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                    deductCmd.Parameters.AddWithValue("@ProductId", productId);
                    deductCmd.ExecuteNonQuery();
                }

                // Delete stock movements
                var deleteMovementsCmd = connection.CreateCommand();
                deleteMovementsCmd.Transaction = transaction;
                deleteMovementsCmd.CommandText = "DELETE FROM StockMovements WHERE ReferenceType = 'PURCHASE' AND ReferenceId = @InvoiceId";
                deleteMovementsCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                deleteMovementsCmd.ExecuteNonQuery();

                // Delete invoice items
                var deleteItemsCmd = connection.CreateCommand();
                deleteItemsCmd.Transaction = transaction;
                deleteItemsCmd.CommandText = "DELETE FROM PurchaseInvoiceItems WHERE InvoiceId = @InvoiceId";
                deleteItemsCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                deleteItemsCmd.ExecuteNonQuery();

                // Delete invoice
                var deleteInvoiceCmd = connection.CreateCommand();
                deleteInvoiceCmd.Transaction = transaction;
                deleteInvoiceCmd.CommandText = "DELETE FROM PurchaseInvoices WHERE Id = @InvoiceId";
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
