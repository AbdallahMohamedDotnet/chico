# 🧪 TESTING GUIDE - Complete Lifecycle Test

## Test Scenario: Full Application Lifecycle

### Prerequisites
- Application built successfully ✅
- Database initialized ✅
- Default admin user created ✅

---

## 1️⃣ LOGIN & AUTHENTICATION TEST

**Test Steps:**
```
1. Run: dotnet run
2. Login with:
   - Username: admin
   - Password: admin123
3. Expected: Dashboard opens
```

**Verification:**
- ✅ Login form appears
- ✅ Authentication succeeds
- ✅ Dashboard loads
- ✅ Username displays in header
- ✅ Current date/time shows

---

## 2️⃣ DASHBOARD STATISTICS TEST

**Test Steps:**
```
1. View dashboard statistics
2. Note the numbers displayed
```

**Expected Results:**
- 📦 Total Products: 35 (from seed data)
- ⚠️ Low Stock: 0-5 (varies based on seed data)
- 💰 Today's Sales: 0.00 (no sales yet)

**Verification:**
- ✅ Stats load without errors
- ✅ Numbers are accurate
- ✅ Arabic text displays correctly

---

## 3️⃣ PRODUCT MANAGEMENT TEST

**Test Steps:**
```
1. Click "📦 إدارة المنتجات"
2. Add new product:
   - Name: Test Product
   - Name Arabic: منتج تجريبي
   - Category: Electronics
   - Purchase Price: 100
   - Sale Price: 150
   - Current Stock: 20
   - Minimum Stock: 5
3. Save product
4. Search for "Test Product"
5. Edit the product (change price)
6. Delete the test product
```

**Verification:**
- ✅ Product list loads
- ✅ Search works
- ✅ Add form opens
- ✅ Product saves successfully
- ✅ Profit margin calculates
- ✅ Edit works
- ✅ Delete works (admin only)

---

## 4️⃣ SALES INVOICE TEST

**Test Steps:**
```
1. Click "💰 فاتورة بيع"
2. Select product: "Laptop Dell XPS"
3. Set quantity: 1
4. Click "Add to Invoice"
5. Enter customer details:
   - Name: Test Customer
   - Phone: 1234567890
6. Apply discount: 50 (Percentage: 5%)
7. Complete sale
8. Note invoice number
```

**Verification:**
- ✅ Product list loads
- ✅ Search/filter works
- ✅ Add to invoice works
- ✅ Totals calculate correctly
- ✅ Discount applies
- ✅ Stock decreases
- ✅ Invoice saves
- ✅ Success message shows

**Expected Calculations:**
```
Product: Laptop Dell XPS
Sale Price: 3200.00
Quantity: 1
Subtotal: 3200.00
Discount (5%): -160.00
Total: 3040.00
Profit: ~700.00 (depends on purchase price)
```

---

## 5️⃣ PURCHASE INVOICE TEST

**Test Steps:**
```
1. Click "📥 فاتورة شراء"
2. Select product: "Wireless Mouse"
3. Set quantity: 10
4. Set unit cost: 20.00
5. Add to invoice
6. Enter supplier: "Tech Supplies"
7. Complete purchase
```

**Verification:**
- ✅ Product list loads
- ✅ Add to invoice works
- ✅ Total calculates (10 × 20 = 200.00)
- ✅ Stock increases by 10
- ✅ Invoice saves

---

## 6️⃣ DASHBOARD REFRESH TEST

**Test Steps:**
```
1. Return to dashboard
2. Check updated statistics
```

**Expected Results:**
- 💰 Today's Sales: 3040.00 (from sales test)
- Stats updated automatically
- Product count still 35

**Verification:**
- ✅ Today's sales shows correct amount
- ✅ Stats reflect recent transactions

---

## 7️⃣ REPORTS SYSTEM TEST

### 7.1 Dashboard Report
```
1. Click "📊 التقارير"
2. View dashboard report (auto-loads)
```

