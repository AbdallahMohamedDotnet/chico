# Chico Desktop Application - Copilot Instructions

## Project Overview
This is a .NET 8.0 Windows Forms desktop application with SQLite3 database integration.

## Technology Stack
- **Framework**: .NET 8.0
- **UI**: Windows Forms
- **Database**: SQLite3 (Microsoft.Data.Sqlite v9.0.9)
- **Language**: C#

## Project Structure
- `Data/` - SQLite database storage (chico.db)
- `DatabaseHelper.cs` - Database operations and initialization
- `Form1.cs` - Main application form
- `.vscode/` - VS Code tasks and launch configurations

## Development Guidelines

### Building
- Use `dotnet build` or press `Ctrl+Shift+B` in VS Code
- Build task is configured in `.vscode/tasks.json`

### Running
- Use `dotnet run` or press `F5` for debugging
- The database is auto-created in the Data folder on first run

### Database Operations
- Use the `DatabaseHelper` class for all database operations
- Connection string is automatically configured
- Sample table `AppData` is created on initialization

## Key Features
- Automatic database initialization
- Pre-configured SQLite integration
- Windows Forms UI ready for customization
- Debug and build configurations set up

## Next Steps for Development
1. Customize Form1 UI using the Windows Forms Designer
2. Add business logic to DatabaseHelper or create new helper classes
3. Implement CRUD operations for your data models
4. Add additional forms as needed

## Notes
- .NET SDK was installed locally using the official Microsoft installation script
- The C# extension for VS Code is installed for IntelliSense and debugging
- Database file location is shown in the form title bar
