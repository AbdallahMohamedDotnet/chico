# Product Management UI Implementation Plan

## Current Status
✅ Database schema complete
✅ Product model defined (`Models/Product.cs`)
✅ ProductRepository complete with all CRUD methods
✅ CategoryRepository complete with GetAll and GetById methods
❌ ProductListForm and ProductEditForm - Deleted (had incorrect property mappings)

## Product Model Property Reference

The existing `Product` model uses these property names:

```csharp
public class Product
{
    public int Id { get; set; }
    public string ProductName { get; set; }              // English name
    public string? ProductNameArabic { get; set; }       // Arabic name
    public int CategoryId { get; set; }
    public string? SerialNumber { get; set; }
    public string? Barcode { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CurrentStock { get; set; }                // NOT "Quantity"
    public int MinimumStock { get; set; }                // NOT "ReorderLevel"
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get. set; }
    
    // Computed
    public bool IsLowStock => CurrentStock <= MinimumStock;
    public decimal ProfitPercentage => ...;
}
```

## Repository Methods Available

### ProductRepository
- ✅ `int AddProduct(Product product)`
- ✅ `void UpdateProduct(Product product)`
- ✅ `void DeleteProduct(int productId)`
- ✅ `List<Product> GetAllProducts()`
- ✅ `Product? GetProductById(int id)`
- ✅ `List<Product> SearchProducts(string searchTerm)`
- ✅ `List<Product> GetLowStockProducts()`

### CategoryRepository
- ✅ `List<Category> GetAllCategories()`
- ✅ `Category? GetCategoryById(int id)`
- ✅ `int AddCategory(Category category)`

## Next Steps

Since the previous forms had 20+ compilation errors due to property name mismatches, I recommend one of two approaches:

### Option A: I Create Corrected Forms (Recommended - Fast)
I'll create two new files with correct property mappings:
1. `Forms/ProductListForm.cs` - List all products with search, filter, add/edit/delete buttons
2. `Forms/ProductEditForm.cs` - Add/Edit product dialog

**Estimated Time:** 5 minutes
**Risk:** Low - I now have exact model structure

### Option B: You Provide A Simple Example (If Preferred)
If you'd prefer to control the UI style, you could:
1. Show me an example of how you want one form structured
2. I'll follow that pattern for all forms

## Recommended Action

**I recommend Option A** - Let me create the corrected forms now. They will:
- Use correct property names (ProductName, CurrentStock, MinimumStock, etc.)
- Match the Arabic RTL styling from MainDashboard
- Have full CRUD functionality
- Include low stock alerting
- Work with existing repositories without any changes

**Your approval to proceed:** Should I create the corrected `ProductListForm.cs` and `ProductEditForm.cs` now with proper property mappings?
