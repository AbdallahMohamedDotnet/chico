using ChicoDesktopApp.Models;
using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class ProductListForm : Form
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly DatabaseHelper _dbHelper;
        
        private DataGridView dgvProducts = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbCategory = null!;
        private CheckBox chkLowStock = null!;
        private Button btnSearch = null!;
        private Button btnAdd = null!;
        private Button btnEdit = null!;
        private Button btnDelete = null!;
        private Button btnRefresh = null!;
        private Label lblTotalProducts = null!;
        private Label lblLowStock = null!;

        public ProductListForm()
        {
            _dbHelper = new DatabaseHelper();
            _productRepository = new ProductRepository(_dbHelper);
            _categoryRepository = new CategoryRepository(_dbHelper);
            
            InitializeComponent();
            SetupForm();
            LoadCategories();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "إدارة المنتجات - Chico ERP";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Segoe UI", 10F);
            this.MinimumSize = new Size(1024, 768);

            // Search Panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Search TextBox
            txtSearch = new TextBox
            {
                Location = new Point(850, 15),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "بحث بالاسم أو الباركود..."
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();

            // Category Filter
            cmbCategory = new ComboBox
            {
                Location = new Point(600, 15),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadProducts();

            // Low Stock Checkbox
            chkLowStock = new CheckBox
            {
                Location = new Point(430, 18),
                Size = new Size(150, 25),
                Text = "منتجات قليلة المخزون",
                Font = new Font("Segoe UI", 10F)
            };
            chkLowStock.CheckedChanged += (s, e) => LoadProducts();

            // Search Button
            btnSearch = new Button
            {
                Location = new Point(320, 15),
                Size = new Size(100, 35),
                Text = "بحث",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadProducts();

            // Refresh Button
            btnRefresh = new Button
            {
                Location = new Point(210, 15),
                Size = new Size(100, 35),
                Text = "تحديث",
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadProducts();

            // Stats Labels
            lblTotalProducts = new Label
            {
                Location = new Point(15, 50),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F),
                Text = "إجمالي المنتجات: 0"
            };

            lblLowStock = new Label
            {
                Location = new Point(220, 50),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Red,
                Text = "منتجات قليلة المخزون: 0"
            };

            pnlSearch.Controls.AddRange(new Control[] {
                txtSearch, cmbCategory, chkLowStock, btnSearch, btnRefresh,
                lblTotalProducts, lblLowStock
            });

            // DataGridView
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10F),
                RowTemplate = { Height = 35 }
            };
            dgvProducts.CellDoubleClick += DgvProducts_CellDoubleClick;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;

            // Action Panel
            var pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Add Button
            btnAdd = new Button
            {
                Location = new Point(880, 15),
                Size = new Size(120, 40),
                Text = "➕ إضافة منتج",
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            // Edit Button
            btnEdit = new Button
            {
                Location = new Point(740, 15),
                Size = new Size(120, 40),
                Text = "✏️ تعديل",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEdit_Click;

            // Delete Button
            btnDelete = new Button
            {
                Location = new Point(600, 15),
                Size = new Size(120, 40),
                Text = "🗑️ حذف",
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            // Only show delete button to Admin
            if (!SessionManager.IsAdmin)
            {
                btnDelete.Visible = false;
            }

            pnlActions.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // Add all to form
            this.Controls.Add(dgvProducts);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(pnlActions);
            
            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Setup DataGridView columns
            dgvProducts.Columns.Clear();
            dgvProducts.Columns.Add("Id", "الرقم");
            dgvProducts.Columns.Add("Barcode", "الباركود");
            dgvProducts.Columns.Add("ProductNameArabic", "الاسم بالعربية");
            dgvProducts.Columns.Add("ProductName", "الاسم بالإنجليزية");
            dgvProducts.Columns.Add("CategoryName", "الفئة");
            dgvProducts.Columns.Add("CurrentStock", "المخزون الحالي");
            dgvProducts.Columns.Add("MinimumStock", "حد إعادة الطلب");
            dgvProducts.Columns.Add("PurchasePrice", "سعر الشراء");
            dgvProducts.Columns.Add("SalePrice", "سعر البيع");
            dgvProducts.Columns.Add("ProfitPercentage", "هامش الربح %");

            // Set column widths
            dgvProducts.Columns["Id"].Width = 60;
            dgvProducts.Columns["Barcode"].Width = 120;
            dgvProducts.Columns["ProductNameArabic"].Width = 200;
            dgvProducts.Columns["ProductName"].Width = 180;
            dgvProducts.Columns["CategoryName"].Width = 120;
            dgvProducts.Columns["CurrentStock"].Width = 80;
            dgvProducts.Columns["MinimumStock"].Width = 100;
            dgvProducts.Columns["PurchasePrice"].Width = 100;
            dgvProducts.Columns["SalePrice"].Width = 100;
            dgvProducts.Columns["ProfitPercentage"].Width = 100;

            // Center align numeric columns
            foreach (var col in new[] { "CurrentStock", "MinimumStock", "PurchasePrice", "SalePrice", "ProfitPercentage" })
            {
                dgvProducts.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add(new { Id = 0, NameArabic = "جميع الفئات" });
            
            var categories = _categoryRepository.GetAllCategories();
            foreach (var category in categories)
            {
                cmbCategory.Items.Add(category);
            }
            
            cmbCategory.DisplayMember = "NameArabic";
            cmbCategory.ValueMember = "Id";
            cmbCategory.SelectedIndex = 0;
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

                // Apply category filter
                if (cmbCategory.SelectedIndex > 0)
                {
                    var selectedCategory = (Category)cmbCategory.SelectedItem!;
                    products = products.Where(p => p.CategoryId == selectedCategory.Id).ToList();
                }

                // Apply low stock filter
                if (chkLowStock.Checked)
                {
                    products = products.Where(p => p.IsLowStock).ToList();
                }

                // Populate DataGridView
                dgvProducts.Rows.Clear();
                foreach (var product in products)
                {
                    var categoryName = _categoryRepository.GetCategoryById(product.CategoryId)?.NameArabic ?? "غير محدد";
                    
                    dgvProducts.Rows.Add(
                        product.Id,
                        product.Barcode ?? "N/A",
                        product.ProductNameArabic ?? product.ProductName,
                        product.ProductName,
                        categoryName,
                        product.CurrentStock,
                        product.MinimumStock,
                        product.PurchasePrice.ToString("F2"),
                        product.SalePrice.ToString("F2"),
                        product.ProfitPercentage.ToString("F1")
                    );
                }

                // Update stats
                lblTotalProducts.Text = $"إجمالي المنتجات: {products.Count}";
                var lowStockCount = _productRepository.GetLowStockProducts().Count;
                lblLowStock.Text = $"منتجات قليلة المخزون: {lowStockCount}";
                lblLowStock.ForeColor = lowStockCount > 0 ? Color.Red : Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المنتجات: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvProducts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProducts.Columns[e.ColumnIndex].Name == "CurrentStock")
            {
                if (e.RowIndex >= 0 && e.Value != null)
                {
                    var quantity = int.Parse(e.Value.ToString()!);
                    var reorderLevel = int.Parse(dgvProducts.Rows[e.RowIndex].Cells["MinimumStock"].Value.ToString()!);
                    
                    if (quantity <= reorderLevel)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(e.CellStyle.Font!, FontStyle.Bold);
                    }
                }
            }
        }

        private void DgvProducts_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnEdit_Click(sender, e);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var form = new ProductEditForm(_productRepository, _categoryRepository);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار منتج للتعديل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var productId = (int)dgvProducts.SelectedRows[0].Cells["Id"].Value;
            var product = _productRepository.GetProductById(productId);

            if (product == null)
            {
                MessageBox.Show("المنتج غير موجود", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var form = new ProductEditForm(_productRepository, _categoryRepository, product);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار منتج للحذف", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var productId = (int)dgvProducts.SelectedRows[0].Cells["Id"].Value;
            var productName = dgvProducts.SelectedRows[0].Cells["ProductNameArabic"].Value?.ToString() 
                ?? dgvProducts.SelectedRows[0].Cells["ProductName"].Value?.ToString() 
                ?? "Unknown";

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف المنتج: {productName}؟\n\nملاحظة: سيتم حذف المنتج نهائياً ولا يمكن التراجع عن هذا الإجراء.",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    _productRepository.DeleteProduct(productId);
                    MessageBox.Show("تم حذف المنتج بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف المنتج: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
