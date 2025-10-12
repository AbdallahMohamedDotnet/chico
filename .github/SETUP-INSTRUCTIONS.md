# .NET SDK Installation Instructions

## Steps to Install .NET SDK

### For Windows:

1. **Download .NET SDK**
   - The download page has been opened in the Simple Browser
   - Download the latest **.NET SDK** (not just the runtime)
   - Recommended: .NET 8.0 or later for desktop development

2. **Run the Installer**
   - Run the downloaded installer (.exe file)
   - Follow the installation wizard
   - Default settings are recommended

3. **Verify Installation**
   - Open a **new PowerShell terminal** (important: must be new)
   - Run: `dotnet --version`
   - You should see the version number (e.g., 8.0.xxx)

4. **Restart VS Code**
   - Close and reopen VS Code to ensure it picks up the new PATH changes

### After Installation:

Once the .NET SDK is installed and verified, I can:
- Scaffold the Windows Forms desktop application
- Add SQLite3 database integration
- Set up the project structure
- Configure build and run tasks

### Troubleshooting:

If `dotnet --version` doesn't work after installation:
1. Close ALL PowerShell terminals
2. Close VS Code completely
3. Reopen VS Code
4. Open a new terminal
5. Try `dotnet --version` again

---

**Once installed, let me know and I'll continue with the project setup!**
