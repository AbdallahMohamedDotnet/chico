# Chico ERP Desktop Application Launcher
# This script builds and runs the Chico ERP application

# Set up PATH for .NET SDK
$env:Path = "$env:LOCALAPPDATA\Microsoft\dotnet;" + $env:Path

# Clear console
Clear-Host

# Display header
Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     🏪 CHICO ERP DESKTOP APPLICATION 🏪         ║" -ForegroundColor Yellow
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "🔍 Checking system requirements..." -ForegroundColor White
Write-Host "   .NET SDK Version: " -NoNewline -ForegroundColor Gray
$dotnetVersion = dotnet --version
Write-Host $dotnetVersion -ForegroundColor Green

# Build project
Write-Host ""
Write-Host "🔨 Building project..." -ForegroundColor Yellow
$buildOutput = dotnet build --configuration Debug --nologo -v minimal 2>&1
$buildSuccess = $LASTEXITCODE -eq 0

if ($buildSuccess) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    
    # Check for warnings
    $warnings = $buildOutput | Select-String -Pattern "warning"
    if ($warnings) {
        Write-Host "⚠️  Build warnings:" -ForegroundColor DarkYellow
        $warnings | ForEach-Object { Write-Host "   $_" -ForegroundColor DarkYellow }
    }
} else {
    Write-Host "❌ Build failed! Errors:" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Verify executable exists
Write-Host ""
Write-Host "🔍 Verifying executable..." -ForegroundColor Yellow
$exePath = "bin\Debug\net8.0-windows\ChicoDesktopApp.exe"
if (Test-Path $exePath) {
    Write-Host "✅ Executable ready: $exePath" -ForegroundColor Green
} else {
    Write-Host "❌ Executable not found at: $exePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Display login info
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║            📝 LOGIN CREDENTIALS                  ║" -ForegroundColor White
Write-Host "╠══════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║   👤 Username: admin                             ║" -ForegroundColor Green
Write-Host "║   🔑 Password: admin123                          ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Launch application
Write-Host "🚀 Launching Chico ERP..." -ForegroundColor Yellow
Write-Host "💡 Look for the login window on your screen!" -ForegroundColor Cyan
Write-Host ""

# Start the application WITHOUT waiting (so window stays open)
try {
    $process = Start-Process -FilePath $exePath -PassThru
    
    Write-Host "✅ Application started! (Process ID: $($process.Id))" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎯 The login window should now be visible." -ForegroundColor Cyan
    Write-Host "   If not, check your taskbar or press Alt+Tab" -ForegroundColor Gray
    Write-Host ""
    Write-Host "💡 You can close this window - the app will keep running." -ForegroundColor Yellow
    Write-Host "   To close the app, close the login/dashboard window." -ForegroundColor Yellow
} catch {
    Write-Host "❌ Error launching application: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "👋 Thank you for using Chico ERP!" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
