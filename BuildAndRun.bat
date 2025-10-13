@echo off
echo ========================================
echo Building Chico Desktop Application...
echo ========================================
"C:\Program Files\dotnet\dotnet.exe" build --nologo

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build successful! Starting application...
    echo Seed data will be applied automatically!
    echo ========================================
    echo.
    start "" ".\bin\Debug\net8.0-windows\ChicoDesktopApp.exe"
) else (
    echo.
    echo ========================================
    echo Build failed! Please check errors above.
    echo ========================================
    pause
)
