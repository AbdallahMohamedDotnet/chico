# Chico Desktop Application

A .NET 8.0 Windows Forms desktop application with integrated SQLite3 database support.

## Features

- **Windows Forms UI** - Modern desktop interface built with .NET 8.0
- **SQLite3 Database** - Local database with automatic initialization
- **Database Helper Class** - Pre-configured database operations
- **Data Folder** - Organized database file storage

## Prerequisites

- .NET 8.0 SDK (installed via `dotnet-install.ps1`)
- Windows OS
- Visual Studio Code with C# extension

## Project Structure

```
chico/
├── Data/                    # SQLite database storage
│   └── chico.db            # Auto-created database file
├── DatabaseHelper.cs       # Database operations class
├── Form1.cs                # Main form code
├── Form1.Designer.cs       # Form designer code
├── Program.cs              # Application entry point
├── ChicoDesktopApp.csproj  # Project configuration
└── README.md               # This file
```

## Getting Started

### Build the Project

```powershell
dotnet build
```

### Run the Application

```powershell
dotnet run
```

Or use VS Code:
- Press `F5` to build and debug
- Press `Ctrl+Shift+B` to build only

## Database

The application uses SQLite3 with the following features:

- **Database Location**: `Data/chico.db`
- **Auto-initialization**: Database and tables created on first run
- **Sample Table**: `AppData` table with Id, Name, Description, CreatedDate fields

### DatabaseHelper Methods

```csharp
// Get database connection
SqliteConnection connection = _dbHelper.GetConnection();

// Add data
_dbHelper.AddData("Item Name", "Item Description");

// Get record count
long count = _dbHelper.GetDataCount();
```

## Development

### VS Code Tasks

- **Build**: `Ctrl+Shift+B` or run task "build"
- **Run**: Run task "run"

### Debug Configuration

Launch configuration is set up in `.vscode/launch.json` for debugging.

## NuGet Packages

- `Microsoft.Data.Sqlite` (v9.0.9) - SQLite database support

## Notes

- The database file is automatically created in the `Data` folder on first run
- The form title displays the full database path for easy reference
- Debug output shows the record count on application start

---

**Ready for development!** Start customizing the form and database operations to build your desktop application.