**Verification:**
- ✅ Today's sales: 3040.00
- ✅ Today's profit shown
- ✅ Inventory value calculated
- ✅ Product counts accurate

### 7.2 Sales Report
```
1. Click "تقارير المبيعات" tab
2. Set date range: Last 30 days
3. Generate report
```

**Verification:**
- ✅ Shows today's sale
- ✅ Daily breakdown displayed
- ✅ Profit margins calculated
- ✅ Invoice count correct

### 7.3 Inventory Report
```
1. Click "تقارير المخزون" tab
2. Click "📦 قيمة المخزون الإجمالية"
3. Click "⚠️ منتجات منخفضة المخزون"
4. Click "🏆 أكثر المنتجات مبيعاً"
```

**Verification:**
- ✅ Inventory value shows
- ✅ Low stock products listed
- ✅ Top selling shows "Laptop Dell XPS"

### 7.4 Profit Report
```
1. Click "تقارير الأرباح" tab
2. Set date range: Today
3. Generate report
```

**Verification:**
- ✅ Sales revenue shown
- ✅ Purchase expenses shown
- ✅ Net profit calculated
- ✅ ROI percentage displayed

### 7.5 Export Report
```
1. Click "📊 تصدير إلى ملف نصي"
2. Save as: test_report.txt
3. Open file
```

**Verification:**
- ✅ File saves successfully
- ✅ Content matches screen
- ✅ Arabic text renders correctly
- ✅ Formatting preserved

---

## 8️⃣ BACKUP SYSTEM TEST (Admin Only)

### 8.1 Create Backup
```
1. Click "🗄️ نسخ احتياطي"
2. View current backups (initially empty)
3. Click "💾 إنشاء نسخة احتياطية"
4. Confirm
5. Wait for success message
```

**Verification:**
- ✅ Backup form opens
- ✅ Current DB info displays
- ✅ Backup creates successfully
- ✅ File appears in list with timestamp
- ✅ File size shown (>70 KB)
- ✅ Backup folder contains file

### 8.2 View Backup Details
```
1. Click on created backup
2. View details
```

**Verification:**
- ✅ File name displays
- ✅ Size shown correctly
- ✅ Creation date/time shown
- ✅ Restore button enabled

### 8.3 Create Multiple Backups
```
1. Create 3 more backups (wait 1 second between each)
2. View backup list
```

**Verification:**
- ✅ All backups listed
- ✅ Sorted by date (newest first)
- ✅ Total count: 4 backups
- ✅ Total size calculated

### 8.4 Delete Backup
```
1. Select oldest backup
2. Click "🗑️ حذف النسخة"
3. Confirm
```

**Verification:**
- ✅ Confirmation dialog appears
- ✅ Backup deletes successfully
- ✅ List updates
- ✅ Count decreases

### 8.5 Open Backup Folder
```
1. Click "📁 فتح المجلد"
```

**Verification:**
- ✅ Explorer opens
- ✅ Shows Backups folder
- ✅ Files visible in Explorer

### 8.6 Cleanup Old Backups
```
1. Create 12 total backups
2. Click "🧹 تنظيف القديمة"
3. Confirm (keep last 10)
```

**Verification:**
- ✅ Cleanup runs
- ✅ Shows "تم حذف 2 نسخة قديمة"
- ✅ Only 10 backups remain
- ✅ Newest 10 kept

---

## 9️⃣ USER MANAGEMENT TEST (Admin Only)

**Test Steps:**
```
1. Click "👥 المستخدمين"
2. Add new user:
   - Username: cashier1
   - Password: cash123
   - Full Name: Test Cashier
   - Role: Cashier
3. Save user
4. Edit user (change name)
5. Reset password
6. Test new password
```

**Verification:**
- ✅ User form opens
- ✅ User saves
- ✅ Edit works
- ✅ Reset password works
- ✅ Can login with new credentials

