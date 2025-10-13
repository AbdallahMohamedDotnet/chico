using ChicoDesktopApp.Forms;
using System.Windows.Forms;

namespace ChicoDesktopApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            // Initialize database and seed data
            var dbHelper = new DatabaseHelper();
            dbHelper.SeedDatabase();
            
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Show login form first
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // If login successful, show main dashboard
                Application.Run(new MainDashboard());
            }
            // If login cancelled or failed, application exits
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ غير متوقع في التطبيق:\n\n{ex.Message}\n\nتفاصيل:\n{ex.StackTrace}",
                "خطأ فادح",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            
            // Log to console for debugging
            Console.WriteLine($"FATAL ERROR: {ex}");
        }
    }    
}