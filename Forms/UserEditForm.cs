using ChicoDesktopApp.Models;
using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class UserEditForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly AuthRepository _authRepo;
        private readonly User? _userToEdit;
        private readonly bool _isEditMode;

        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtFullName;
        private TextBox txtFullNameArabic;
        private ComboBox cmbRole;
        private CheckBox chkIsActive;
        private Button btnSave;
        private Button btnCancel;
        private Label lblPasswordNote;

        public UserEditForm(DatabaseHelper dbHelper, User? userToEdit = null)
        {
            _dbHelper = dbHelper;
            _authRepo = new AuthRepository(dbHelper);
            _userToEdit = userToEdit;
            _isEditMode = userToEdit != null;

            InitializeComponent();
            
            if (_isEditMode)
            {
                LoadUserData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "تعديل مستخدم - Edit User" : "إضافة مستخدم - Add User";
            this.Size = new Size(550, 550);
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
                Text = _isEditMode ? "✏️ تعديل بيانات المستخدم" : "➕ إضافة مستخدم جديد",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(lblTitle);
            yPos += 50;

            // Username
            var lblUsername = new Label
            {
                Text = "اسم المستخدم:",
                Location = new Point(370, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtUsername = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11F),
                ReadOnly = _isEditMode // Username cannot be changed
            };
            this.Controls.AddRange(new Control[] { lblUsername, txtUsername });
            yPos += 40;

            // Full Name
            var lblFullName = new Label
            {
                Text = "الاسم الكامل (EN):",
                Location = new Point(370, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtFullName = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11F)
            };
            this.Controls.AddRange(new Control[] { lblFullName, txtFullName });
            yPos += 40;

            // Full Name Arabic
            var lblFullNameArabic = new Label
            {
                Text = "الاسم الكامل (AR):",
                Location = new Point(370, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtFullNameArabic = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11F)
            };
            this.Controls.AddRange(new Control[] { lblFullNameArabic, txtFullNameArabic });
            yPos += 40;

            // Role
            var lblRole = new Label
            {
                Text = "الدور:",
                Location = new Point(370, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            cmbRole = new ComboBox
            {
                Location = new Point(20, yPos),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new object[] { "مدير (Admin)", "أمين صندوق (Cashier)" });
            cmbRole.SelectedIndex = 0;
            this.Controls.AddRange(new Control[] { lblRole, cmbRole });
            yPos += 40;

            // Is Active
            chkIsActive = new CheckBox
            {
                Text = "المستخدم نشط",
                Location = new Point(250, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F),
                Checked = true
            };
            this.Controls.Add(chkIsActive);
            yPos += 40;

            // Password section
            var lblPasswordSection = new Label
            {
                Text = "━━━━━━━━━━━━ كلمة المرور ━━━━━━━━━━━━",
                Location = new Point(100, yPos),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblPasswordSection);
            yPos += 35;

            // Password note for edit mode
            if (_isEditMode)
            {
                lblPasswordNote = new Label
                {
                    Text = "⚠️ اترك كلمة المرور فارغة إذا كنت لا تريد تغييرها",
                    Location = new Point(20, yPos),
                    Size = new Size(500, 25),
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(230, 126, 34),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(lblPasswordNote);
                yPos += 30;
            }

            // Password
            var lblPassword = new Label
            {
                Text = _isEditMode ? "كلمة المرور الجديدة:" : "كلمة المرور:",
                Location = new Point(370, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtPassword = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            this.Controls.AddRange(new Control[] { lblPassword, txtPassword });
            yPos += 40;

            // Confirm Password
            var lblConfirmPassword = new Label
            {
                Text = "تأكيد كلمة المرور:",
                Location = new Point(370, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtConfirmPassword = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            this.Controls.AddRange(new Control[] { lblConfirmPassword, txtConfirmPassword });
            yPos += 60;

            // Buttons Panel
            var pnlButtons = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(500, 50),
                BackColor = Color.FromArgb(236, 240, 241)
            };

            // Save Button
            btnSave = new Button
            {
                Text = "💾 حفظ",
                Location = new Point(280, 5),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "❌ إلغاء",
                Location = new Point(20, 5),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            pnlButtons.Controls.AddRange(new Control[] { btnSave, btnCancel });
            this.Controls.Add(pnlButtons);
        }

        private void LoadUserData()
        {
            if (_userToEdit == null) return;

            txtUsername.Text = _userToEdit.Username;
            txtFullName.Text = _userToEdit.FullName;
            txtFullNameArabic.Text = _userToEdit.FullNameArabic ?? "";
            cmbRole.SelectedIndex = _userToEdit.Role == UserRole.Admin ? 0 : 1;
            chkIsActive.Checked = _userToEdit.IsActive;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم المستخدم", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("الرجاء إدخال الاسم الكامل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtFullName.Focus();
                return;
            }

            // Password validation (required for new users, optional for edit)
            if (!_isEditMode)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("الرجاء إدخال كلمة المرور", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Focus();
                    return;
                }
            }
            else if (!string.IsNullOrWhiteSpace(txtPassword.Text) && txtPassword.Text.Length < 6)
            {
                MessageBox.Show("كلمة المرور الجديدة يجب أن تكون 6 أحرف على الأقل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Focus();
                return;
            }

            // Confirm password match
            if (!string.IsNullOrWhiteSpace(txtPassword.Text) && txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("كلمة المرور وتأكيد كلمة المرور غير متطابقتين", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    // Update existing user
                    _userToEdit!.FullName = txtFullName.Text.Trim();
                    _userToEdit.FullNameArabic = string.IsNullOrWhiteSpace(txtFullNameArabic.Text) ? null : txtFullNameArabic.Text.Trim();
                    _userToEdit.Role = cmbRole.SelectedIndex == 0 ? UserRole.Admin : UserRole.Cashier;
                    _userToEdit.IsActive = chkIsActive.Checked;

                    _authRepo.UpdateUser(_userToEdit);

                    // Update password if provided
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        _authRepo.ResetPassword(_userToEdit.Id, txtPassword.Text);
                    }
                }
                else
                {
                    // Add new user
                    var newUser = new User
                    {
                        Username = txtUsername.Text.Trim(),
                        FullName = txtFullName.Text.Trim(),
                        FullNameArabic = string.IsNullOrWhiteSpace(txtFullNameArabic.Text) ? null : txtFullNameArabic.Text.Trim(),
                        Role = cmbRole.SelectedIndex == 0 ? UserRole.Admin : UserRole.Cashier,
                        IsActive = chkIsActive.Checked,
                        CreatedDate = DateTime.Now
                    };

                    _authRepo.AddUser(newUser, txtPassword.Text);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ المستخدم:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