---

## 🔟 RESTORE BACKUP TEST (Critical!)

**⚠️ WARNING: This will replace current database!**

**Test Steps:**
```
1. Click "🗄️ نسخ احتياطي"
2. Select a backup from before sales test
3. Click "♻️ استعادة النسخة"
4. Read warning carefully
5. Confirm restore
6. Wait for success message
7. Close and reopen application
```

**Verification:**
- ✅ Pre-restore backup created
- ✅ Restore succeeds
- ✅ Dashboard stats revert (Today's Sales = 0)
- ✅ Recent sale no longer exists
- ✅ Application works normally
- ✅ Data integrity maintained

---

## 1️⃣1️⃣ LOGOUT & RE-LOGIN TEST

**Test Steps:**
```
1. Click "🚪 تسجيل الخروج"
2. Confirm logout
3. Login form appears
4. Login again with admin/admin123
```

**Verification:**
- ✅ Logout confirmation shows
- ✅ Returns to login
- ✅ Session cleared
- ✅ Re-login works
- ✅ Dashboard loads fresh

---

## 1️⃣2️⃣ ROLE-BASED ACCESS TEST

### Test as Cashier
```
1. Logout from admin
2. Login as: cashier1 / cash123
3. Try to access:
   - Products ✅ (should work)
   - Sales ✅ (should work)
   - Purchases ✅ (should work)
   - Reports ✅ (should work)
   - Users ❌ (should show error)
   - Backup ❌ (should show error)
```

**Verification:**
- ✅ Cashier can access allowed features
- ✅ Blocked from admin features
- ✅ Error messages display
- ✅ No unauthorized access

---

## 📊 TEST RESULTS SUMMARY

### ✅ Passed Tests
- [ ] Login & Authentication
- [ ] Dashboard Statistics
- [ ] Product Management (CRUD)
- [ ] Sales Invoice Processing
- [ ] Purchase Invoice Processing
- [ ] Dashboard Refresh
- [ ] Reports - Dashboard
- [ ] Reports - Sales
- [ ] Reports - Inventory
- [ ] Reports - Profit
- [ ] Report Export
- [ ] Backup Creation
- [ ] Backup Details
- [ ] Backup Deletion
- [ ] Backup Cleanup
- [ ] Open Backup Folder
- [ ] Backup Restore
- [ ] User Management
- [ ] Logout & Re-login
- [ ] Role-Based Access Control

### ❌ Failed Tests
(List any failures here)

---

## 🐛 ISSUES FOUND
(Document any bugs or issues discovered during testing)

**Example Format:**
```
Issue #1: [Title]
Severity: Low/Medium/High
Description: ...
Steps to Reproduce: ...
Expected: ...
Actual: ...
```

---

## 📝 TEST NOTES

### Performance
- Application startup: _____ seconds
- Dashboard load: _____ seconds
- Report generation: _____ seconds
- Backup creation: _____ seconds
- Backup restore: _____ seconds

### Data Integrity
- [ ] Stock updates correctly
- [ ] Prices calculate accurately
- [ ] Reports match database
- [ ] Backup/restore preserves data

### User Experience
- [ ] Interface is intuitive
- [ ] Arabic text displays properly
- [ ] Error messages are clear
- [ ] Confirmation dialogs work
- [ ] Forms validate input

---

## ✅ SIGN-OFF

**Tester Name:** _________________  
**Date:** _________________  
**Overall Result:** ⭕ PASS / FAIL  

**Comments:**
___________________________________________
___________________________________________
___________________________________________

---

## 🚀 READY FOR PRODUCTION?

**Checklist:**
- [ ] All tests passed
- [ ] No critical bugs
- [ ] Performance acceptable
- [ ] Data integrity verified
- [ ] Backup/restore tested
- [ ] Security validated
- [ ] User experience approved

**Status:** ⭕ READY / NOT READY

