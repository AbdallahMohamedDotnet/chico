using ChicoDesktopApp.Models;
using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class SalesInvoiceForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly SalesInvoiceRepository _salesRepo;
        private readonly ProductRepository _productRepo;
        private readonly CategoryRepository _categoryRepo;
        
        private DataGridView dgvProducts;
        private DataGridView dgvInvoiceItems;
        private TextBox txtBarcode;
        private TextBox txtSearch;
        private TextBox txtCustomerName;
        private TextBox txtCustomerPhone;
        private TextBox txtNotes;
        private NumericUpDown nudDiscount;
        private ComboBox cmbDiscountType;
        private Label lblSubtotal;
        private Label lblDiscount;
        private Label lblTotal;
        private Label lblProfit;
        private Label lblInvoiceNumber;
        private Button btnCompleteSale;
        private Button btnCancel;
        private Button btnAddToInvoice;
        private NumericUpDown nudQuantity;
        private ComboBox cmbCategoryFilter;

        private SalesInvoice _currentInvoice;

        public SalesInvoiceForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _salesRepo = new SalesInvoiceRepository(_dbHelper);
            _productRepo = new ProductRepository(_dbHelper);
            _categoryRepo = new CategoryRepository(_dbHelper);
            
            _currentInvoice = new SalesInvoice
            {
                InvoiceNumber = _salesRepo.GenerateInvoiceNumber(),
                InvoiceDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                Items = new List<SalesInvoiceItem>()
            };

            InitializeComponent();
            LoadProducts();
            CalculateTotals();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "فاتورة مبيعات - Chico ERP";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Segoe UI", 10F);
            this.MinimumSize = new Size(1200, 800);
            this.BackColor = Color.FromArgb(240, 242, 245);

            // Top Panel - Invoice Info
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            lblInvoiceNumber = new Label
            {
                Text = $"رقم الفاتورة: {_currentInvoice.InvoiceNumber}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 150, 243),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblDate = new Label
            {
                Text = $"التاريخ: {DateTime.Now:yyyy/MM/dd HH:mm}",
                Font = new Font("Segoe UI", 11F),
                Location = new Point(20, 45),
                AutoSize = true
            };

            pnlTop.Controls.AddRange(new Control[] { lblInvoiceNumber, lblDate });

            // Main Container - Split into 2 sections
            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // LEFT SIDE - Products Selection (60% width)
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            // Search Panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            var lblBarcode = new Label
            {
                Text = "الباركود:",
                Location = new Point(15, 15),
                AutoSize = true
            };

            txtBarcode = new TextBox
            {
                Location = new Point(15, 40),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11F)
            };
            txtBarcode.KeyPress += TxtBarcode_KeyPress;

            var lblSearch = new Label
            {
                Text = "بحث:",
                Location = new Point(280, 15),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(280, 40),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 11F)
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();

            var lblCategory = new Label
            {
                Text = "الفئة:",
                Location = new Point(500, 15),
                AutoSize = true
            };

            cmbCategoryFilter = new ComboBox
            {
                Location = new Point(500, 40),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbCategoryFilter.Items.Add("الكل");
            cmbCategoryFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndexChanged += (s, e) => LoadProducts();

            var lblQuantity = new Label
            {
                Text = "الكمية:",
                Location = new Point(15, 80),
                AutoSize = true
            };

            nudQuantity = new NumericUpDown
            {
                Location = new Point(80, 78),
                Size = new Size(80, 30),
                Minimum = 1,
                Maximum = 1000,
                Value = 1,
                Font = new Font("Segoe UI", 11F)
            };

            btnAddToInvoice = new Button
            {
                Text = "➕ إضافة للفاتورة",
                Location = new Point(180, 75),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddToInvoice.FlatAppearance.BorderSize = 0;
            btnAddToInvoice.Click += BtnAddToInvoice_Click;

            pnlSearch.Controls.AddRange(new Control[] {
                lblBarcode, txtBarcode, lblSearch, txtSearch,
                lblCategory, cmbCategoryFilter, lblQuantity, nudQuantity, btnAddToInvoice
            });

            // Products Grid
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
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
                RowTemplate = { Height = 35 }
            };

            dgvProducts.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false },
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "اسم المنتج", FillWeight = 30 },
                new DataGridViewTextBoxColumn { Name = "Barcode", HeaderText = "الباركود", FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "CurrentStock", HeaderText = "المخزون", FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "SalePrice", HeaderText = "سعر البيع", FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "CategoryName", HeaderText = "الفئة", FillWeight = 15 }
            });

            dgvProducts.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    BtnAddToInvoice_Click(s, e);
                }
            };

            pnlLeft.Controls.Add(dgvProducts);
            pnlLeft.Controls.Add(pnlSearch);

            // RIGHT SIDE - Invoice Items and Totals (40% width) WITH SCROLLING
            var pnlRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = (int)(this.Width * 0.4),
                Padding = new Padding(5),
                AutoScroll = true  // Enable scrolling for entire right panel
            };

            // Container for all right side controls
            var pnlRightContent = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Invoice Items Grid
            var pnlInvoiceItems = new Panel
            {
                Dock = DockStyle.Top,
                Height = 350,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblInvoiceItems = new Label
            {
                Text = "عناصر الفاتورة",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight
            };

            dgvInvoiceItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F),
                RowTemplate = { Height = 30 }
            };

            dgvInvoiceItems.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "ProductId", Visible = false },
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "المنتج", ReadOnly = true, FillWeight = 40 },
                new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "الكمية", FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "السعر", ReadOnly = true, FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", ReadOnly = true, FillWeight = 20 },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "حذف", Text = "🗑️", UseColumnTextForButtonValue = true, FillWeight = 10 }
            });

            dgvInvoiceItems.CellContentClick += DgvInvoiceItems_CellContentClick;
            dgvInvoiceItems.CellEndEdit += (s, e) => CalculateTotals();

            pnlInvoiceItems.Controls.Add(dgvInvoiceItems);
            pnlInvoiceItems.Controls.Add(lblInvoiceItems);

            // Customer Info Panel
            var pnlCustomer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblCustomerTitle = new Label
            {
                Text = "معلومات العميل",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var lblCustomerName = new Label
            {
                Text = "اسم العميل:",
                Location = new Point(10, 45),
                AutoSize = true
            };

            txtCustomerName = new TextBox
            {
                Location = new Point(100, 42),
                Size = new Size(250, 25)
            };

            var lblPhone = new Label
            {
                Text = "الهاتف:",
                Location = new Point(10, 80),
                AutoSize = true
            };

            txtCustomerPhone = new TextBox
            {
                Location = new Point(100, 77),
                Size = new Size(250, 25)
            };

            var lblNotesLabel = new Label
            {
                Text = "ملاحظات:",
                Location = new Point(10, 115),
                AutoSize = true
            };

            txtNotes = new TextBox
            {
                Location = new Point(100, 112),
                Size = new Size(250, 25)
            };

            pnlCustomer.Controls.AddRange(new Control[] {
                lblCustomerTitle, lblCustomerName, txtCustomerName,
                lblPhone, txtCustomerPhone, lblNotesLabel, txtNotes
            });

            // Discount Panel
            var pnlDiscount = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblDiscountTitle = new Label
            {
                Text = "الخصم:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 15),
                AutoSize = true
            };

            nudDiscount = new NumericUpDown
            {
                Location = new Point(100, 12),
                Size = new Size(100, 25),
                Minimum = 0,
                Maximum = 100000,
                DecimalPlaces = 2
            };
            nudDiscount.ValueChanged += (s, e) => CalculateTotals();

            cmbDiscountType = new ComboBox
            {
                Location = new Point(210, 12),
                Size = new Size(80, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDiscountType.Items.AddRange(new object[] { "ريال", "%" });
            cmbDiscountType.SelectedIndex = 0;
            cmbDiscountType.SelectedIndexChanged += (s, e) => CalculateTotals();

            pnlDiscount.Controls.AddRange(new Control[] {
                lblDiscountTitle, nudDiscount, cmbDiscountType
            });

            // Totals Panel
            var pnlTotals = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = Color.FromArgb(33, 150, 243),
                Padding = new Padding(15)
            };

            lblSubtotal = new Label
            {
                Text = "المجموع الفرعي: 0.00 ريال",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };

            lblDiscount = new Label
            {
                Text = "الخصم: 0.00 ريال",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.White,
                Location = new Point(15, 50),
                AutoSize = true
            };

            lblTotal = new Label
            {
                Text = "الإجمالي النهائي: 0.00 ريال",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 85),
                AutoSize = true
            };

            lblProfit = new Label
            {
                Text = "الربح المتوقع: 0.00 ريال",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.LightGreen,
                Location = new Point(15, 130),
                AutoSize = true
            };

            pnlTotals.Controls.AddRange(new Control[] { lblSubtotal, lblDiscount, lblTotal, lblProfit });

            // Buttons Panel
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            btnCompleteSale = new Button
            {
                Text = "💰 إتمام البيع",
                Location = new Point(10, 15),
                Size = new Size(170, 45),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCompleteSale.FlatAppearance.BorderSize = 0;
            btnCompleteSale.Click += BtnCompleteSale_Click;

            btnCancel = new Button
            {
                Text = "❌ إلغاء",
                Location = new Point(190, 15),
                Size = new Size(100, 45),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            pnlButtons.Controls.AddRange(new Control[] { btnCompleteSale, btnCancel });

            // Add all right panels to content panel in correct order (bottom to top for Dock.Top)
            pnlRightContent.Controls.Add(pnlButtons);
            pnlRightContent.Controls.Add(pnlTotals);
            pnlRightContent.Controls.Add(pnlDiscount);
            pnlRightContent.Controls.Add(pnlCustomer);
            pnlRightContent.Controls.Add(pnlInvoiceItems);

            pnlRight.Controls.Add(pnlRightContent);

            // Add to main container
            pnlMain.Controls.Add(pnlLeft);
            pnlMain.Controls.Add(pnlRight);

            // Add to form
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlTop);

            this.ResumeLayout(false);

            // Load categories
            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = _categoryRepo.GetAllCategories();
            foreach (var category in categories)
            {
                cmbCategoryFilter.Items.Add(category.NameArabic ?? category.Name);
            }
        }

        private void LoadProducts()
        {
            var products = _productRepo.GetAllProducts();

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
            if (cmbCategoryFilter.SelectedIndex > 0)
            {
                var categoryName = cmbCategoryFilter.SelectedItem.ToString();
                var category = _categoryRepo.GetAllCategories()
                    .FirstOrDefault(c => (c.NameArabic ?? c.Name) == categoryName);
                if (category != null)
                {
                    products = products.Where(p => p.CategoryId == category.Id).ToList();
                }
            }

            // Filter out products with zero stock
            products = products.Where(p => p.CurrentStock > 0).ToList();

            dgvProducts.Rows.Clear();
            foreach (var product in products)
            {
                var category = _categoryRepo.GetCategoryById(product.CategoryId);
                dgvProducts.Rows.Add(
                    product.Id,
                    product.ProductNameArabic ?? product.ProductName,
                    product.Barcode ?? "N/A",
                    product.CurrentStock,
                    product.SalePrice.ToString("F2"),
                    category?.NameArabic ?? category?.Name ?? "غير محدد"
                );
            }
        }

        private void TxtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                var barcode = txtBarcode.Text.Trim();
                if (!string.IsNullOrWhiteSpace(barcode))
                {
                    var product = _productRepo.GetAllProducts()
                        .FirstOrDefault(p => p.Barcode == barcode);
                    
                    if (product != null)
                    {
                        if (product.CurrentStock > 0)
                        {
                            AddProductToInvoice(product, (int)nudQuantity.Value);
                            txtBarcode.Clear();
                            txtBarcode.Focus();
                        }
                        else
                        {
                            MessageBox.Show("المنتج غير متوفر في المخزون", "تنبيه",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("الباركود غير موجود", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnAddToInvoice_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
            var product = _productRepo.GetProductById(productId);

            if (product != null)
            {
                int quantity = (int)nudQuantity.Value;
                if (quantity > product.CurrentStock)
                {
                    MessageBox.Show($"الكمية المطلوبة ({quantity}) أكبر من المخزون المتاح ({product.CurrentStock})",
                        "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddProductToInvoice(product, quantity);
            }
        }

        private void AddProductToInvoice(Product product, int quantity)
        {
            // Check if product already in invoice
            foreach (DataGridViewRow row in dgvInvoiceItems.Rows)
            {
                int existingProductId = Convert.ToInt32(row.Cells["ProductId"].Value);
                if (existingProductId == product.Id)
                {
                    int existingQty = Convert.ToInt32(row.Cells["Quantity"].Value);
                    int newQty = existingQty + quantity;
                    
                    if (newQty > product.CurrentStock)
                    {
                        MessageBox.Show($"الكمية الإجمالية ({newQty}) أكبر من المخزون المتاح ({product.CurrentStock})",
                            "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    row.Cells["Quantity"].Value = newQty;
                    row.Cells["Total"].Value = (newQty * product.SalePrice).ToString("F2");
                    CalculateTotals();
                    return;
                }
            }

            // Add new item
            decimal total = quantity * product.SalePrice;
            dgvInvoiceItems.Rows.Add(
                product.Id,
                product.ProductNameArabic ?? product.ProductName,
                quantity,
                product.SalePrice.ToString("F2"),
                total.ToString("F2"),
                "🗑️"
            );

            CalculateTotals();
        }

        private void DgvInvoiceItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvInvoiceItems.Columns[e.ColumnIndex].Name == "Delete")
            {
                dgvInvoiceItems.Rows.RemoveAt(e.RowIndex);
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            decimal subtotal = 0;
            decimal totalProfit = 0;

            foreach (DataGridViewRow row in dgvInvoiceItems.Rows)
            {
                int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);
                decimal total = quantity * unitPrice;

                subtotal += total;

                // Calculate profit
                var product = _productRepo.GetProductById(productId);
                if (product != null)
                {
                    decimal profit = (unitPrice - product.PurchasePrice) * quantity;
                    totalProfit += profit;
                }
            }

            // Calculate discount
            decimal discountAmount = 0;
            if (nudDiscount.Value > 0)
            {
                if (cmbDiscountType.SelectedIndex == 0) // Fixed amount
                {
                    discountAmount = nudDiscount.Value;
                }
                else // Percentage
                {
                    discountAmount = subtotal * (nudDiscount.Value / 100);
                }
            }

            decimal finalTotal = subtotal - discountAmount;

            lblSubtotal.Text = $"المجموع الفرعي: {subtotal:F2} ريال";
            lblDiscount.Text = $"الخصم: {discountAmount:F2} ريال";
            lblTotal.Text = $"الإجمالي النهائي: {finalTotal:F2} ريال";
            lblProfit.Text = $"الربح المتوقع: {totalProfit:F2} ريال";
        }

        private void BtnCompleteSale_Click(object sender, EventArgs e)
        {
            if (dgvInvoiceItems.Rows.Count == 0)
            {
                MessageBox.Show("الرجاء إضافة منتجات للفاتورة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"هل تريد إتمام عملية البيع؟\n\nالإجمالي: {lblTotal.Text}",
                "تأكيد البيع",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Build invoice
                    _currentInvoice.CustomerName = string.IsNullOrWhiteSpace(txtCustomerName.Text) ? null : txtCustomerName.Text.Trim();
                    _currentInvoice.CustomerPhone = string.IsNullOrWhiteSpace(txtCustomerPhone.Text) ? null : txtCustomerPhone.Text.Trim();
                    _currentInvoice.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
                    _currentInvoice.Items.Clear();

                    decimal subtotal = 0;
                    decimal totalProfit = 0;

                    foreach (DataGridViewRow row in dgvInvoiceItems.Rows)
                    {
                        int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                        string productName = row.Cells["ProductName"].Value.ToString() ?? "";
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                        decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);

                        var product = _productRepo.GetProductById(productId);
                        if (product == null) continue;

                        decimal itemTotal = quantity * unitPrice;
                        decimal itemProfit = (unitPrice - product.PurchasePrice) * quantity;

                        subtotal += itemTotal;
                        totalProfit += itemProfit;

                        _currentInvoice.Items.Add(new SalesInvoiceItem
                        {
                            ProductId = productId,
                            ProductName = productName,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            UnitCost = product.PurchasePrice,
                            TotalPrice = itemTotal,
                            Profit = itemProfit
                        });
                    }

                    _currentInvoice.Subtotal = subtotal;

                    // Calculate discount
                    if (nudDiscount.Value > 0)
                    {
                        if (cmbDiscountType.SelectedIndex == 0) // Fixed
                        {
                            _currentInvoice.DiscountAmount = nudDiscount.Value;
                        }
                        else // Percentage
                        {
                            _currentInvoice.DiscountAmount = subtotal * (nudDiscount.Value / 100);
                        }
                    }
                    else
                    {
                        _currentInvoice.DiscountAmount = 0;
                    }

                    _currentInvoice.TotalAmount = subtotal - _currentInvoice.DiscountAmount;
                    _currentInvoice.TotalProfit = totalProfit;

                    // Save to database (this will auto-decrement stock)
                    int invoiceId = _salesRepo.AddSalesInvoice(_currentInvoice);

                    MessageBox.Show(
                        $"✅ تمت عملية البيع بنجاح!\n\nرقم الفاتورة: {_currentInvoice.InvoiceNumber}\nالإجمالي: {_currentInvoice.TotalAmount:F2} ريال\nالربح: {_currentInvoice.TotalProfit:F2} ريال",
                        "نجاح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء حفظ الفاتورة:\n{ex.Message}",
                        "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
