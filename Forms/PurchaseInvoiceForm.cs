using ChicoDesktopApp.Models;
using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class PurchaseInvoiceForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly PurchaseInvoiceRepository _purchaseRepo;
        private readonly ProductRepository _productRepo;
        
        private DataGridView dgvProducts;
        private DataGridView dgvInvoiceItems;
        private TextBox txtSearch;
        private TextBox txtSupplierName;
        private TextBox txtSupplierPhone;
        private TextBox txtNotes;
        private Label lblTotal;
        private Label lblInvoiceNumber;
        private Button btnCompletePurchase;
        private Button btnCancel;
        private Button btnAddToInvoice;
        private NumericUpDown nudQuantity;
        private NumericUpDown nudUnitCost;

        private PurchaseInvoice _currentInvoice;

        public PurchaseInvoiceForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _purchaseRepo = new PurchaseInvoiceRepository(_dbHelper);
            _productRepo = new ProductRepository(_dbHelper);
            
            _currentInvoice = new PurchaseInvoice
            {
                InvoiceNumber = _purchaseRepo.GenerateInvoiceNumber(),
                InvoiceDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                Items = new List<PurchaseInvoiceItem>()
            };

            InitializeComponent();
            LoadProducts();
            CalculateTotals();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "فاتورة مشتريات - Chico ERP";
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
                ForeColor = Color.FromArgb(156, 39, 176),
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

            // Main Container
            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // LEFT SIDE - Products Selection
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

            var lblSearch = new Label
            {
                Text = "بحث عن منتج:",
                Location = new Point(15, 15),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(15, 40),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11F)
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();

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
                Maximum = 10000,
                Value = 1,
                Font = new Font("Segoe UI", 11F)
            };

            var lblCost = new Label
            {
                Text = "سعر الشراء:",
                Location = new Point(180, 80),
                AutoSize = true
            };

            nudUnitCost = new NumericUpDown
            {
                Location = new Point(270, 78),
                Size = new Size(120, 30),
                Minimum = 0,
                Maximum = 1000000,
                DecimalPlaces = 2,
                Value = 0,
                Font = new Font("Segoe UI", 11F)
            };

            btnAddToInvoice = new Button
            {
                Text = "➕ إضافة للفاتورة",
                Location = new Point(410, 75),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddToInvoice.FlatAppearance.BorderSize = 0;
            btnAddToInvoice.Click += BtnAddToInvoice_Click;

            pnlSearch.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblQuantity, nudQuantity, lblCost, nudUnitCost, btnAddToInvoice
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
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "اسم المنتج", FillWeight = 35 },
                new DataGridViewTextBoxColumn { Name = "Barcode", HeaderText = "الباركود", FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "CurrentStock", HeaderText = "المخزون الحالي", FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "LastPurchasePrice", HeaderText = "آخر سعر شراء", FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "SalePrice", HeaderText = "سعر البيع", FillWeight = 15 }
            });

            dgvProducts.SelectionChanged += (s, e) =>
            {
                if (dgvProducts.CurrentRow != null)
                {
                    var lastPrice = dgvProducts.CurrentRow.Cells["LastPurchasePrice"].Value?.ToString();
                    if (!string.IsNullOrEmpty(lastPrice))
                    {
                        nudUnitCost.Value = Convert.ToDecimal(lastPrice);
                    }
                }
            };

            dgvProducts.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    BtnAddToInvoice_Click(s, e);
                }
            };

            pnlLeft.Controls.Add(dgvProducts);
            pnlLeft.Controls.Add(pnlSearch);

            // RIGHT SIDE - Invoice Items and Totals WITH SCROLLING
            var pnlRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = (int)(this.Width * 0.4),
                Padding = new Padding(5),
                AutoScroll = true  // Enable scrolling
            };

            // Container for right side controls
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
                Height = 400,
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
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "المنتج", ReadOnly = true, FillWeight = 35 },
                new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "الكمية", FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "UnitCost", HeaderText = "سعر الوحدة", FillWeight = 20 },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", ReadOnly = true, FillWeight = 20 },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "حذف", Text = "🗑️", UseColumnTextForButtonValue = true, FillWeight = 10 }
            });

            dgvInvoiceItems.CellContentClick += DgvInvoiceItems_CellContentClick;
            dgvInvoiceItems.CellEndEdit += (s, e) => CalculateTotals();

            pnlInvoiceItems.Controls.Add(dgvInvoiceItems);
            pnlInvoiceItems.Controls.Add(lblInvoiceItems);

            // Supplier Info Panel
            var pnlSupplier = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblSupplierTitle = new Label
            {
                Text = "معلومات المورد",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var lblSupplierName = new Label
            {
                Text = "اسم المورد:",
                Location = new Point(10, 45),
                AutoSize = true
            };

            txtSupplierName = new TextBox
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

            txtSupplierPhone = new TextBox
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

            pnlSupplier.Controls.AddRange(new Control[] {
                lblSupplierTitle, lblSupplierName, txtSupplierName,
                lblPhone, txtSupplierPhone, lblNotesLabel, txtNotes
            });

            // Totals Panel
            var pnlTotals = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(156, 39, 176),
                Padding = new Padding(15)
            };

            lblTotal = new Label
            {
                Text = "إجمالي الفاتورة: 0.00 ريال",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 40),
                AutoSize = true
            };

            pnlTotals.Controls.Add(lblTotal);

            // Buttons Panel
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            btnCompletePurchase = new Button
            {
                Text = "📦 إتمام الشراء",
                Location = new Point(10, 15),
                Size = new Size(170, 45),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCompletePurchase.FlatAppearance.BorderSize = 0;
            btnCompletePurchase.Click += BtnCompletePurchase_Click;

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

            pnlButtons.Controls.AddRange(new Control[] { btnCompletePurchase, btnCancel });

            // Add all right panels to content panel
            pnlRightContent.Controls.Add(pnlButtons);
            pnlRightContent.Controls.Add(pnlTotals);
            pnlRightContent.Controls.Add(pnlSupplier);
            pnlRightContent.Controls.Add(pnlInvoiceItems);

            pnlRight.Controls.Add(pnlRightContent);

            // Add to main container
            pnlMain.Controls.Add(pnlLeft);
            pnlMain.Controls.Add(pnlRight);

            // Add to form
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlTop);

            this.ResumeLayout(false);
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

            dgvProducts.Rows.Clear();
            foreach (var product in products)
            {
                dgvProducts.Rows.Add(
                    product.Id,
                    product.ProductNameArabic ?? product.ProductName,
                    product.Barcode ?? "N/A",
                    product.CurrentStock,
                    product.PurchasePrice.ToString("F2"),
                    product.SalePrice.ToString("F2")
                );
            }
        }

        private void BtnAddToInvoice_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            if (nudUnitCost.Value <= 0)
            {
                MessageBox.Show("الرجاء إدخال سعر الشراء", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudUnitCost.Focus();
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
            var product = _productRepo.GetProductById(productId);

            if (product != null)
            {
                int quantity = (int)nudQuantity.Value;
                decimal unitCost = nudUnitCost.Value;

                AddProductToInvoice(product, quantity, unitCost);
            }
        }

        private void AddProductToInvoice(Product product, int quantity, decimal unitCost)
        {
            // Check if product already in invoice
            foreach (DataGridViewRow row in dgvInvoiceItems.Rows)
            {
                int existingProductId = Convert.ToInt32(row.Cells["ProductId"].Value);
                if (existingProductId == product.Id)
                {
                    int existingQty = Convert.ToInt32(row.Cells["Quantity"].Value);
                    int newQty = existingQty + quantity;
                    
                    row.Cells["Quantity"].Value = newQty;
                    row.Cells["UnitCost"].Value = unitCost.ToString("F2");
                    row.Cells["Total"].Value = (newQty * unitCost).ToString("F2");
                    CalculateTotals();
                    return;
                }
            }

            // Add new item
            decimal total = quantity * unitCost;
            dgvInvoiceItems.Rows.Add(
                product.Id,
                product.ProductNameArabic ?? product.ProductName,
                quantity,
                unitCost.ToString("F2"),
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
            decimal total = 0;

            foreach (DataGridViewRow row in dgvInvoiceItems.Rows)
            {
                int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                decimal unitCost = Convert.ToDecimal(row.Cells["UnitCost"].Value);
                decimal itemTotal = quantity * unitCost;

                row.Cells["Total"].Value = itemTotal.ToString("F2");
                total += itemTotal;
            }

            lblTotal.Text = $"إجمالي الفاتورة: {total:F2} ريال";
        }

        private void BtnCompletePurchase_Click(object sender, EventArgs e)
        {
            if (dgvInvoiceItems.Rows.Count == 0)
            {
                MessageBox.Show("الرجاء إضافة منتجات للفاتورة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم المورد", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSupplierName.Focus();
                return;
            }

            var result = MessageBox.Show(
                $"هل تريد إتمام عملية الشراء؟\n\nالإجمالي: {lblTotal.Text}",
                "تأكيد الشراء",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Build invoice
                    _currentInvoice.SupplierName = txtSupplierName.Text.Trim();
                    _currentInvoice.SupplierPhone = string.IsNullOrWhiteSpace(txtSupplierPhone.Text) ? null : txtSupplierPhone.Text.Trim();
                    _currentInvoice.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
                    _currentInvoice.Items.Clear();

                    decimal total = 0;

                    foreach (DataGridViewRow row in dgvInvoiceItems.Rows)
                    {
                        int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                        string productName = row.Cells["ProductName"].Value.ToString() ?? "";
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                        decimal unitCost = Convert.ToDecimal(row.Cells["UnitCost"].Value);
                        decimal itemTotal = quantity * unitCost;

                        total += itemTotal;

                        _currentInvoice.Items.Add(new PurchaseInvoiceItem
                        {
                            ProductId = productId,
                            ProductName = productName,
                            Quantity = quantity,
                            UnitCost = unitCost,
                            TotalCost = itemTotal
                        });
                    }

                    _currentInvoice.TotalAmount = total;

                    // Save to database (this will auto-increment stock and update purchase prices)
                    int invoiceId = _purchaseRepo.AddPurchaseInvoice(_currentInvoice);

                    MessageBox.Show(
                        $"✅ تمت عملية الشراء بنجاح!\n\nرقم الفاتورة: {_currentInvoice.InvoiceNumber}\nالإجمالي: {_currentInvoice.TotalAmount:F2} ريال\n\nتم تحديث المخزون وأسعار الشراء",
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
