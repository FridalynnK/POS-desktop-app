# POS-desktop-app
# POS System — Point of Sale (POS.Core)

A layered .NET 8 Windows desktop Point-of-Sale application built with WPF, Entity Framework Core, and SQL Server.

---

## Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Core Domain](#core-domain)
- [Services](#services)
- [DTOs](#dtos)
- [Interfaces](#interfaces)
- [Database](#database)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Key Workflows](#key-workflows)
- [Planned / Stub Components](#planned--stub-components)

---

## Overview

POS System is a desktop POS solution targeted at small retail businesses. It handles:

- Product catalogue and inventory tracking
- Sale creation with automatic stock deduction
- Customer management with debt/advance/installment balance tracking
- Payment recording against customer balances
- Receipt printing via the Windows printing API
- Sales and inventory reporting (in progress)

---

## Project Structure

```
POSSystem/
├── POS.Core/               # Domain layer — entities, interfaces, DTOs, enums
│   ├── DTOs/
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
├── POS.Data/               # Data access layer — EF Core DbContext, migrations
│   └── Context/
│       └── PosDbContext
└── POS.Services/           # Business logic layer
    ├── Products/
    ├── Sales/
    ├── Payments/
    └── Reports/
```

---

## Architecture

The solution follows a classic **3-layer architecture**:

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `POS.Core` | Entities, interfaces, DTOs, enums — no dependencies |
| Data | `POS.Data` | EF Core `DbContext`, migrations, database access |
| Services | `POS.Services` | Business logic, orchestration, printing |
| UI | *(WPF project)* | Windows desktop interface |

Dependency direction: UI → Services → Data → Core (domain has no outward dependencies).

---

## Core Domain

### Entities

| Entity | Description |
|---|---|
| `Product` | Catalogue item with SKU, pricing, stock quantity, reorder level, and expiry |
| `Sale` | A completed transaction with reference number, total, and payment method |
| `SaleItem` | Line item linking a `Sale` to a `Product` with quantity, unit price, and line total |
| `Customer` | Customer record with contact info; linked to sales and balances |
| `CustomerBalance` | Tracks outstanding amounts per sale — supports Debt, Advance, and Installment types |
| `Payment` | A payment made against a `CustomerBalance` |
| `User` | System user (Admin or Cashier) with hashed password and role |

### Enums

| Enum | Values |
|---|---|
| `UserRole` | `Admin`, `Cashier` |
| `BalanceType` | *(stub — Debt / Advance / Installment)* |
| `PaymentMethod` | *(stub — Cash / Card / Mobile Money etc.)* |

---

## Services

### `SaleService` — `POS.Services.Sales`

Implements `ISaleService`. Handles the full sale creation workflow inside a database transaction:

1. Load requested products from the database.
2. Validate all products exist.
3. Check each product has sufficient stock.
4. Calculate the order total using current selling prices.
5. Persist the `Sale` record and obtain its ID.
6. Persist each `SaleItem` and deduct stock from the product.
7. Commit the transaction (or roll back on failure).

Returns the new `Sale.Id` on success.

### `ProductService` — `POS.Services.Products`

Implements `IProductService`. Retrieves the full product list from the database via EF Core.

### `ReceiptService` — `POS.Services.Sales`

Implements `IReceiptService`. Loads a sale with its items and products, formats a plain-text receipt, and sends it to the Windows print queue using `System.Drawing.Printing`.

### `PaymentService` / `DebtService` — `POS.Services.Payments`

Stubs — not yet implemented.

### `SalesReportService` / `InventoryReportService` — `POS.Services.Reports`

Stubs — not yet implemented.

---

## DTOs

| DTO | Purpose |
|---|---|
| `SaleRequestDto` | Input for creating a sale — cashier ID, optional customer ID, payment method, list of items |
| `SaleItemDto` | Line item input — product ID, name, quantity, unit price; computes `LineTotal` |
| `SaleResultDto` | *(stub)* — intended to carry sale result data back to the UI |
| `ReceiptDto` | *(stub)* |
| `PaymentDto` | *(stub)* |

---

## Interfaces

| Interface | Implemented By |
|---|---|
| `ISaleService` | `SaleService` |
| `IProductService` | `ProductService` |
| `IReceiptService` | `ReceiptService` |
| `IPaymentService` | *(stub)* |
| `IInventoryService` | *(stub)* |

---

## Database

- **Engine:** Microsoft SQL Server (SQL Express)
- **ORM:** Entity Framework Core 8
- **Connection string location:** `App.config`

Default connection string:

```xml
<add name="PosSystemDB"
     connectionString="Data source=DESKTOP-EMHUU2O\SQLEXPRESS;Database=POSSystemDB;Trusted_Connection=True;TrustServerCertificate=True;"
     providerName="System.Data.SqlClient" />
```

Update the `Data source` value to match your SQL Server instance name before running.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) (`8.0.418` or later via `latestFeature` roll-forward)
- SQL Server or SQL Server Express
- Windows OS (project targets `net8.0-windows` / WPF)
- Visual Studio 2022+ or JetBrains Rider

---

## Getting Started

1. **Clone the repository**

   ```bash
   git clone <repo-url>
   cd POSSystem
   ```

2. **Update the connection string** in `POS.Core/App.config` (or the consuming project's config) to point to your SQL Server instance.

3. **Apply database migrations** (from the solution root):

   ```bash
   dotnet ef database update --project POS.Data --startup-project <UI-project>
   ```

4. **Build the solution**

   ```bash
   dotnet build
   ```

5. **Run the WPF application** from Visual Studio or:

   ```bash
   dotnet run --project <UI-project>
   ```

---

## Configuration

| Setting | Location | Description |
|---|---|---|
| Database connection | `App.config` → `connectionStrings` | SQL Server connection string |
| SDK version | `global.json` | Pins to .NET 8, allows `latestFeature` roll-forward |

---

## Key Workflows

### Creating a Sale

```
UI collects items
  → builds SaleRequestDto (CashierId, CustomerId?, PaymentMethod, Items[])
  → calls ISaleService.CreateSaleAsync(request)
    → validates products & stock
    → persists Sale + SaleItems
    → deducts inventory
    → returns Sale.Id
  → calls IReceiptService.PrintReceiptAsync(saleId)
    → loads sale with items
    → prints to default printer
```

### Customer Balance

When a sale is made on credit, a `CustomerBalance` record is created linked to the sale. Subsequent `Payment` records reduce the `Outstanding` amount on that balance. Balance types supported: **Debt**, **Advance**, **Installment**.

---

## Planned / Stub Components

The following components exist as stubs and are pending implementation:

- `PaymentService` — record and apply customer payments
- `DebtService` — manage overdue debts and alerts
- `SalesReportService` — daily/weekly/monthly sales summaries
- `InventoryReportService` — low-stock alerts, reorder reports
- `BalanceType` enum — fully define balance categories
- `PaymentMethod` enum — Cash, Mobile Money, Card, etc.
- `SaleResultDto` / `ReceiptDto` / `PaymentDto` — complete DTO definitions
- `IInventoryService` / `IPaymentService` — define and implement interfaces
