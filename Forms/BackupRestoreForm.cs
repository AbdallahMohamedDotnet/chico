using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChicoDesktopApp.Forms
{
    public partial class BackupRestoreForm : Form
    {
        private readonly BackupHelper _backupHelper;
        private readonly string _databasePath;

        private ListBox lstBackups = null!;
        private Button btnCreateBackup = null!;
        private Button btnRestoreBackup = null!;
        private Button btnDeleteBackup = null!;
        private Button btnOpenFolder = null!;
        private Button btnCleanup = null!;
        private Button btnClose = null!;
        private Label lblBackupInfo = null!;
        private Label lblDatabaseInfo = null!;

        public BackupRestoreForm(DatabaseHelper dbHelper)
        {
            _databasePath = dbHelper.GetDatabasePath();
            _backupHelper = new BackupHelper(_databasePath);

            InitializeComponent();
            LoadBackupList();
            UpdateDatabaseInfo();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "النسخ الاحتياطي والاستعادة - Chico ERP";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Segoe UI", 10F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title Panel
            var pnlTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(33, 150, 243),
                Padding = new Padding(20)
            };

            var lblTitle = new Label
            {
                Text = "🗄️ إدارة النسخ الاحتياطي",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlTitle.Controls.Add(lblTitle);

            // Database Info Panel
            var pnlDbInfo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            lblDatabaseInfo = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                ForeColor = Color.FromArgb(60, 60, 60),
                Text = "Loading..."
            };

            pnlDbInfo.Controls.Add(lblDatabaseInfo);

            // Main Panel
            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Backup list
            var lblBackupList = new Label
            {
                Text = "📋 النسخ الاحتياطية المتوفرة:",
                Location = new Point(0, 0),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            lstBackups = new ListBox
            {
                Location = new Point(0, 35),
                Size = new Size(740, 250),
                Font = new Font("Consolas", 9F),
                SelectionMode = SelectionMode.One
            };
            lstBackups.SelectedIndexChanged += LstBackups_SelectedIndexChanged;

            lblBackupInfo = new Label
            {
                Location = new Point(0, 290),
                Size = new Size(740, 40),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Text = "اختر نسخة احتياطية لعرض التفاصيل"
            };

            // Buttons Panel
            var pnlButtons = new Panel
            {
                Location = new Point(0, 340),
                Size = new Size(740, 100)
            };

            btnCreateBackup = new Button
            {
                Text = "💾 إنشاء نسخة احتياطية",
                Location = new Point(0, 0),
                Size = new Size(180, 45),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCreateBackup.FlatAppearance.BorderSize = 0;
            btnCreateBackup.Click += BtnCreateBackup_Click;

            btnRestoreBackup = new Button
            {
                Text = "♻️ استعادة النسخة",
                Location = new Point(190, 0),
                Size = new Size(180, 45),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnRestoreBackup.FlatAppearance.BorderSize = 0;
            btnRestoreBackup.Click += BtnRestoreBackup_Click;

            btnDeleteBackup = new Button
            {
                Text = "🗑️ حذف النسخة",
                Location = new Point(380, 0),
                Size = new Size(170, 45),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnDeleteBackup.FlatAppearance.BorderSize = 0;
            btnDeleteBackup.Click += BtnDeleteBackup_Click;

            btnCleanup = new Button
            {
                Text = "🧹 تنظيف القديمة",
                Location = new Point(0, 55),
                Size = new Size(180, 40),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(103, 58, 183),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCleanup.FlatAppearance.BorderSize = 0;
            btnCleanup.Click += BtnCleanup_Click;

            btnOpenFolder = new Button
            {
                Text = "📁 فتح المجلد",
                Location = new Point(190, 55),
                Size = new Size(180, 40),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOpenFolder.FlatAppearance.BorderSize = 0;
            btnOpenFolder.Click += BtnOpenFolder_Click;

            pnlButtons.Controls.AddRange(new Control[] { 
                btnCreateBackup, btnRestoreBackup, btnDeleteBackup, 
                btnCleanup, btnOpenFolder 
            });

            pnlMain.Controls.AddRange(new Control[] { 
                lblBackupList, lstBackups, lblBackupInfo, pnlButtons 
            });

            // Bottom Panel
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            btnClose = new Button
            {
                Text = "إغلاق",
                Size = new Size(120, 40),
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(btnClose);

            this.Controls.AddRange(new Control[] { pnlMain, pnlDbInfo, pnlTitle, pnlBottom });
            this.ResumeLayout(false);
        }

        private void LoadBackupList()
        {
            try
            {
                lstBackups.Items.Clear();
                var backups = _backupHelper.GetBackupFiles();

                if (backups.Length == 0)
                {
                    lstBackups.Items.Add("لا توجد نسخ احتياطية");
                    return;
                }

                foreach (var backup in backups)
                {
                    string fileName = Path.GetFileName(backup);
                    FileInfo fileInfo = new FileInfo(backup);
                    string size = $"{fileInfo.Length / 1024.0:F2} KB";
                    string date = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    lstBackups.Items.Add($"{fileName,-50} | {size,-10} | {date}");
                }

                lblBackupInfo.Text = $"📊 إجمالي النسخ: {backups.Length} | الحجم الكلي: {_backupHelper.GetTotalBackupSizeMB():F2} MB";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل قائمة النسخ:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDatabaseInfo()
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    FileInfo dbInfo = new FileInfo(_databasePath);
                    lblDatabaseInfo.Text = $"📂 قاعدة البيانات:\n" +
                                          $"   المسار: {_databasePath}\n" +
                                          $"   الحجم: {dbInfo.Length / 1024.0:F2} KB  |  آخر تعديل: {dbInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
                }
                else
                {
                    lblDatabaseInfo.Text = "⚠️ قاعدة البيانات غير موجودة!";
                }
            }
            catch (Exception ex)
            {
                lblDatabaseInfo.Text = $"خطأ: {ex.Message}";
            }
        }

        private void LstBackups_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool hasSelection = lstBackups.SelectedIndex >= 0 && 
                               lstBackups.Items.Count > 0 && 
                               !lstBackups.Items[0].ToString()!.Contains("لا توجد");

            btnRestoreBackup.Enabled = hasSelection;
            btnDeleteBackup.Enabled = hasSelection;

            if (hasSelection)
            {
                try
                {
                    var backups = _backupHelper.GetBackupFiles();
                    if (lstBackups.SelectedIndex < backups.Length)
                    {
                        string backupPath = backups[lstBackups.SelectedIndex];
                        FileInfo fileInfo = new FileInfo(backupPath);
                        lblBackupInfo.Text = $"📄 {Path.GetFileName(backupPath)}\n" +
                                           $"   الحجم: {fileInfo.Length / 1024.0:F2} KB  |  التاريخ: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}";
                    }
                }
                catch { }
            }
        }

        private void BtnCreateBackup_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "هل تريد إنشاء نسخة احتياطية من قاعدة البيانات؟\n\n" +
                    "سيتم حفظ النسخة في مجلد Backups",
                    "تأكيد النسخ الاحتياطي",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;
                    string backupPath = _backupHelper.CreateBackup();
                    this.Cursor = Cursors.Default;

                    MessageBox.Show(
                        $"✅ تم إنشاء النسخة الاحتياطية بنجاح!\n\n" +
                        $"الموقع: {Path.GetFileName(backupPath)}\n" +
                        $"الحجم: {_backupHelper.GetBackupSizeMB(backupPath):F2} MB",
                        "نجاح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LoadBackupList();
                    UpdateDatabaseInfo();
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"❌ خطأ في إنشاء النسخة الاحتياطية:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRestoreBackup_Click(object? sender, EventArgs e)
        {
            if (lstBackups.SelectedIndex < 0)
                return;

            try
            {
                var backups = _backupHelper.GetBackupFiles();
                if (lstBackups.SelectedIndex >= backups.Length)
                    return;

                string backupPath = backups[lstBackups.SelectedIndex];
                string backupName = Path.GetFileName(backupPath);

                var result = MessageBox.Show(
                    $"⚠️ تحذير: استعادة النسخة الاحتياطية ستستبدل قاعدة البيانات الحالية!\n\n" +
                    $"النسخة المحددة: {backupName}\n\n" +
                    $"سيتم إنشاء نسخة احتياطية من قاعدة البيانات الحالية قبل الاستعادة.\n\n" +
                    $"هل تريد المتابعة؟",
                    "تأكيد الاستعادة",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;
                    _backupHelper.RestoreBackup(backupPath);
                    this.Cursor = Cursors.Default;

                    MessageBox.Show(
                        "✅ تمت استعادة النسخة الاحتياطية بنجاح!\n\n" +
                        "يُنصح بإعادة تشغيل التطبيق للتأكد من تحميل البيانات الجديدة.",
                        "نجاح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LoadBackupList();
                    UpdateDatabaseInfo();
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"❌ خطأ في استعادة النسخة:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteBackup_Click(object? sender, EventArgs e)
        {
            if (lstBackups.SelectedIndex < 0)
                return;

            try
            {
                var backups = _backupHelper.GetBackupFiles();
                if (lstBackups.SelectedIndex >= backups.Length)
                    return;

                string backupPath = backups[lstBackups.SelectedIndex];
                string backupName = Path.GetFileName(backupPath);

                var result = MessageBox.Show(
                    $"هل تريد حذف النسخة الاحتياطية؟\n\n{backupName}\n\n" +
                    "لا يمكن التراجع عن هذا الإجراء!",
                    "تأكيد الحذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    File.Delete(backupPath);
                    MessageBox.Show("✅ تم حذف النسخة الاحتياطية", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadBackupList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في حذف النسخة:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCleanup_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "هل تريد حذف النسخ القديمة؟\n\n" +
                    "سيتم الاحتفاظ بآخر 10 نسخ احتياطية فقط",
                    "تأكيد التنظيف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int deletedCount = _backupHelper.CleanupOldBackups(10);
                    
                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"✅ تم حذف {deletedCount} نسخة قديمة", "نجاح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBackupList();
                    }
                    else
                    {
                        MessageBox.Show("لا توجد نسخ قديمة للحذف", "معلومة",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في التنظيف:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenFolder_Click(object? sender, EventArgs e)
        {
            try
            {
                string backupFolder = _backupHelper.GetBackupFolder();
                
                if (Directory.Exists(backupFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", backupFolder);
                }
                else
                {
                    MessageBox.Show("مجلد النسخ الاحتياطي غير موجود", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح المجلد:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
