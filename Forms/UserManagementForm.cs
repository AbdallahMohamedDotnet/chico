using ChicoDesktopApp.Models;
using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class UserManagementForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly AuthRepository _authRepo;
        
        private DataGridView dgvUsers;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnResetPassword;
        private Button btnRefresh;
        private Button btnClose;
        private Panel pnlActions;
        private Label lblTitle;

        public UserManagementForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _authRepo = new AuthRepository(dbHelper);
            
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة المستخدمين - User Management";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title Label
            lblTitle = new Label
            {
                Text = "👥 إدارة المستخدمين",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // DataGridView
            dgvUsers = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(940, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 11F)
            };

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "الرقم",
                DataPropertyName = "Id",
                Width = 80,
                Visible = false
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Username",
                HeaderText = "اسم المستخدم",
                DataPropertyName = "Username",
                Width = 150
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "الاسم الكامل",
                DataPropertyName = "FullName",
                Width = 200
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullNameArabic",
                HeaderText = "الاسم بالعربية",
                DataPropertyName = "FullNameArabic",
                Width = 200
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RoleDisplay",
                HeaderText = "الدور",
                DataPropertyName = "RoleDisplay",
                Width = 120
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IsActive",
                HeaderText = "نشط",
                DataPropertyName = "IsActive",
                Width = 80
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedDate",
                HeaderText = "تاريخ الإنشاء",
                DataPropertyName = "CreatedDate",
                Width = 150
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LastLoginDate",
                HeaderText = "آخر تسجيل دخول",
                DataPropertyName = "LastLoginDate",
                Width = 150
            });

            // Format dates and boolean values
            dgvUsers.CellFormatting += (s, e) =>
            {
                if (dgvUsers.Columns[e.ColumnIndex].Name == "CreatedDate" && e.Value != null)
                {
                    if (e.Value is DateTime date)
                        e.Value = date.ToString("yyyy-MM-dd HH:mm");
                }
                else if (dgvUsers.Columns[e.ColumnIndex].Name == "LastLoginDate" && e.Value != null)
                {
                    if (e.Value is DateTime date)
                        e.Value = date.ToString("yyyy-MM-dd HH:mm");
                }
                else if (dgvUsers.Columns[e.ColumnIndex].Name == "IsActive")
                {
                    e.Value = (bool)e.Value ? "نعم" : "لا";
                }
            };

            // Highlight inactive users
            dgvUsers.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvUsers.Rows.Count)
                {
                    var row = dgvUsers.Rows[e.RowIndex];
                    if (row.Cells["IsActive"].Value != null && !(bool)((DataGridViewRow)row).DataBoundItem.GetType().GetProperty("IsActive").GetValue(((DataGridViewRow)row).DataBoundItem))
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        row.DefaultCellStyle.ForeColor = Color.DarkGray;
                    }
                }
            };

            dgvUsers.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            // Action Panel
            pnlActions = new Panel
            {
                Location = new Point(20, 580),
                Size = new Size(940, 60),
                BackColor = Color.FromArgb(236, 240, 241)
            };

            // Add Button
            btnAdd = new Button
            {
                Text = "➕ إضافة مستخدم",
                Location = new Point(760, 10),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            // Edit Button
            btnEdit = new Button
            {
                Text = "✏️ تعديل",
                Location = new Point(600, 10),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEdit_Click;

            // Delete Button
            btnDelete = new Button
            {
                Text = "🗑️ حذف",
                Location = new Point(450, 10),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            // Reset Password Button
            btnResetPassword = new Button
            {
                Text = "🔑 إعادة تعيين كلمة المرور",
                Location = new Point(240, 10),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResetPassword.FlatAppearance.BorderSize = 0;
            btnResetPassword.Click += BtnResetPassword_Click;

            // Refresh Button
            btnRefresh = new Button
            {
                Text = "🔄",
                Location = new Point(180, 10),
                Size = new Size(50, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadUsers();

            // Close Button
            btnClose = new Button
            {
                Text = "❌ إغلاق",
                Location = new Point(10, 10),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlActions.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnResetPassword, btnRefresh, btnClose });

            // Add all to form
            this.Controls.Add(lblTitle);
            this.Controls.Add(dgvUsers);
            this.Controls.Add(pnlActions);
        }

        private void LoadUsers()
        {
            try
            {
                var users = _authRepo.GetAllUsers();
                dgvUsers.DataSource = users;

                // Update title with count
                lblTitle.Text = $"👥 إدارة المستخدمين ({users.Count} مستخدم)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المستخدمين:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using var form = new UserEditForm(_dbHelper);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
                MessageBox.Show("تم إضافة المستخدم بنجاح!", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار مستخدم للتعديل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = (User)dgvUsers.SelectedRows[0].DataBoundItem;
            
            using var form = new UserEditForm(_dbHelper, selectedUser);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
                MessageBox.Show("تم تحديث المستخدم بنجاح!", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار مستخدم للحذف", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = (User)dgvUsers.SelectedRows[0].DataBoundItem;

            // Prevent deleting current user
            if (selectedUser.Id == SessionManager.CurrentUser?.Id)
            {
                MessageBox.Show("لا يمكن حذف المستخدم الحالي!", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف المستخدم '{selectedUser.DisplayName}'؟\n\nهذا الإجراء لا يمكن التراجع عنه!",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool deleted = _authRepo.DeleteUser(selectedUser.Id);
                    if (deleted)
                    {
                        LoadUsers();
                        MessageBox.Show("تم حذف المستخدم بنجاح!", "نجح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("لا يمكن حذف آخر مدير في النظام!", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف المستخدم:\n{ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار مستخدم لإعادة تعيين كلمة المرور", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = (User)dgvUsers.SelectedRows[0].DataBoundItem;

            using var form = new ResetPasswordForm(_dbHelper, selectedUser);
            if (form.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("تم إعادة تعيين كلمة المرور بنجاح!", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
