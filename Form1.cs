namespace ChicoDesktopApp;

public partial class Form1 : Form
{
    private readonly DatabaseHelper _dbHelper;

    public Form1()
    {
        InitializeComponent();
        
        // Initialize database
        _dbHelper = new DatabaseHelper();
        
        // Display database path in the form title
        this.Text = $"Chico Desktop App - DB: {_dbHelper.GetDatabasePath()}";
        
        // Load initial data count
        LoadDataCount();
    }

    private void LoadDataCount()
    {
        long count = _dbHelper.GetDataCount();
        // You can update a label or textbox here
        // For now, just show in debug output
        System.Diagnostics.Debug.WriteLine($"Total records in database: {count}");
    }
}
