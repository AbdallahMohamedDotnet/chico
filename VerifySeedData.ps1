# Verify Seed Data - Check if products were inserted
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   🔍 SEED DATA VERIFICATION                       ║" -ForegroundColor Cyan
Write-Host "║   Checking Database Contents                       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$dbPath = ".\bin\Debug\net8.0-windows\Data\chico.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "❌ Database not found at: $dbPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run the application first to create the database." -ForegroundColor Yellow
    exit 1
}

try {
    # Load the SQLite assembly from the application bin folder
    $sqliteAssembly = ".\bin\Debug\net8.0-windows\Microsoft.Data.Sqlite.dll"
    
    if (Test-Path $sqliteAssembly) {
        Add-Type -Path $sqliteAssembly
        
        $connectionString = "Data Source=$dbPath"
        $connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)
        $connection.Open()
        
        # Get product count
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM Products"
        $productCount = $cmd.ExecuteScalar()
        
        Write-Host "📦 Products Found: $productCount" -ForegroundColor $(if ($productCount -eq 35) { "Green" } else { "Yellow" })
        
        if ($productCount -eq 35) {
            Write-Host "   ✅ All 35 seed products are in the database!" -ForegroundColor Green
        } elseif ($productCount -eq 0) {
            Write-Host "   ⚠️  No products found - seed data may not have run" -ForegroundColor Red
        } else {
            Write-Host "   ℹ️  Unexpected count (expected 35)" -ForegroundColor Yellow
        }
        
        Write-Host ""
        
        # Get category breakdown
        Write-Host "📊 Products by Category:" -ForegroundColor Cyan
        $cmd.CommandText = @"
SELECT c.CategoryName, COUNT(p.ProductId) as Count
FROM Categories c
LEFT JOIN Products p ON c.CategoryId = p.CategoryId
GROUP BY c.CategoryId, c.CategoryName
ORDER BY c.CategoryName
"@
        
        $reader = $cmd.ExecuteReader()
        while ($reader.Read()) {
            $catName = $reader["CategoryName"]
            $count = $reader["Count"]
            $bar = "█" * $count
            Write-Host "   $catName : $count $bar" -ForegroundColor Gray
        }
        $reader.Close()
        
        Write-Host ""
        
        # Get total inventory value
        $cmd.CommandText = @"
SELECT 
    ROUND(SUM(PurchasePrice * CurrentStock), 2) as PurchaseValue,
    ROUND(SUM(SalePrice * CurrentStock), 2) as SaleValue
FROM Products
"@
        
        $reader = $cmd.ExecuteReader()
        if ($reader.Read()) {
            $purchaseValue = $reader["PurchaseValue"]
            $saleValue = $reader["SaleValue"]
            $profit = $saleValue - $purchaseValue
            
            Write-Host "💰 Inventory Value:" -ForegroundColor Cyan
            Write-Host "   Purchase Value: $purchaseValue SAR" -ForegroundColor White
            Write-Host "   Retail Value: $saleValue SAR" -ForegroundColor White
            Write-Host "   Potential Profit: $([math]::Round($profit, 2)) SAR" -ForegroundColor Green
        }
        $reader.Close()
        
        Write-Host ""
        
        # Show sample products
        Write-Host "🎯 Sample Products:" -ForegroundColor Cyan
        $cmd.CommandText = @"
SELECT ProductName, Barcode, SalePrice, CurrentStock
FROM Products
ORDER BY RANDOM()
LIMIT 5
"@
        
        $reader = $cmd.ExecuteReader()
        while ($reader.Read()) {
            $name = $reader["ProductName"]
            $barcode = $reader["Barcode"]
            $price = $reader["SalePrice"]
            $stock = $reader["CurrentStock"]
            Write-Host "   📦 $name ($barcode) - $price SAR - Stock: $stock" -ForegroundColor Gray
        }
        $reader.Close()
        
        $connection.Close()
        
        Write-Host ""
        Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║   ✅ VERIFICATION COMPLETE                        ║" -ForegroundColor Green
        Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-Host ""
        Write-Host "💡 To view products in the app:" -ForegroundColor Cyan
        Write-Host "   1. Login (admin/admin123)" -ForegroundColor Gray
        Write-Host "   2. Click '📦 المنتجات' button" -ForegroundColor Gray
        Write-Host "   3. All 35 products will be displayed" -ForegroundColor Gray
        
    } else {
        Write-Host "⚠️  Microsoft.Data.Sqlite.dll not found" -ForegroundColor Yellow
        Write-Host "   Expected at: $sqliteAssembly" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Alternative verification:" -ForegroundColor Cyan
        Write-Host "   1. Open the application" -ForegroundColor Gray
        Write-Host "   2. Login with admin/admin123" -ForegroundColor Gray
        Write-Host "   3. Click '📦 المنتجات' to view products" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "❌ Error accessing database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please verify the seed data through the application UI:" -ForegroundColor Yellow
    Write-Host "   1. Launch the application" -ForegroundColor Gray
    Write-Host "   2. Login with admin/admin123" -ForegroundColor Gray
    Write-Host "   3. Click '📦 المنتجات' button" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
