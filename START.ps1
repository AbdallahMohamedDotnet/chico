# Quick Start - Chico ERP
# Just run this file to launch the application!

$env:Path = "$env:LOCALAPPDATA\Microsoft\dotnet;" + $env:Path

Clear-Host
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "         🏪 CHICO ERP DESKTOP APPLICATION       " -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check if executable exists
$exePath = "bin\Debug\net8.0-windows\ChicoDesktopApp.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "⚠️  Building application for first time..." -ForegroundColor Yellow
    Write-Host ""
    dotnet build --nologo -v quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed! Please check for errors." -ForegroundColor Red
        Write-Host ""
        Write-Host "Press any key to exit..." -ForegroundColor Gray
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        exit 1
    }
    Write-Host "✅ Build complete!" -ForegroundColor Green
    Write-Host ""
}

# Display login info
Write-Host "📝 Login Credentials:" -ForegroundColor White
Write-Host "   👤 Username: " -NoNewline -ForegroundColor Gray
Write-Host "admin" -ForegroundColor Green
Write-Host "   🔑 Password: " -NoNewline -ForegroundColor Gray
Write-Host "admin123" -ForegroundColor Green
Write-Host ""
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Launch application
Write-Host "🚀 Launching Chico ERP..." -ForegroundColor Yellow
Start-Process -FilePath $exePath

Start-Sleep -Milliseconds 800

Write-Host "✅ Application started!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Look for the login window on your screen." -ForegroundColor Cyan
Write-Host "   (Check taskbar if not visible)" -ForegroundColor Gray
Write-Host ""
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now close this window." -ForegroundColor DarkGray
Write-Host "The application will continue running." -ForegroundColor DarkGray
Write-Host ""
