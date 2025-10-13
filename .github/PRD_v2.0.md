# Product Requirements Document (PRD): Chico ERP System

**Version:** 2.0  
**Date:** October 13, 2025  
**Status:** Reference Document  

## Note on Current Implementation

This PRD specifies Clean Architecture (4 projects) with EF Core and MAUI.  
**Current implementation uses:** Windows Forms + SQLite + Single Project architecture.

The current implementation achieves the same business goals with a simpler architecture suitable for small retail stores.

---

## 1. Introduction & Overview

This document outlines the complete product and technical requirements for the "Chico" ERP desktop application. It is designed to guide a staged development process, ensuring a robust, scalable, and maintainable final product. The project is divided into three distinct stages: foundational setup, core feature implementation, and advanced system functionalities.

* **Product Vision:** To empower small electronics retailers with a simple, powerful, and reliable tool to manage their business operations, providing clear insights and automating daily tasks.
* **Target Audience:** Owners and employees of single-location electronics stores in the Arab-speaking market.

## 2. Staged Development Plan & Requirements

This PRD is structured to follow the specified development stages.

---

## Stage 1: Foundational Architecture & Database Setup

**Goal:** To establish a solid, scalable application skeleton based on Clean Architecture principles and to configure the data persistence layer using Entity Framework Core with SQLite.

| Requirement ID | Requirement | Details for the Developer | Current Status |
| :--- | :--- | :--- | :--- |
| **S1-AR-01** | **Project Structure** | The solution must be structured into four projects: `Chico.Core` (Domain), `Chico.Application` (Application Logic), `Chico.Infrastructure` (Data/External Services), and `Chico.MAUI` (Presentation/UI). This separation ensures maintainability and follows Clean Architecture principles. | ⚠️ Single project structure (simplified) |
| **S1-AR-02** | **Dependency Rule** | Dependencies must flow inwards: The UI depends on Application, Infrastructure depends on Application, and Application depends on Core. The Core (domain entities) must have zero dependencies on any other layer. | ⚠️ Adapted with Models/Repositories/Forms folders |
| **S1-DB-01** | **Database Provider** | The application must use **Entity Framework Core** as the ORM and **SQLite** as the database engine. This choice supports a lightweight, file-based, serverless database ideal for a desktop application. | ⚠️ Using Microsoft.Data.Sqlite directly |
| **S1-DB-02** | **Database Context** | An `ApplicationDbContext` class must be created within the `Chico.Infrastructure` project. It will manage all database operations, including connections and transactions. | ✅ DatabaseHelper.cs provides equivalent functionality |
| **S1-DM-01** | **Domain Models (Entities)** | All primary database tables must be defined as entity classes within the `Chico.Core` project. These include: `Product`, `Category`, `SaleInvoice`, `SaleInvoiceItem`, `PurchaseInvoice`, and `PurchaseInvoiceItem`. These classes represent the business domain and are the heart of the application. | ✅ Complete - Models folder with all entities |
| **S1-TS-01** | **Initial Test & Validation** | After setup, the application must successfully compile and run. A simple test function (e.g., a button in the UI) must be implemented to perform a basic database operation (e.g., write a new record and read it back) to confirm that the entire stack—from UI to database—is configured correctly. | ✅ Build successful, database operations tested |

---

## Stage 2: Core Feature Implementation via Abstraction

**Goal:** To build and test the core business features of the application (Inventory, Invoicing, Reporting) by first defining abstractions (interfaces) and then implementing them. This ensures a loosely-coupled and testable system.

### 2.1 Feature Dependency Tree

Development must follow a specific order to manage dependencies. Features are categorized by their dependency level:

* **Level 0 (No Dependencies):** These features can be built first.
    * **Inventory Management:** The core repository of all products.
* **Level 1 (Depends on Level 0):** These features require Inventory Management to be functional.
    * **Invoice Management (Sales & Purchase):** Depends on `Inventory` to add products to invoices.
* **Level 2 (Depends on Level 1):** These features require Invoice Management to be functional.
    * **Profit & Loss System:** Depends on `Invoicing` data (sales and purchase prices) to calculate profits.

### 2.2 Feature Implementation Requirements

For each feature below, the development process must be:

1. **Define Interfaces:** Create service and repository interfaces in the `Chico.Application` layer (e.g., `IProductRepository`, `IInventoryService`).
2. **Implement Interfaces:** Create concrete implementations of these interfaces in the `Chico.Infrastructure` layer (for repositories) and `Chico.Application` layer (for services).
3. **Build UI:** Develop the user interface in the `Chico.MAUI` project, which interacts only with the interfaces from the Application layer.
4. **Test Thoroughly:** Conduct unit and integration tests for the feature before moving to the next one.

