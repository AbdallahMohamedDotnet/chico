namespace ChicoDesktopApp.Models
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation property
        public List<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    }

    public class PurchaseInvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}
