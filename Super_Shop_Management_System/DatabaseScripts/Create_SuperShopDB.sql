IF DB_ID('SuperShopDB') IS NULL
BEGIN
    CREATE DATABASE SuperShopDB;
END
GO

USE SuperShopDB;
GO

IF OBJECT_ID('dbo.Sales', 'U') IS NOT NULL DROP TABLE dbo.Sales;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Username NVARCHAR(60) NOT NULL UNIQUE,
    Password NVARCHAR(256) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Employee')),
    Email NVARCHAR(150) NOT NULL UNIQUE,
    SecurityQuestion NVARCHAR(250) NOT NULL,
    SecurityAnswer NVARCHAR(256) NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE dbo.Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(120) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE dbo.Products
(
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(180) NOT NULL,
    Barcode NVARCHAR(80) NOT NULL UNIQUE,
    CategoryID INT NOT NULL,
    PurchasePrice DECIMAL(18,2) NOT NULL CHECK (PurchasePrice > 0),
    SellingPrice DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL CHECK (StockQuantity >= 0),
    ReorderLevel INT NOT NULL CHECK (ReorderLevel >= 0),
    ExpiryDate DATE NULL,
    SupplierName NVARCHAR(180) NULL,
    Description NVARCHAR(500) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_Products_SellingPrice_GreaterOrEqual
        CHECK (SellingPrice >= PurchasePrice),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID)
);
GO

CREATE TABLE dbo.Sales
(
    SaleID INT IDENTITY(1,1) PRIMARY KEY,
    TotalAmount DECIMAL(18,2) NOT NULL CHECK (TotalAmount >= 0),
    SaleDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

INSERT INTO dbo.Users
(FullName, Username, Password, Role, Email, SecurityQuestion, SecurityAnswer, CreatedDate)
VALUES
('System Administrator', 'admin', 'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7', 'Admin', 'admin@supershop.local', 'What is your favorite color?', '16477688c0e00699c6cfa4497a3612d7e83c532062b64b250fed8908128ed548', GETDATE()),
('Store Employee', 'employee', 'b4bd29480ab196faa782e0d4ecd10c2f4212814105227e5f7992f5bf4b212a64', 'Employee', 'employee@supershop.local', 'What is your pet name?', 'f15c16b99f82d8201767d3a841ff40849c8a1b812ffbfd2e393d2b6aa6682a6e', GETDATE());
GO

INSERT INTO dbo.Categories (CategoryName, Description, CreatedDate)
VALUES ('Groceries', 'Daily grocery essentials', GETDATE()),
       ('Beverages', 'Drinks and refreshments', GETDATE()),
       ('Personal Care', 'Health and hygiene products', GETDATE());
GO

INSERT INTO dbo.Products
(ProductName, Barcode, CategoryID, PurchasePrice, SellingPrice, StockQuantity, ReorderLevel, ExpiryDate, SupplierName, Description, CreatedDate)
VALUES
('Premium Rice 5kg', '890100000001', 1, 500.00, 580.00, 24, 8, '2027-12-31', 'Global Agro', 'High-quality premium rice', GETDATE()),
('Orange Juice 1L', '890100000002', 2, 90.00, 120.00, 10, 12, '2026-11-30', 'Fresh Sip Ltd.', 'Natural orange juice', GETDATE()),
('Hand Wash 250ml', '890100000003', 3, 55.00, 75.00, 40, 10, NULL, 'CleanCare', 'Antibacterial hand wash', GETDATE());
GO

INSERT INTO dbo.Sales (TotalAmount, SaleDate)
VALUES (1250.00, GETDATE()),
       (875.00, GETDATE()),
       (9950.00, DATEADD(DAY, -10, GETDATE()));
GO