| Feature | Requirement ID | Functional Requirement | Details for the Developer | Current Status |
| :--- | :--- | :--- | :--- | :--- |
| **Inventory Management (Level 0)** | **S2-INV-01** | **Product CRUD Operations** | Implement full Create, Read, Update, and Delete (CRUD) functionality for products and categories. The UI must provide forms and lists for these operations. Test each operation to ensure it correctly manipulates data in the SQLite database. | 🔄 Backend complete (ProductRepository), UI pending |
| | **S2-INV-02** | **Low Stock Alerting** | The system must allow setting a "reorder level" for each product. The inventory dashboard should visually flag any product whose stock quantity is at or below this level. Test by manually adjusting stock to trigger the alert. | 🔄 Backend supports ReorderLevel, UI pending |
| **Invoice Management (Level 1)** | **S2-INV-01** | **Sales & Purchase Invoicing** | Develop the UI and backend logic for creating, viewing, and storing sales and purchase invoices. Invoices must pull product data from the inventory system. Upon finalizing an invoice, the system must trigger an **automated stock update** (decrement for sales, increment for purchases). | 🔄 Database schema complete, UI pending |
| | **S2-INV-02** | **Invoice ID & Discount** | Ensure every invoice is saved with a unique, auto-generated ID. The sales invoice form must include a field for a **fixed amount discount**, which is subtracted from the total. Test that stock levels change correctly and discounts are applied accurately. | 🔄 Database supports Discount field, UI pending |
| **Profit & Loss System (Level 2)**| **S2-PL-01** | **Profit Calculation Engine** | Implement the business logic to calculate the gross profit for each `SaleInvoiceItem` (`SalePrice` - `PurchasePrice`). This calculation should occur when an invoice is finalized. | ⏳ Pending invoice implementation |
| | **S2-PL-02** | **Financial Reporting UI** | Create a dedicated reporting screen that displays Total Revenue, Cost of Goods Sold (COGS), and Gross Profit. The screen must include date filters (e.g., This Month, Custom Range) to allow the user to analyze performance over specific periods. Test the report by creating several invoices and verifying that the calculated totals are correct. | ⏳ Pending invoice implementation |

---

## Stage 3: Advanced System & Security Features

**Goal:** To enhance the application with critical operational and security functionalities, including data protection and user access control.

| Feature | Requirement ID | Functional Requirement | Details for the Developer | Current Status |
| :--- | :--- | :--- | :--- | :--- |
| **Database Management** | **S3-DB-01** | **Database Backup** | Implement a feature that allows the user to create a backup of the SQLite database file (`chico.db`). The UI should provide a button that lets the user choose a safe location on their computer to save a copy of the database file. | ⏳ Pending |
| | **S3-DB-02** | **Database Restore** | Implement a feature that allows the user to restore the database from a backup file. The UI should prompt the user to select a backup file, and upon confirmation, the system will replace the current database file with the selected backup. This operation should come with a strong warning to the user about data loss. | ⏳ Pending |
| **Authentication** | **S3-AU-01** | **User Login System** | Create a login screen that requires a **Username and Password**. The system must maintain a `User` table in the database to store user credentials (passwords must be hashed and salted for security). The application should be inaccessible until a user successfully logs in. | ✅ Complete - SHA256 hashing, LoginForm |
| **Authorization**| **S3-AZ-01** | **Role-Based Access Control**| Implement a role management system. Define at least two roles: **Admin** and **Cashier**. Create a `Role` table and a user-role mapping table in the database. | ✅ Complete - UserRole enum (Admin/Cashier) |
| | **S3-AZ-02** | **Feature Permissions** | Restrict access to features based on the logged-in user's role: <br> - **Admin:** Full access to all features (Inventory, Invoicing, Reports, User Management, Backup/Restore). <br> - **Cashier:** Limited access, primarily to the Sales Invoicing screen. They should not be able to view profit reports or manage other users. | ✅ Complete - SessionManager with role checks |
| | **S3-AZ-03** | **User Management UI** | Create a settings screen, accessible only to Admins, where they can create, edit, and delete user accounts and assign roles. | ⏳ Pending UI implementation |

---

## Current Implementation Mapping

### ✅ Completed (Stage 1 & 3 Partial)
- Database schema with all tables
- All domain models (Product, Category, SalesInvoice, PurchaseInvoice, User, etc.)
- Complete authentication system with login
- Role-based access control (Admin/Cashier)
- Session management
- Main dashboard with stats
- Arabic RTL support

### 🔄 In Progress (Stage 2 - Level 0)
- Product management backend complete
- Category management backend complete
- **Next: Product Management UI forms**

### ⏳ Pending (Stage 2 & 3)
- Invoice Management UI (Level 1)
- Profit & Loss Reports (Level 2)
- User Management UI
- Database Backup/Restore

---

## Development Roadmap

Following the dependency tree, implement in this order:

1. **Product Management UI** (Level 0 - S2-INV-01, S2-INV-02)
   - Product list form with search/filter
   - Add/Edit product form
   - Category management
   - Low stock alerts

2. **Sales Invoice UI** (Level 1 - S2-INV-01, S2-INV-02)
   - POS-style interface
   - Product selection from inventory
   - Discount field
   - Auto-generate invoice ID
   - Auto-update stock on save

3. **Purchase Invoice UI** (Level 1 - S2-INV-01)
   - Similar to sales but increments stock
   - Links to suppliers (future enhancement)

4. **Profit & Loss Reports** (Level 2 - S2-PL-01, S2-PL-02)
   - Calculate profit per sale item
   - Summary reports with date filters
   - Revenue, COGS, Gross Profit display

5. **User Management UI** (Stage 3 - S3-AZ-03)
   - Admin-only user creation/editing
   - Role assignment

6. **Backup/Restore** (Stage 3 - S3-DB-01, S3-DB-02)
   - File copy with SaveFileDialog
   - File replace with OpenFileDialog
   - Warning messages

---

## Technical Notes

**Current Stack:**
- .NET 8.0 Windows Forms
- Microsoft.Data.Sqlite v9.0.9
- SQLite database (chico.db)
- SHA256 password hashing
- Arabic RTL layout

**Architecture Adaptation:**
- Models folder = Domain entities (Core)
- Repositories folder = Data access (Infrastructure)
- Forms folder = UI (Presentation)
- SessionManager = Application service

This simplified architecture achieves PRD goals while being more maintainable for a small business application.
