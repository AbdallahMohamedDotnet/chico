using ChicoDesktopApp.Repositories;
using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class ReportsForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly ReportRepository _reportRepository;
        
        private TabControl tabReports = null!;
        private DateTimePicker dtpStartDate = null!;
        private DateTimePicker dtpEndDate = null!;
        private Button btnGenerateReport = null!;
        private Button btnExport = null!;
        private Button btnClose = null!;
        private RichTextBox rtbReport = null!;

        public ReportsForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _reportRepository = new ReportRepository(dbHelper);
            
            InitializeComponent();
            LoadInitialReports();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "التقارير - Chico ERP";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Segoe UI", 10F);

            // Tab Control for different report types
            tabReports = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F)
            };

            // Add tabs
            var tabSales = new TabPage("تقارير المبيعات");
            var tabPurchases = new TabPage("تقارير المشتريات");
            var tabInventory = new TabPage("تقارير المخزون");
            var tabProfit = new TabPage("تقارير الأرباح");
            var tabDashboard = new TabPage("لوحة المعلومات");

            tabReports.TabPages.AddRange(new[] { tabDashboard, tabSales, tabInventory, tabPurchases, tabProfit });

            // Setup Dashboard Tab
            SetupDashboardTab(tabDashboard);

            // Setup Sales Tab
            SetupSalesTab(tabSales);

            // Setup Inventory Tab
            SetupInventoryTab(tabInventory);

            // Setup Purchases Tab
            SetupPurchasesTab(tabPurchases);

            // Setup Profit Tab
            SetupProfitTab(tabProfit);

            // Bottom panel with export and close buttons
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            btnExport = new Button
            {
                Text = "📊 تصدير إلى ملف نصي",
                Size = new Size(150, 40),
                Location = new Point(160, 10),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            btnClose = new Button
            {
                Text = "إغلاق",
                Size = new Size(150, 40),
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.AddRange(new Control[] { btnClose, btnExport });

            this.Controls.AddRange(new Control[] { tabReports, pnlBottom });
            this.ResumeLayout(false);
        }

        private void SetupDashboardTab(TabPage tab)
        {
            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            rtbReport = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            var btnRefresh = new Button
            {
                Text = "🔄 تحديث",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadDashboardReport();

            pnlMain.Controls.Add(rtbReport);
            pnlMain.Controls.Add(btnRefresh);
            tab.Controls.Add(pnlMain);
        }

        private void SetupSalesTab(TabPage tab)
        {
            SetupDateRangeReport(tab, "المبيعات", LoadSalesReport);
        }

        private void SetupPurchasesTab(TabPage tab)
        {
            SetupDateRangeReport(tab, "المشتريات", LoadPurchasesReport);
        }

        private void SetupInventoryTab(TabPage tab)
        {
            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 0, 0, 10) };
            
            var btnInventoryValue = new Button
            {
                Text = "📦 قيمة المخزون الإجمالية",
                Location = new Point(0, 0),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnInventoryValue.FlatAppearance.BorderSize = 0;
            btnInventoryValue.Click += (s, e) => LoadInventoryValueReport(rtb);

            var btnLowStock = new Button
            {
                Text = "⚠️ منتجات منخفضة المخزون",
                Location = new Point(210, 0),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLowStock.FlatAppearance.BorderSize = 0;
            btnLowStock.Click += (s, e) => LoadLowStockReport(rtb);

            var btnOutOfStock = new Button
            {
                Text = "🚫 منتجات نفذت من المخزون",
                Location = new Point(420, 0),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOutOfStock.FlatAppearance.BorderSize = 0;
            btnOutOfStock.Click += (s, e) => LoadOutOfStockReport(rtb);

            var btnTopSelling = new Button
            {
                Text = "🏆 أكثر المنتجات مبيعاً",
                Location = new Point(0, 50),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTopSelling.FlatAppearance.BorderSize = 0;
            btnTopSelling.Click += (s, e) => LoadTopSellingReport(rtb);

            pnlTop.Controls.AddRange(new Control[] { btnInventoryValue, btnLowStock, btnOutOfStock, btnTopSelling });
            pnlMain.Controls.Add(rtb);
            pnlMain.Controls.Add(pnlTop);
            tab.Controls.Add(pnlMain);

            // Load initial report
            LoadInventoryValueReport(rtb);
        }

        private void SetupProfitTab(TabPage tab)
        {
            SetupDateRangeReport(tab, "الأرباح", LoadProfitReport);
        }

        private void SetupDateRangeReport(TabPage tab, string reportName, Action<RichTextBox, DateTime, DateTime> loadAction)
        {
            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 0, 0, 10) };
            
            var lblTitle = new Label
            {
                Text = $"تقرير {reportName}",
                Location = new Point(0, 5),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            var lblFrom = new Label
            {
                Text = "من تاريخ:",
                Location = new Point(0, 40),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10F)
            };

            var dtp1 = new DateTimePicker
            {
                Location = new Point(90, 38),
                Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1)
            };

            var lblTo = new Label
            {
                Text = "إلى تاريخ:",
                Location = new Point(250, 40),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10F)
            };

            var dtp2 = new DateTimePicker
            {
                Location = new Point(340, 38),
                Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            var btnGenerate = new Button
            {
                Text = "📊 إنشاء التقرير",
                Location = new Point(500, 35),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += (s, e) => loadAction(rtb, dtp1.Value, dtp2.Value);

            pnlTop.Controls.AddRange(new Control[] { lblTitle, lblFrom, dtp1, lblTo, dtp2, btnGenerate });
            pnlMain.Controls.Add(rtb);
            pnlMain.Controls.Add(pnlTop);
            tab.Controls.Add(pnlMain);

            // Load initial report
            loadAction(rtb, dtp1.Value, dtp2.Value);
        }

        private void LoadInitialReports()
        {
            LoadDashboardReport();
        }

        private void LoadDashboardReport()
        {
            try
            {
                var stats = _reportRepository.GetDashboardStatistics();
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                    لوحة المعلومات - Dashboard                    ");
                sb.AppendLine($"                    {DateTime.Now:yyyy-MM-dd HH:mm:ss}                    ");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                sb.AppendLine("📊 إحصائيات اليوم:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  💰 إجمالي المبيعات:        {stats.TodaysSales:N2} جنيه");
                sb.AppendLine($"  📈 صافي الربح:             {stats.TodaysProfit:N2} جنيه");
                sb.AppendLine($"  📄 عدد الفواتير:           {stats.TodaysInvoiceCount}");
                sb.AppendLine();
                
                sb.AppendLine("📅 إحصائيات الشهر الحالي:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  💰 إجمالي المبيعات:        {stats.MonthSales:N2} جنيه");
                sb.AppendLine($"  📈 صافي الربح:             {stats.MonthProfit:N2} جنيه");
                sb.AppendLine();
                
                sb.AppendLine("📦 إحصائيات المخزون:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  📦 إجمالي المنتجات:        {stats.TotalProducts}");
                sb.AppendLine($"  ⚠️  منتجات منخفضة:         {stats.LowStockProducts}");
                sb.AppendLine($"  🚫 منتجات نفذت:           {stats.OutOfStockProducts}");
                sb.AppendLine($"  💵 قيمة المخزون:           {stats.InventoryValue:N2} جنيه");
                sb.AppendLine();

                if (stats.TodaysSales > 0)
                {
                    decimal profitMargin = (stats.TodaysProfit / stats.TodaysSales) * 100;
                    sb.AppendLine($"  📊 هامش الربح اليوم:       {profitMargin:F2}%");
                }

                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtbReport.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل التقرير:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSalesReport(RichTextBox rtb, DateTime startDate, DateTime endDate)
        {
            try
            {
                var (totalSales, totalProfit, invoiceCount) = _reportRepository.GetSalesByDateRange(startDate, endDate);
                var dailySales = _reportRepository.GetDailySalesSummary(startDate, endDate);
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                        تقرير المبيعات                           ");
                sb.AppendLine($"                    من {startDate:yyyy-MM-dd} إلى {endDate:yyyy-MM-dd}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                sb.AppendLine("📊 الملخص الإجمالي:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  💰 إجمالي المبيعات:        {totalSales:N2} جنيه");
                sb.AppendLine($"  📈 صافي الربح:             {totalProfit:N2} جنيه");
                sb.AppendLine($"  📄 عدد الفواتير:           {invoiceCount}");
                
                if (totalSales > 0)
                {
                    decimal profitMargin = (totalProfit / totalSales) * 100;
                    sb.AppendLine($"  📊 هامش الربح:             {profitMargin:F2}%");
                    sb.AppendLine($"  💵 متوسط الفاتورة:         {(invoiceCount > 0 ? totalSales / invoiceCount : 0):N2} جنيه");
                }
                sb.AppendLine();
                
                if (dailySales.Count > 0)
                {
                    sb.AppendLine("📅 المبيعات اليومية:");
                    sb.AppendLine("─────────────────────────────────────────────────────────────");
                    sb.AppendLine($"  {"التاريخ",-12} {"المبيعات",-15} {"الربح",-15} {"الفواتير",-10}");
                    sb.AppendLine("  " + new string('─', 55));
                    
                    foreach (var (date, sales, profit, count) in dailySales)
                    {
                        sb.AppendLine($"  {date:yyyy-MM-dd,-12} {sales,-15:N2} {profit,-15:N2} {count,-10}");
                    }
                }
                
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtb.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل تقرير المبيعات:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchasesReport(RichTextBox rtb, DateTime startDate, DateTime endDate)
        {
            try
            {
                var (totalPurchases, invoiceCount) = _reportRepository.GetPurchasesByDateRange(startDate, endDate);
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                        تقرير المشتريات                          ");
                sb.AppendLine($"                    من {startDate:yyyy-MM-dd} إلى {endDate:yyyy-MM-dd}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                sb.AppendLine("📊 الملخص الإجمالي:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  💰 إجمالي المشتريات:       {totalPurchases:N2} جنيه");
                sb.AppendLine($"  📄 عدد الفواتير:           {invoiceCount}");
                
                if (invoiceCount > 0)
                {
                    sb.AppendLine($"  💵 متوسط الفاتورة:         {totalPurchases / invoiceCount:N2} جنيه");
                }
                
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtb.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل تقرير المشتريات:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProfitReport(RichTextBox rtb, DateTime startDate, DateTime endDate)
        {
            try
            {
                var (totalSales, totalProfit, invoiceCount) = _reportRepository.GetSalesByDateRange(startDate, endDate);
                var (totalPurchases, purchaseCount) = _reportRepository.GetPurchasesByDateRange(startDate, endDate);
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                        تقرير الأرباح                            ");
                sb.AppendLine($"                    من {startDate:yyyy-MM-dd} إلى {endDate:yyyy-MM-dd}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                sb.AppendLine("💰 المبيعات:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  📈 إيرادات المبيعات:       {totalSales:N2} جنيه");
                sb.AppendLine($"  📊 صافي الربح:             {totalProfit:N2} جنيه");
                sb.AppendLine($"  📄 عدد الفواتير:           {invoiceCount}");
                sb.AppendLine();
                
                sb.AppendLine("💸 المشتريات:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  📉 إجمالي المصروفات:       {totalPurchases:N2} جنيه");
                sb.AppendLine($"  📄 عدد الفواتير:           {purchaseCount}");
                sb.AppendLine();
                
                sb.AppendLine("📊 التحليل:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                decimal netProfit = totalProfit - totalPurchases;
                sb.AppendLine($"  💵 الربح الصافي:           {netProfit:N2} جنيه");
                
                if (totalSales > 0)
                {
                    decimal profitMargin = (totalProfit / totalSales) * 100;
                    sb.AppendLine($"  📊 هامش الربح:             {profitMargin:F2}%");
                }
                
                if (totalPurchases > 0 && totalSales > 0)
                {
                    decimal roi = ((totalSales - totalPurchases) / totalPurchases) * 100;
                    sb.AppendLine($"  📈 العائد على الاستثمار:   {roi:F2}%");
                }
                
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtb.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل تقرير الأرباح:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadInventoryValueReport(RichTextBox rtb)
        {
            try
            {
                var (purchaseValue, saleValue, productCount) = _reportRepository.GetInventoryValue();
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                    تقرير قيمة المخزون                          ");
                sb.AppendLine($"                    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                sb.AppendLine("📦 قيمة المخزون:");
                sb.AppendLine("─────────────────────────────────────────────────────────────");
                sb.AppendLine($"  💰 قيمة الشراء:            {purchaseValue:N2} جنيه");
                sb.AppendLine($"  💵 قيمة البيع المتوقعة:    {saleValue:N2} جنيه");
                sb.AppendLine($"  📈 الربح المتوقع:          {(saleValue - purchaseValue):N2} جنيه");
                sb.AppendLine($"  📦 عدد المنتجات:           {productCount}");
                
                if (purchaseValue > 0)
                {
                    decimal potentialMargin = ((saleValue - purchaseValue) / purchaseValue) * 100;
                    sb.AppendLine($"  📊 هامش الربح المتوقع:     {potentialMargin:F2}%");
                }
                
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtb.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل تقرير المخزون:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLowStockReport(RichTextBox rtb)
        {
            try
            {
                var products = _reportRepository.GetLowStockReport();
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                تقرير المنتجات منخفضة المخزون                    ");
                sb.AppendLine($"                    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                if (products.Count == 0)
                {
                    sb.AppendLine("✅ لا توجد منتجات منخفضة المخزون");
                }
                else
                {
                    sb.AppendLine($"⚠️  عدد المنتجات منخفضة المخزون: {products.Count}");
                    sb.AppendLine();
                    sb.AppendLine($"  {"المنتج",-30} {"الكمية الحالية",-15} {"الحد الأدنى",-15} {"النقص",-10}");
                    sb.AppendLine("  " + new string('─', 70));
                    
                    foreach (var product in products)
                    {
                        string name = product.ProductNameArabic ?? product.ProductName;
                        if (name.Length > 28) name = name.Substring(0, 28);
                        int shortage = product.MinimumStock - product.CurrentStock;
                        sb.AppendLine($"  {name,-30} {product.CurrentStock,-15} {product.MinimumStock,-15} {shortage,-10}");
                    }
                }
                
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtb.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل التقرير:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOutOfStockReport(RichTextBox rtb)
        {
            try
            {
                var products = _reportRepository.GetOutOfStockProducts();
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                تقرير المنتجات نفذت من المخزون                   ");
                sb.AppendLine($"                    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                if (products.Count == 0)
                {
                    sb.AppendLine("✅ جميع المنتجات متوفرة في المخزون");
                }
                else
                {
                    sb.AppendLine($"🚫 عدد المنتجات نفذت من المخزون: {products.Count}");
                    sb.AppendLine();
                    sb.AppendLine($"  {"المنتج",-35} {"الفئة",-20} {"الحد الأدنى",-15}");
                    sb.AppendLine("  " + new string('─', 70));
                    
                    foreach (var product in products)
                    {
                        string name = product.ProductNameArabic ?? product.ProductName;
                        if (name.Length > 33) name = name.Substring(0, 33);
                        string category = product.CategoryNameArabic ?? product.CategoryName ?? "";
                        if (category.Length > 18) category = category.Substring(0, 18);
                        sb.AppendLine($"  {name,-35} {category,-20} {product.MinimumStock,-15}");
                    }
                }
                
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtb.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل التقرير:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopSellingReport(RichTextBox rtb)
        {
            try
            {
                var products = _reportRepository.GetTopSellingProducts(20);
                
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("                    أكثر 20 منتج مبيعاً                         ");
                sb.AppendLine($"                    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                if (products.Count == 0)
                {
                    sb.AppendLine("📊 لا توجد مبيعات حتى الآن");
                }
                else
                {
                    sb.AppendLine($"  {"#",-4} {"المنتج",-30} {"الكمية المباعة",-15} {"الإيرادات",-15}");
                    sb.AppendLine("  " + new string('─', 70));
                    
                    int rank = 1;
                    foreach (var (product, totalSold, totalRevenue) in products)
                    {
                        string name = product.ProductNameArabic ?? product.ProductName;
                        if (name.Length > 28) name = name.Substring(0, 28);
                        sb.AppendLine($"  {rank,-4} {name,-30} {totalSold,-15} {totalRevenue,-15:N2}");
                        rank++;
                    }
                }
                
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                rtb.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل التقرير:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"Report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt",
                    Title = "تصدير التقرير"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Get the active tab's report
                    string reportText = "";
                    if (tabReports.SelectedTab?.Controls.Count > 0)
                    {
                        foreach (Control ctrl in tabReports.SelectedTab.Controls)
                        {
                            if (ctrl is Panel panel)
                            {
                                foreach (Control pctrl in panel.Controls)
                                {
                                    if (pctrl is RichTextBox rtb)
                                    {
                                        reportText = rtb.Text;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    System.IO.File.WriteAllText(sfd.FileName, reportText, Encoding.UTF8);
                    MessageBox.Show("تم تصدير التقرير بنجاح!", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تصدير التقرير:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
