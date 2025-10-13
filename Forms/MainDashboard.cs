using ChicoDesktopApp.Repositories;
using ChicoDesktopApp.Models;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class MainDashboard : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        
        // Product management controls
        private Panel pnlProductManagement;
        private DataGridView dgvProducts;
        private TextBox txtSearch;
        private Button btnAddProduct;
        private Button btnEditProduct;
        private Button btnDeleteProduct;
        private Button btnRefresh;

        public MainDashboard()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _productRepository = new ProductRepository(_dbHelper);
            _categoryRepository = new CategoryRepository(_dbHelper);
            
            // Initialize product management section
            InitializeProductManagementSection();
        }

        private void MainDashboard_Load(object sender, EventArgs e)
        {
            // Update welcome message with current user
            lblCurrentUser.Text = $"المستخدم: {SessionManager.GetCurrentUserDisplay()} ({SessionManager.CurrentUser?.RoleDisplay})";
            
            // Add context menu to current user label for account settings
            var userMenu = new ContextMenuStrip();
            userMenu.RightToLeft = RightToLeft.Yes;
            
            var changePasswordItem = new ToolStripMenuItem("🔐 تغيير كلمة المرور");
            changePasswordItem.Click += (s, ev) =>
            {
                using var form = new ChangePasswordForm(_dbHelper);
                form.ShowDialog();
            };
            userMenu.Items.Add(changePasswordItem);
            
            lblCurrentUser.Cursor = Cursors.Hand;
            lblCurrentUser.ContextMenuStrip = userMenu;
            lblCurrentUser.MouseClick += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Left)
                {
                    userMenu.Show(lblCurrentUser, ev.Location);
                }
            };
            
            // Hide Users button if not admin
            if (!SessionManager.IsAdmin)
            {
                btnUsers.Visible = false;
            }

            // Load dashboard statistics
            LoadDashboardStats();
            
            // Load products in the management section
            LoadProducts();

            // Start timer for date/time
            timerDateTime.Start();
            UpdateDateTime();
        }
        
        private void InitializeProductManagementSection()
        {
            // Create product management panel
            pnlProductManagement = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            // Header
            var lblHeader = new Label
            {
                Text = "📦 إدارة المنتجات",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 150, 243),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Search panel
            var pnlSearch = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(panelContent.Width - 40, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblSearch = new Label
            {
                Text = "🔍 البحث:",
                Location = new Point(920, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            txtSearch = new TextBox
            {
                Location = new Point(600, 12),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10F)
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();

            btnAddProduct = new Button
            {
                Text = "➕ إضافة منتج",
                Location = new Point(450, 10),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddProduct.FlatAppearance.BorderSize = 0;
            btnAddProduct.Click += BtnAddProduct_Click;

            btnEditProduct = new Button
            {
                Text = "✏️ تعديل",
                Location = new Point(310, 10),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditProduct.FlatAppearance.BorderSize = 0;
            btnEditProduct.Click += BtnEditProduct_Click;

            btnDeleteProduct = new Button
            {
                Text = "🗑️ حذف",
                Location = new Point(170, 10),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteProduct.FlatAppearance.BorderSize = 0;
            btnDeleteProduct.Click += BtnDeleteProduct_Click;

            btnRefresh = new Button
            {
                Text = "🔄 تحديث",
                Location = new Point(30, 10),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => { LoadProducts(); LoadDashboardStats(); };

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnAddProduct, btnEditProduct, btnDeleteProduct, btnRefresh });

            // Products DataGridView
            dgvProducts = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(panelContent.Width - 40, panelContent.Height - 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10F),
                RowTemplate = { Height = 40 }
            };

            dgvProducts.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false },
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "اسم المنتج", FillWeight = 25 },
                new DataGridViewTextBoxColumn { Name = "Barcode", HeaderText = "الباركود", FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "الفئة", FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "CurrentStock", HeaderText = "المخزون", FillWeight = 10 },
                new DataGridViewTextBoxColumn { Name = "PurchasePrice", HeaderText = "سعر الشراء", FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "SalePrice", HeaderText = "سعر البيع", FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "MinStock", HeaderText = "الحد الأدنى", FillWeight = 11 }
            });

            dgvProducts.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEditProduct_Click(s, e); };

            // Add all controls to product management panel
            pnlProductManagement.Controls.AddRange(new Control[] { lblHeader, pnlSearch, dgvProducts });

            // Add product management panel to main content area
            panelContent.Controls.Clear();
            panelContent.Controls.Add(pnlProductManagement);
        }
        
        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAllProducts();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    var searchTerm = txtSearch.Text.Trim().ToLower();
                    products = products.Where(p =>
                        (p.ProductNameArabic != null && p.ProductNameArabic.ToLower().Contains(searchTerm)) ||
                        p.ProductName.ToLower().Contains(searchTerm) ||
                        (p.Barcode != null && p.Barcode.Contains(searchTerm))
                    ).ToList();
                }

                dgvProducts.Rows.Clear();
                foreach (var product in products)
                {
                    var category = _categoryRepository.GetCategoryById(product.CategoryId);
                    var categoryName = category?.NameArabic ?? category?.Name ?? "غير محدد";

                    var rowIndex = dgvProducts.Rows.Add(
                        product.Id,
                        product.ProductNameArabic ?? product.ProductName,
                        product.Barcode ?? "N/A",
                        categoryName,
                        product.CurrentStock,
                        product.PurchasePrice.ToString("F2"),
                        product.SalePrice.ToString("F2"),
                        product.MinimumStock
                    );

                    // Highlight low stock items
                    if (product.CurrentStock <= product.MinimumStock)
                    {
                        dgvProducts.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                        dgvProducts.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(183, 28, 28);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المنتجات:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            var editForm = new ProductEditForm(_dbHelper);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
                LoadDashboardStats();
                MessageBox.Show("تم إضافة المنتج بنجاح!", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEditProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("الرجاء اختيار منتج للتعديل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
            var product = _productRepository.GetProductById(productId);

            if (product != null)
            {
                var editForm = new ProductEditForm(_dbHelper, product);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                    LoadDashboardStats();
                    MessageBox.Show("تم تحديث المنتج بنجاح!", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("الرجاء اختيار منتج للحذف", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
            string productName = dgvProducts.CurrentRow.Cells["ProductName"].Value.ToString() ?? "";

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف المنتج '{productName}'؟\n\nهذا الإجراء لا يمكن التراجع عنه!",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _productRepository.DeleteProduct(productId);
                    LoadProducts();
                    LoadDashboardStats();
                    MessageBox.Show("تم حذف المنتج بنجاح!", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف المنتج:\n{ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
                // Get total products
                var totalProducts = _productRepository.GetAllProducts().Count;
                lblTotalProducts.Text = $"📦 إجمالي المنتجات\n{totalProducts}";

                // Get low stock products
                var lowStockProducts = _productRepository.GetLowStockProducts().Count;
                lblLowStock.Text = $"⚠️ تنبيه مخزون منخفض\n{lowStockProducts}";

                // Get today's sales (placeholder - will implement later)
                lblTodaySales.Text = "💰 مبيعات اليوم\n0.00";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الإحصائيات:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDateTime()
        {
            var now = DateTime.Now;
            var culture = new CultureInfo("ar-SA");
            lblDateTime.Text = now.ToString("yyyy/MM/dd - hh:mm:ss tt", culture);
        }

        private void timerDateTime_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            // Show product management section in main content area
            panelContent.Controls.Clear();
            panelContent.Controls.Add(pnlProductManagement);
            pnlProductManagement.Dock = DockStyle.Fill;
            LoadProducts();
            LoadDashboardStats();
        }

        private void btnSalesInvoice_Click(object sender, EventArgs e)
        {
            var salesInvoiceForm = new SalesInvoiceForm(_dbHelper);
            salesInvoiceForm.ShowDialog();
            // Refresh dashboard stats after closing sales invoice
            LoadDashboardStats();
        }

        private void btnPurchaseInvoice_Click(object sender, EventArgs e)
        {
            var purchaseInvoiceForm = new PurchaseInvoiceForm(_dbHelper);
            purchaseInvoiceForm.ShowDialog();
            // Refresh dashboard stats after closing purchase invoice
            LoadDashboardStats();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            MessageBox.Show("قريباً: التقارير", "قيد التطوير",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Open Reports Form
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show("عذراً، هذه الميزة متاحة للمديرين فقط", "غير مصرح",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new UserManagementForm(_dbHelper);
            form.ShowDialog();
            
            // Refresh stats in case users were added/modified
            LoadDashboardStats();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل تريد تسجيل الخروج؟", "تأكيد تسجيل الخروج",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SessionManager.Logout();
                this.Close();
                
                // Show login form again
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Reopen dashboard with new user
                    var newDashboard = new MainDashboard();
                    newDashboard.Show();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void MainDashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SessionManager.IsAuthenticated)
            {
                var result = MessageBox.Show("هل تريد إغلاق التطبيق؟", "تأكيد الإغلاق",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    SessionManager.Logout();
                }
            }
        }
    }
}
