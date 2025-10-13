using ChicoDesktopApp.Models;
using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class ResetPasswordForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly AuthRepository _authRepo;
        private readonly User _user;

        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnReset;
        private Button btnCancel;
        private Label lblUserInfo;

        public ResetPasswordForm(DatabaseHelper dbHelper, User user)
        {
            _dbHelper = dbHelper;
            _authRepo = new AuthRepository(dbHelper);
            _user = user;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "إعادة تعيين كلمة المرور - Reset Password";
            this.Size = new Size(500, 350);
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
                Text = "🔑 إعادة تعيين كلمة المرور",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(155, 89, 182),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(lblTitle);
            yPos += 50;

            // User Info
            lblUserInfo = new Label
            {
                Text = $"المستخدم: {_user.DisplayName}\nاسم الدخول: {_user.Username}",
                Location = new Point(20, yPos),
                Size = new Size(440, 50),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(236, 240, 241),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblUserInfo);
            yPos += 70;

            // Warning
            var lblWarning = new Label
            {
                Text = "⚠️ سيتم إعادة تعيين كلمة المرور لهذا المستخدم",
                Location = new Point(20, yPos),
                Size = new Size(440, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.FromArgb(230, 126, 34),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblWarning);
            yPos += 40;

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

            // Confirm Password
            var lblConfirmPassword = new Label
            {
                Text = "تأكيد كلمة المرور:",
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
            yPos += 60;

            // Buttons Panel
            var pnlButtons = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(440, 50),
                BackColor = Color.FromArgb(236, 240, 241)
            };

            // Reset Button
            btnReset = new Button
            {
                Text = "🔑 إعادة تعيين",
                Location = new Point(240, 5),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "❌ إلغاء",
                Location = new Point(20, 5),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            pnlButtons.Controls.AddRange(new Control[] { btnReset, btnCancel });
            this.Controls.Add(pnlButtons);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("الرجاء إدخال كلمة المرور الجديدة", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Text.Length < 6)
            {
                MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("كلمة المرور وتأكيد كلمة المرور غير متطابقتين", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtConfirmPassword.Focus();
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من إعادة تعيين كلمة المرور للمستخدم '{_user.DisplayName}'؟",
                "تأكيد",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _authRepo.ResetPassword(_user.Id, txtNewPassword.Text);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في إعادة تعيين كلمة المرور:\n{ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
