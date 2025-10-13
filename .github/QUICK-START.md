# Chico ERP - Quick Start Guide

## 🎉 What's Ready

Your Chico ERP system has a **complete and tested backend infrastructure** ready for UI development!

### ✅ Completed Components

1. **Database Schema**: All tables created with proper relationships
2. **Data Models**: Product, Category, Invoice entities with computed properties
3. **Repository Layer**: Full CRUD operations for products and categories
4. **Stock Management**: Automatic inventory tracking with audit trail
5. **Invoice Generation**: Unique invoice number system (SALE-2025-00001)

### 🗄️ Database Location

```
E:\chico\bin\Debug\net8.0-windows\Data\chico.db
```

The database is automatically created with:
- 8 default product categories (in English and Arabic)
- All required tables and relationships
- Indexes for fast searching

---

## 🚀 How to Run

### Method 1: Visual Studio Code
```powershell
# Press F5 in VS Code
```

### Method 2: Command Line
```powershell
# Set PATH and run
$env:Path = "$env:LOCALAPPDATA\Microsoft\dotnet;" + $env:Path
dotnet run
```

### Method 3: Direct Executable
```powershell
.\bin\Debug\net8.0-windows\ChicoDesktopApp.exe
```

---

## 📖 Using the Repository Layer

### Example: Add a New Product

```csharp
var product = new Product
{
    ProductName = "iPhone 15 Pro",
    ProductNameArabic = "آيفون 15 برو",
    CategoryId = 1, // Mobiles
    SerialNumber = "IP15P-001",
    PurchasePrice = 800,
    SalePrice = 1000,
    CurrentStock = 10,
    MinimumStock = 3
};

int productId = _productRepository.AddProduct(product);
```

### Example: Search Products

```csharp
// Search by name or serial
var results = _productRepository.SearchProducts("iPhone");

// Get all products
var allProducts = _productRepository.GetAllProducts();

// Get low stock alerts
var lowStock = _productRepository.GetLowStockProducts();
```

### Example: Update Stock

```csharp
// Decrease stock (sale)
_productRepository.UpdateStock(productId, -2, "SALE", "Invoice", invoiceId);

// Increase stock (purchase)
_productRepository.UpdateStock(productId, 10, "PURCHASE", "Invoice", purchaseId);
```

---

## 🎨 Next Steps: UI Development

### Phase 1: Product Management UI (Recommended First)

**Forms to Create**:
1. **ProductListForm** - DataGridView showing all products with search
2. **AddProductForm** - Dialog for adding new products
3. **EditProductForm** - Dialog for editing existing products

**Features to Implement**:
- Category dropdown (populated from CategoryRepository)
- Stock level display with low stock highlighting
- Search functionality
- Add/Edit/Delete buttons

### Phase 2: Sales Invoice UI

**Forms to Create**:
1. **SalesInvoiceForm** - Main POS interface
2. **ProductSelectionDialog** - Quick product picker

**Features to Implement**:
- Product search and selection
- Quantity input
- Price display with profit margin
- Discount field
- Invoice total calculation
- Print preview

### Phase 3: Dashboard & Reports

**Forms to Create**:
1. **MainDashboardForm** - Replace current Form1
2. **ProfitLossReportForm** - Financial reports

---

## 📚 Code Examples

### Get All Categories for Dropdown

```csharp
var categories = _categoryRepository.GetAllCategories();
comboBoxCategory.DataSource = categories;
comboBoxCategory.DisplayMember = "NameArabic"; // or "Name" for English
comboBoxCategory.ValueMember = "Id";
```

### Check Product Stock Before Sale

```csharp
var product = _productRepository.GetProductById(productId);
if (product.CurrentStock < quantityToSell)
{
    MessageBox.Show("المخزون غير كافٍ!", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}
```

### Generate Invoice Number

```csharp
string invoiceNumber = _dbHelper.GenerateInvoiceNumber("SALE");
// Result: SALE-2025-00001
```

---

## 🔧 Database Schema Reference

### Key Tables

**Products**
- Id, ProductName, ProductNameArabic
- CategoryId, SerialNumber, Barcode
- PurchasePrice, SalePrice
- CurrentStock, MinimumStock

**SalesInvoices**
- Id, InvoiceNumber, InvoiceDate
- CustomerName, CustomerPhone
- Subtotal, DiscountAmount, TotalAmount, TotalProfit

**PurchaseInvoices**
- Id, InvoiceNumber, InvoiceDate
- SupplierName, SupplierPhone
- TotalAmount

**StockMovements** (Audit Trail)
- ProductId, MovementType, Quantity
- ReferenceType, ReferenceId, MovementDate

---

## 🎯 PRD Requirements Status

| Module | Backend | UI | Status |
|--------|---------|----|----|
| Product Management | ✅ | ⏳ | Backend Ready |
| Stock Updates | ✅ | ⏳ | Automatic |
| Low Stock Alerts | ✅ | ⏳ | Backend Ready |
| Sales Invoices | ✅ | ⏳ | Backend Ready |
| Purchase Invoices | ✅ | ⏳ | Backend Ready |
| P&L Reports | ✅ | ⏳ | Backend Ready |

---

## 📞 Current Application State

When you run the application:
- ✅ Database is created automatically
- ✅ 8 categories are seeded
- ✅ Window opens with title showing DB path
- ✅ Debug output shows product counts
- ⏳ UI shows empty form (ready for customization)

---

## 💡 Tips for UI Development

1. **Use DataGridView** for product lists - easy sorting and filtering
2. **Use ComboBox** for category selection
3. **Validate input** before calling repository methods
4. **Use transactions** for invoice creation (invoice + items + stock update)
5. **Show confirmation dialogs** for delete operations
6. **Implement print preview** for invoices
7. **Add loading indicators** for long operations

---

## 🐛 Debugging

The application writes debug information to the Output window:
- Total products count
- Low stock alerts
- Product details

Check the Output window in VS Code when running in debug mode (F5).

---

## 📄 Additional Documentation

- **README.md** - Project overview and feature list
- **DEVELOPMENT-PROGRESS.md** - Detailed progress report
- **copilot-instructions.md** - Project guidelines

---

**Status**: ✅ Backend Complete | 🔄 Ready for UI Development  
**Next Recommended Task**: Create ProductListForm with DataGridView
