using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class ChangePasswordForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly AuthRepository _authRepo;

        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnChange;
        private Button btnCancel;
        private Label lblUserInfo;
        private CheckBox chkShowPasswords;

        public ChangePasswordForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _authRepo = new AuthRepository(dbHelper);

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "تغيير كلمة المرور - Change Password";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;

            // Title
            var lblTitle = new Label
            {
                Text = "🔐 تغيير كلمة المرور",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(lblTitle);
            yPos += 50;

            // User Info
            lblUserInfo = new Label
            {
                Text = $"المستخدم: {SessionManager.GetCurrentUserDisplay()}",
                Location = new Point(20, yPos),
                Size = new Size(440, 40),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(236, 240, 241),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblUserInfo);
            yPos += 60;

            // Current Password
            var lblCurrentPassword = new Label
            {
                Text = "كلمة المرور الحالية:",
                Location = new Point(300, yPos),
                Size = new Size(160, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtCurrentPassword = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(270, 30),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            this.Controls.AddRange(new Control[] { lblCurrentPassword, txtCurrentPassword });
            yPos += 50;

            // Separator
            var lblSeparator = new Label
            {
                Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
                Location = new Point(20, yPos),
                Size = new Size(440, 25),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(189, 195, 199),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblSeparator);
            yPos += 30;

            // New Password
            var lblNewPassword = new Label
            {
                Text = "كلمة المرور الجديدة:",
                Location = new Point(300, yPos),
                Size = new Size(160, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtNewPassword = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(270, 30),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            this.Controls.AddRange(new Control[] { lblNewPassword, txtNewPassword });
            yPos += 40;

            // Password strength hint
            var lblPasswordHint = new Label
            {
                Text = "💡 يجب أن تكون 6 أحرف على الأقل",
                Location = new Point(20, yPos),
                Size = new Size(270, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(127, 140, 141),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblPasswordHint);
            yPos += 30;

            // Confirm Password
            var lblConfirmPassword = new Label
            {
                Text = "تأكيد كلمة المرور الجديدة:",
                Location = new Point(300, yPos),
                Size = new Size(160, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtConfirmPassword = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(270, 30),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            this.Controls.AddRange(new Control[] { lblConfirmPassword, txtConfirmPassword });
            yPos += 50;

            // Show passwords checkbox
            chkShowPasswords = new CheckBox
            {
                Text = "👁️ إظهار كلمات المرور",
                Location = new Point(150, yPos),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F)
            };
            chkShowPasswords.CheckedChanged += (s, e) =>
            {
                bool show = chkShowPasswords.Checked;
                txtCurrentPassword.UseSystemPasswordChar = !show;
                txtNewPassword.UseSystemPasswordChar = !show;
                txtConfirmPassword.UseSystemPasswordChar = !show;
            };
            this.Controls.Add(chkShowPasswords);
            yPos += 40;

            // Buttons Panel
            var pnlButtons = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(440, 50),
                BackColor = Color.FromArgb(236, 240, 241)
            };

            // Change Button
            btnChange = new Button
            {
                Text = "✅ تغيير كلمة المرور",
                Location = new Point(220, 5),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChange.FlatAppearance.BorderSize = 0;
            btnChange.Click += BtnChange_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "❌ إلغاء",
                Location = new Point(20, 5),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            pnlButtons.Controls.AddRange(new Control[] { btnChange, btnCancel });
            this.Controls.Add(pnlButtons);
        }

        private void BtnChange_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtCurrentPassword.Text))
            {
                MessageBox.Show("الرجاء إدخال كلمة المرور الحالية", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCurrentPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("الرجاء إدخال كلمة المرور الجديدة", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Text.Length < 6)
            {
                MessageBox.Show("كلمة المرور الجديدة يجب أن تكون 6 أحرف على الأقل", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("كلمة المرور الجديدة وتأكيد كلمة المرور غير متطابقتين", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtConfirmPassword.Focus();
                return;
            }

            if (txtCurrentPassword.Text == txtNewPassword.Text)
            {
                MessageBox.Show("كلمة المرور الجديدة يجب أن تكون مختلفة عن الحالية", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            try
            {
                var userId = SessionManager.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    MessageBox.Show("خطأ في جلسة المستخدم", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool success = _authRepo.ChangePassword(userId, txtCurrentPassword.Text, txtNewPassword.Text);

                if (success)
                {
                    MessageBox.Show("تم تغيير كلمة المرور بنجاح!\n\nستحتاج إلى تسجيل الدخول مرة أخرى عند الخروج.", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("كلمة المرور الحالية غير صحيحة", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtCurrentPassword.SelectAll();
                    txtCurrentPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تغيير كلمة المرور:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
