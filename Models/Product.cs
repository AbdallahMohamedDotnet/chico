namespace ChicoDesktopApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductNameArabic { get; set; }
        public int CategoryId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Barcode { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; } = 5;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation property
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryNameArabic { get; set; }

        // Computed properties
        public decimal ProfitMargin => SalePrice - PurchasePrice;
        public decimal ProfitPercentage => PurchasePrice > 0 ? ((SalePrice - PurchasePrice) / PurchasePrice * 100) : 0;
        public bool IsLowStock => CurrentStock <= MinimumStock;
    }
}
