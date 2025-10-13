# View Seed Data Script - Display all products in the database
# This script queries the database and shows the seeded products in a nice format

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   📊 DATABASE SEED DATA VIEWER                    ║" -ForegroundColor Cyan
Write-Host "║   Chico Desktop Application                        ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check if SQLite is available
$sqliteCmd = Get-Command sqlite3 -ErrorAction SilentlyContinue

if (-not $sqliteCmd) {
    Write-Host "⚠️  SQLite command line tool not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To install SQLite:" -ForegroundColor Yellow
    Write-Host "1. Download from: https://www.sqlite.org/download.html" -ForegroundColor Gray
    Write-Host "2. Or use: winget install SQLite.SQLite" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Alternative: View products through the application UI" -ForegroundColor Cyan
    Write-Host "   • Launch ChicoDesktopApp.exe" -ForegroundColor Gray
    Write-Host "   • Click '📦 المنتجات' button" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor DarkGray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

# Check if database exists
$dbPath = ".\bin\Debug\net8.0-windows\Data\chico.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "⚠️  Database not found at: $dbPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "To create the database:" -ForegroundColor Yellow
    Write-Host "1. Build the project: dotnet build" -ForegroundColor Gray
    Write-Host "2. Run the application once" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor DarkGray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

# Get product count
Write-Host "🔍 Querying database..." -ForegroundColor Yellow
$productCount = sqlite3 $dbPath "SELECT COUNT(*) FROM Products;"
Write-Host "   ✅ Found $productCount products" -ForegroundColor Green
Write-Host ""

if ($productCount -eq 0) {
    Write-Host "📦 No products found in database" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "The database is empty. To seed it:" -ForegroundColor Cyan
    Write-Host "1. Run the application (it will auto-seed)" -ForegroundColor Gray
    Write-Host "2. Or run: .\ResetDatabase.ps1" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor DarkGray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

# Display products by category
Write-Host "📊 PRODUCTS BY CATEGORY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

# Get categories
$categories = sqlite3 $dbPath "SELECT CategoryId, CategoryName FROM Categories ORDER BY CategoryId;" -separator "|"

foreach ($catLine in $categories) {
    $parts = $catLine.Split("|")
    $categoryId = $parts[0]
    $categoryName = $parts[1]
    
    Write-Host "🏷️  $categoryName" -ForegroundColor Yellow
    Write-Host "   ─────────────────────────────────────────────" -ForegroundColor DarkGray
    
    # Get products in this category
    $products = sqlite3 $dbPath @"
SELECT 
    ProductName,
    Barcode,
    PurchasePrice,
    SalePrice,
    CurrentStock,
    MinimumStock
FROM Products 
WHERE CategoryId = $categoryId
ORDER BY ProductName;
"@ -separator "|"

    if ($products) {
        foreach ($prodLine in $products) {
            $pparts = $prodLine.Split("|")
            $name = $pparts[0]
            $barcode = $pparts[1]
            $purchase = $pparts[2]
            $sale = $pparts[3]
            $stock = $pparts[4]
            $minStock = $pparts[5]
            
            # Check if low stock
            $stockStatus = ""
            if ([int]$stock -le [int]$minStock) {
                $stockStatus = " ⚠️ LOW STOCK"
                Write-Host "   📦 $name" -ForegroundColor Red
            } else {
                Write-Host "   📦 $name" -ForegroundColor White
            }
            
            Write-Host "      Code: $barcode | Buy: $purchase SAR | Sell: $sale SAR | Stock: $stock/$minStock$stockStatus" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
}

# Display summary statistics
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "📈 INVENTORY SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

# Total products
Write-Host "   Total Products: $productCount" -ForegroundColor White

# Total inventory value
$totalPurchaseValue = sqlite3 $dbPath "SELECT ROUND(SUM(PurchasePrice * CurrentStock), 2) FROM Products;"
Write-Host "   Purchase Value: $totalPurchaseValue SAR" -ForegroundColor White

$totalSaleValue = sqlite3 $dbPath "SELECT ROUND(SUM(SalePrice * CurrentStock), 2) FROM Products;"
Write-Host "   Retail Value: $totalSaleValue SAR" -ForegroundColor White

$potentialProfit = [decimal]$totalSaleValue - [decimal]$totalPurchaseValue
Write-Host "   Potential Profit: $([math]::Round($potentialProfit, 2)) SAR" -ForegroundColor Green

# Low stock items
$lowStockCount = sqlite3 $dbPath "SELECT COUNT(*) FROM Products WHERE CurrentStock <= MinimumStock;"
if ([int]$lowStockCount -gt 0) {
    Write-Host "   ⚠️  Low Stock Items: $lowStockCount" -ForegroundColor Red
} else {
    Write-Host "   ✅ Low Stock Items: 0" -ForegroundColor Green
}

# Products by category
Write-Host ""
Write-Host "   Products by Category:" -ForegroundColor White
$categoryStats = sqlite3 $dbPath @"
SELECT 
    c.CategoryName,
    COUNT(p.ProductId) as ProductCount
FROM Categories c
LEFT JOIN Products p ON c.CategoryId = p.CategoryId
GROUP BY c.CategoryId, c.CategoryName
ORDER BY ProductCount DESC;
"@ -separator "|"

foreach ($statLine in $categoryStats) {
    $parts = $statLine.Split("|")
    $catName = $parts[0]
    $count = $parts[1]
    
    $barChart = "█" * [int]$count
    Write-Host "      $catName`: $count $barChart" -ForegroundColor Gray
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""
Write-Host "💡 TIP: Use the application UI for detailed product management" -ForegroundColor Cyan
Write-Host "   Launch app → Click '📦 المنتجات' → View/Edit/Delete products" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
