# Reset and Reseed Database Script for Chico Desktop Application
# This script stops the application, deletes the database, and restarts with fresh seed data

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   🔄 DATABASE RESET & RESEED UTILITY              ║" -ForegroundColor Cyan
Write-Host "║   Chico Desktop Application                        ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Stop the application
Write-Host "🛑 Step 1: Stopping application..." -ForegroundColor Yellow
$processes = Get-Process -Name "ChicoDesktopApp" -ErrorAction SilentlyContinue
if ($processes) {
    Stop-Process -Name "ChicoDesktopApp" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "   ✅ Application stopped successfully!" -ForegroundColor Green
} else {
    Write-Host "   ℹ️  Application is not running" -ForegroundColor Gray
}

# Step 2: Delete database file
Write-Host ""
Write-Host "🗑️  Step 2: Deleting database..." -ForegroundColor Yellow
$dbPath = ".\bin\Debug\net8.0-windows\Data\chico.db"
$absolutePath = Resolve-Path $dbPath -ErrorAction SilentlyContinue

if (Test-Path $dbPath) {
    $fileSize = (Get-Item $dbPath).Length / 1KB
    Write-Host "   📊 Current database size: $([math]::Round($fileSize, 2)) KB" -ForegroundColor Gray
    
    Remove-Item $dbPath -Force
    Write-Host "   ✅ Database deleted successfully!" -ForegroundColor Green
    Write-Host "   📍 Location: $dbPath" -ForegroundColor Gray
} else {
    Write-Host "   ⚠️  Database not found at: $dbPath" -ForegroundColor Red
    Write-Host "   ℹ️  This is normal if the application hasn't been built yet" -ForegroundColor Gray
}

# Step 3: Delete temporary files (optional)
Write-Host ""
Write-Host "🧹 Step 3: Cleaning temporary files..." -ForegroundColor Yellow
$tempFiles = @(
    ".\bin\Debug\net8.0-windows\Data\chico.db-shm",
    ".\bin\Debug\net8.0-windows\Data\chico.db-wal"
)

$cleanedCount = 0
foreach ($file in $tempFiles) {
    if (Test-Path $file) {
        Remove-Item $file -Force
        $cleanedCount++
    }
}

if ($cleanedCount -gt 0) {
    Write-Host "   ✅ Cleaned $cleanedCount temporary file(s)" -ForegroundColor Green
} else {
    Write-Host "   ℹ️  No temporary files to clean" -ForegroundColor Gray
}

# Step 4: Restart application
Write-Host ""
Write-Host "🚀 Step 4: Starting application..." -ForegroundColor Yellow
$exePath = ".\bin\Debug\net8.0-windows\ChicoDesktopApp.exe"

if (Test-Path $exePath) {
    Start-Process -FilePath $exePath
    Write-Host "   ✅ Application started successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║   🎉 RESET COMPLETE!                              ║" -ForegroundColor Green
    Write-Host "║                                                    ║" -ForegroundColor Green
    Write-Host "║   The application will:                            ║" -ForegroundColor Green
    Write-Host "║   1. Create a fresh database                       ║" -ForegroundColor Green
    Write-Host "║   2. Initialize all tables                         ║" -ForegroundColor Green
    Write-Host "║   3. Insert 35 sample products                     ║" -ForegroundColor Green
    Write-Host "║   4. Create admin user (admin/admin123)            ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "📦 Seed Data Includes:" -ForegroundColor Cyan
    Write-Host "   • 8 Electronics items (Laptops, Mouse, Keyboard...)" -ForegroundColor Gray
    Write-Host "   • 7 Food items (Rice, Sugar, Coffee...)" -ForegroundColor Gray
    Write-Host "   • 5 Clothing items (T-Shirts, Jeans, Shoes...)" -ForegroundColor Gray
    Write-Host "   • 4 Books (Novels, Programming...)" -ForegroundColor Gray
    Write-Host "   • 5 Home items (Plates, Knives, Pots...)" -ForegroundColor Gray
    Write-Host "   • 5 Sports items (Football, Basketball...)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🔑 Login Credentials:" -ForegroundColor Cyan
    Write-Host "   Username: admin" -ForegroundColor Gray
    Write-Host "   Password: admin123" -ForegroundColor Gray
} else {
    Write-Host "   ⚠️  Application executable not found: $exePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 To build the application, run:" -ForegroundColor Yellow
    Write-Host "   dotnet build" -ForegroundColor White
    Write-Host ""
    Write-Host "   Then run this script again to reset and reseed the database." -ForegroundColor Gray
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
