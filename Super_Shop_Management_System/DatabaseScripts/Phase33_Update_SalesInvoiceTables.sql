USE SuperShopDB;
GO

-- Phase 3.2/3.3 compatibility: ensure Sales has required columns
IF OBJECT_ID('dbo.Sales', 'U') IS NULL
BEGIN
    THROW 50000, 'dbo.Sales table is missing.', 1;
END
GO

IF COL_LENGTH('dbo.Sales', 'CustomerID') IS NULL
    ALTER TABLE dbo.Sales ADD CustomerID INT NULL;
GO

IF COL_LENGTH('dbo.Sales', 'EmployeeID') IS NULL
    ALTER TABLE dbo.Sales ADD EmployeeID INT NULL;
GO

IF COL_LENGTH('dbo.Sales', 'Discount') IS NULL
    ALTER TABLE dbo.Sales ADD Discount DECIMAL(18,2) NULL;
GO

IF COL_LENGTH('dbo.Sales', 'VAT') IS NULL
    ALTER TABLE dbo.Sales ADD VAT DECIMAL(18,2) NULL;
GO

IF COL_LENGTH('dbo.Sales', 'GrandTotal') IS NULL
    ALTER TABLE dbo.Sales ADD GrandTotal DECIMAL(18,2) NULL;
GO

IF COL_LENGTH('dbo.Sales', 'PaymentMethod') IS NULL
    ALTER TABLE dbo.Sales ADD PaymentMethod NVARCHAR(50) NULL;
GO

IF COL_LENGTH('dbo.Sales', 'PaymentStatus') IS NULL
    ALTER TABLE dbo.Sales ADD PaymentStatus NVARCHAR(20) NULL;
GO

-- Fill defaults for historical/seed rows so joins/prints don't fail
DECLARE @AnyEmployeeID INT = (SELECT TOP 1 EmployeeID FROM dbo.Employees ORDER BY EmployeeID);
IF @AnyEmployeeID IS NULL SET @AnyEmployeeID = 0;

UPDATE dbo.Sales
SET
    EmployeeID = ISNULL(EmployeeID, @AnyEmployeeID),
    Discount = ISNULL(Discount, 0),
    VAT = ISNULL(VAT, 0),
    GrandTotal = ISNULL(GrandTotal, ISNULL(TotalAmount, 0)),
    PaymentMethod = ISNULL(PaymentMethod, 'Cash'),
    PaymentStatus = ISNULL(PaymentStatus, 'Paid');
GO

-- Ensure SalesDetails exists with required columns
IF OBJECT_ID('dbo.SalesDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesDetails
    (
        SalesDetailID INT IDENTITY(1,1) PRIMARY KEY,
        SaleID INT NOT NULL,
        ProductID INT NOT NULL,
        Quantity INT NOT NULL CHECK (Quantity > 0),
        UnitPrice DECIMAL(18,2) NOT NULL,
        SubTotal DECIMAL(18,2) NOT NULL
    );
END
GO

-- Add missing columns if table already exists partially
IF OBJECT_ID('dbo.SalesDetails', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.SalesDetails', 'SaleID') IS NULL
        ALTER TABLE dbo.SalesDetails ADD SaleID INT NULL;
    GO
    IF COL_LENGTH('dbo.SalesDetails', 'ProductID') IS NULL
        ALTER TABLE dbo.SalesDetails ADD ProductID INT NULL;
    GO
    IF COL_LENGTH('dbo.SalesDetails', 'Quantity') IS NULL
        ALTER TABLE dbo.SalesDetails ADD Quantity INT NULL;
    GO
    IF COL_LENGTH('dbo.SalesDetails', 'UnitPrice') IS NULL
        ALTER TABLE dbo.SalesDetails ADD UnitPrice DECIMAL(18,2) NULL;
    GO
    IF COL_LENGTH('dbo.SalesDetails', 'SubTotal') IS NULL
        ALTER TABLE dbo.SalesDetails ADD SubTotal DECIMAL(18,2) NULL;
    GO
END
GO

-- Optional: ensure Products.StockQuantity is non-negative (already enforced in Products)
-- SELECT TOP 10 * FROM dbo.Sales ORDER BY SaleID DESC;

