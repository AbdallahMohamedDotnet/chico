namespace ChicoDesktopApp.Models
{
    public class SalesInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalProfit { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation property
        public List<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    }

    public class SalesInvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Profit { get; set; }
    }
}
