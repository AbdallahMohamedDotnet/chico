@echo off
echo ========================================
echo Building Chico Desktop Application...
echo Using MSBuild (Visual Studio 2022)
echo ========================================
echo.

"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "ChicoDesktopApp.csproj" /p:Configuration=Debug /v:minimal /nologo

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo ✅ BUILD SUCCESSFUL!
    echo ========================================
    echo.
    echo Starting application...
    echo Seed data will be applied automatically on first run!
    echo.
    echo Login credentials:
    echo   Username: admin
    echo   Password: admin123
    echo.
    echo ========================================
    echo.
    timeout /t 2 /nobreak >nul
    start "" ".\bin\Debug\net8.0-windows\ChicoDesktopApp.exe"
    echo Application started! Check the window that just opened.
    echo.
) else (
    echo.
    echo ========================================
    echo ❌ BUILD FAILED!
    echo ========================================
    echo Please check the errors above.
    echo.
    pause
)
