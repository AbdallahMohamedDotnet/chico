using ChicoDesktopApp.Models;
using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class ProductEditForm : Form
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly Product? _existingProduct;
        private readonly bool _isEditMode;

        private TextBox txtBarcode = null!;
        private TextBox txtProductName = null!;
        private TextBox txtProductNameArabic = null!;
        private ComboBox cmbCategory = null!;
        private NumericUpDown numCurrentStock = null!;
        private NumericUpDown numMinimumStock = null!;
        private NumericUpDown numPurchasePrice = null!;
        private NumericUpDown numSalePrice = null!;
        private TextBox txtDescription = null!;
        private Label lblProfitMargin = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public ProductEditForm(ProductRepository productRepository, CategoryRepository categoryRepository, Product? product = null)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _existingProduct = product;
            _isEditMode = product != null;

            InitializeComponent();
            LoadCategories();
            
            if (_isEditMode && _existingProduct != null)
            {
                LoadProductData();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = _isEditMode ? "تعديل منتج - Chico ERP" : "إضافة منتج جديد - Chico ERP";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.White;

            int labelX = 550;
            int controlX = 180;
            int currentY = 20;
            int rowHeight = 60;
            int controlWidth = 350;

            // Title Label
            var lblTitle = new Label
            {
                Text = _isEditMode ? "تعديل بيانات المنتج" : "إضافة منتج جديد",
                Location = new Point(0, currentY),
                Size = new Size(700, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            currentY += 50;

            // Barcode
            var lblBarcode = new Label
            {
                Text = "الباركود:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtBarcode = new TextBox
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 11F)
            };
            currentY += rowHeight;

            // Product Name Arabic
            var lblProductNameArabic = new Label
            {
                Text = "*الاسم بالعربية:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            txtProductNameArabic = new TextBox
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 11F)
            };
            currentY += rowHeight;

            // Product Name English
            var lblProductName = new Label
            {
                Text = "*الاسم بالإنجليزية:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            txtProductName = new TextBox
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 11F)
            };
            currentY += rowHeight;

            // Category
            var lblCategory = new Label
            {
                Text = "*الفئة:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            cmbCategory = new ComboBox
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11F)
            };
            currentY += rowHeight;

            // Current Stock
            var lblCurrentStock = new Label
            {
                Text = "*المخزون الحالي:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            numCurrentStock = new NumericUpDown
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                Minimum = 0,
                Maximum = 999999,
                Value = 0,
                Font = new Font("Segoe UI", 11F),
                TextAlign = HorizontalAlignment.Center
            };
            currentY += rowHeight;

            // Minimum Stock
            var lblMinimumStock = new Label
            {
                Text = "حد إعادة الطلب:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            numMinimumStock = new NumericUpDown
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                Minimum = 0,
                Maximum = 999999,
                Value = 10,
                Font = new Font("Segoe UI", 11F),
                TextAlign = HorizontalAlignment.Center
            };
            currentY += rowHeight;

            // Purchase Price
            var lblPurchasePrice = new Label
            {
                Text = "*سعر الشراء:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            numPurchasePrice = new NumericUpDown
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                Minimum = 0,
                Maximum = 9999999,
                DecimalPlaces = 2,
                Value = 0,
                Font = new Font("Segoe UI", 11F),
                TextAlign = HorizontalAlignment.Center
            };
            numPurchasePrice.ValueChanged += (s, e) => CalculateProfitMargin();
            currentY += rowHeight;

            // Sale Price
            var lblSalePrice = new Label
            {
                Text = "*سعر البيع:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            numSalePrice = new NumericUpDown
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                Minimum = 0,
                Maximum = 9999999,
                DecimalPlaces = 2,
                Value = 0,
                Font = new Font("Segoe UI", 11F),
                TextAlign = HorizontalAlignment.Center
            };
            numSalePrice.ValueChanged += (s, e) => CalculateProfitMargin();
            currentY += rowHeight;

            // Profit Margin Display
            lblProfitMargin = new Label
            {
                Text = "هامش الربح: 0.0%",
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96),
                BackColor = Color.FromArgb(230, 245, 237),
                BorderStyle = BorderStyle.FixedSingle
            };
            currentY += 45;

            // Description
            var lblDescription = new Label
            {
                Text = "الوصف:",
                Location = new Point(labelX, currentY),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            txtDescription = new TextBox
            {
                Location = new Point(controlX, currentY),
                Size = new Size(controlWidth, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 10F)
            };
            currentY += 80;

            // Required fields note
            var lblRequired = new Label
            {
                Text = "* الحقول المطلوبة",
                Location = new Point(controlX, currentY),
                Size = new Size(150, 20),
                ForeColor = Color.FromArgb(192, 57, 43),
                Font = new Font("Segoe UI", 9F)
            };
            currentY += 30;

            // Buttons Panel
            var pnlButtons = new Panel
            {
                Location = new Point(0, currentY),
                Size = new Size(700, 60),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Save Button
            btnSave = new Button
            {
                Location = new Point(400, 10),
                Size = new Size(130, 40),
                Text = _isEditMode ? "💾 حفظ التعديلات" : "➕ إضافة",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Location = new Point(250, 10),
                Size = new Size(130, 40),
                Text = "❌ إلغاء",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            pnlButtons.Controls.AddRange(new Control[] { btnSave, btnCancel });

            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblBarcode, txtBarcode,
                lblProductNameArabic, txtProductNameArabic,
                lblProductName, txtProductName,
                lblCategory, cmbCategory,
                lblCurrentStock, numCurrentStock,
                lblMinimumStock, numMinimumStock,
                lblPurchasePrice, numPurchasePrice,
                lblSalePrice, numSalePrice,
                lblProfitMargin,
                lblDescription, txtDescription,
                lblRequired,
                pnlButtons
            });

            this.ResumeLayout(false);
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            var categories = _categoryRepository.GetAllCategories();
            
            foreach (var category in categories)
            {
                cmbCategory.Items.Add(category);
            }

            cmbCategory.DisplayMember = "NameArabic";
            cmbCategory.ValueMember = "Id";

            if (cmbCategory.Items.Count > 0)
            {
                cmbCategory.SelectedIndex = 0;
            }
        }

        private void LoadProductData()
        {
            if (_existingProduct == null) return;

            txtBarcode.Text = _existingProduct.Barcode ?? "";
            txtProductNameArabic.Text = _existingProduct.ProductNameArabic ?? "";
            txtProductName.Text = _existingProduct.ProductName;
            numCurrentStock.Value = _existingProduct.CurrentStock;
            numMinimumStock.Value = _existingProduct.MinimumStock;
            numPurchasePrice.Value = _existingProduct.PurchasePrice;
            numSalePrice.Value = _existingProduct.SalePrice;
            txtDescription.Text = _existingProduct.Description ?? "";

            // Select category
            for (int i = 0; i < cmbCategory.Items.Count; i++)
            {
                var category = (Category)cmbCategory.Items[i]!;
                if (category.Id == _existingProduct.CategoryId)
                {
                    cmbCategory.SelectedIndex = i;
                    break;
                }
            }

            CalculateProfitMargin();
        }

        private void CalculateProfitMargin()
        {
            if (numPurchasePrice.Value == 0)
            {
                lblProfitMargin.Text = "هامش الربح: 0.0%";
                lblProfitMargin.ForeColor = Color.Gray;
                return;
            }

            decimal profitMargin = ((numSalePrice.Value - numPurchasePrice.Value) / numPurchasePrice.Value) * 100;
            lblProfitMargin.Text = $"هامش الربح: {profitMargin:F1}%";
            
            if (profitMargin < 0)
            {
                lblProfitMargin.ForeColor = Color.Red;
                lblProfitMargin.BackColor = Color.FromArgb(255, 230, 230);
            }
            else if (profitMargin < 10)
            {
                lblProfitMargin.ForeColor = Color.Orange;
                lblProfitMargin.BackColor = Color.FromArgb(255, 245, 230);
            }
            else
            {
                lblProfitMargin.ForeColor = Color.FromArgb(39, 174, 96);
                lblProfitMargin.BackColor = Color.FromArgb(230, 245, 237);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم المنتج بالإنجليزية", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار فئة المنتج", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return;
            }

            if (numPurchasePrice.Value == 0)
            {
                MessageBox.Show("يرجى إدخال سعر الشراء", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPurchasePrice.Focus();
                return;
            }

            if (numSalePrice.Value == 0)
            {
                MessageBox.Show("يرجى إدخال سعر البيع", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSalePrice.Focus();
                return;
            }

            if (numSalePrice.Value < numPurchasePrice.Value)
            {
                var result = MessageBox.Show(
                    "سعر البيع أقل من سعر الشراء! هذا يعني خسارة في كل عملية بيع.\n\nهل تريد المتابعة؟",
                    "تحذير",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    numSalePrice.Focus();
                    return;
                }
            }

            try
            {
                var selectedCategory = (Category)cmbCategory.SelectedItem!;

                if (_isEditMode && _existingProduct != null)
                {
                    // Update existing product
                    _existingProduct.Barcode = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text.Trim();
                    _existingProduct.ProductNameArabic = string.IsNullOrWhiteSpace(txtProductNameArabic.Text) ? null : txtProductNameArabic.Text.Trim();
                    _existingProduct.ProductName = txtProductName.Text.Trim();
                    _existingProduct.CategoryId = selectedCategory.Id;
                    _existingProduct.CurrentStock = (int)numCurrentStock.Value;
                    _existingProduct.MinimumStock = (int)numMinimumStock.Value;
                    _existingProduct.PurchasePrice = numPurchasePrice.Value;
                    _existingProduct.SalePrice = numSalePrice.Value;
                    _existingProduct.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                    _existingProduct.UpdatedDate = DateTime.Now;

                    _productRepository.UpdateProduct(_existingProduct);
                    MessageBox.Show("تم تعديل المنتج بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Create new product
                    var newProduct = new Product
                    {
                        Barcode = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text.Trim(),
                        ProductNameArabic = string.IsNullOrWhiteSpace(txtProductNameArabic.Text) ? null : txtProductNameArabic.Text.Trim(),
                        ProductName = txtProductName.Text.Trim(),
                        CategoryId = selectedCategory.Id,
                        CurrentStock = (int)numCurrentStock.Value,
                        MinimumStock = (int)numMinimumStock.Value,
                        PurchasePrice = numPurchasePrice.Value,
                        SalePrice = numSalePrice.Value,
                        Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsActive = true
                    };

                    _productRepository.AddProduct(newProduct);
                    MessageBox.Show("تم إضافة المنتج بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ المنتج: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
