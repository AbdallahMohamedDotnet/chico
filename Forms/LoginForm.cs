using ChicoDesktopApp.Repositories;
using System;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthRepository _authRepository;
        private readonly DatabaseHelper _dbHelper;

        public LoginForm()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _authRepository = new AuthRepository(_dbHelper);
            
            // Load and apply theme
            ThemeManager.LoadThemePreference();
            ThemeManager.ApplyTheme(this);
            
            // Set focus to username field
            this.Load += (s, e) => txtUsername.Focus();
            
            // Handle Enter key in password field
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    btnLogin_Click(btnLogin, EventArgs.Empty);
                    e.Handled = true;
                }
            };
            
            // Debug: Verify admin account exists
            System.Diagnostics.Debug.WriteLine("\n=== Login Form Loaded - Admin Account Verification ===");
            System.Diagnostics.Debug.WriteLine($"User count: {_authRepository.GetUserCount()}");
            _authRepository.VerifyAdminAccount();
            System.Diagnostics.Debug.WriteLine("===================================================\n");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم المستخدم", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("الرجاء إدخال كلمة المرور", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                // Disable login button to prevent double-click
                btnLogin.Enabled = false;
                btnLogin.Text = "جاري التحقق...";
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                // Authenticate user
                var user = _authRepository.AuthenticateUser(txtUsername.Text.Trim(), txtPassword.Text);

                if (user != null)
                {
                    // Set current user in session
                    SessionManager.CurrentUser = user;

                    // Log successful login
                    System.Diagnostics.Debug.WriteLine($"Login successful: {user.Username} ({user.Role})");

                    // Show success message
                    MessageBox.Show($"مرحباً {user.DisplayName}!\nالدور: {user.RoleDisplay}", 
                        "تسجيل دخول ناجح", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Close login form and show main dashboard
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة\n\nتلميح: اسم المستخدم الافتراضي هو 'admin' وكلمة المرور 'admin123'", 
                        "خطأ في تسجيل الدخول", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    // Clear password and focus username
                    txtPassword.Clear();
                    txtUsername.SelectAll();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تسجيل الدخول:\n{ex.Message}\n\nتفاصيل: {ex.StackTrace}", 
                    "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
            finally
            {
                // Re-enable login button and restore cursor
                btnLogin.Enabled = true;
                btnLogin.Text = "تسجيل الدخول";
                this.Cursor = Cursors.Default;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل تريد إغلاق التطبيق؟", "تأكيد الخروج", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
