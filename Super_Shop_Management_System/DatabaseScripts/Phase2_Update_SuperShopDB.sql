USE SuperShopDB;
GO

IF OBJECT_ID('dbo.Attendance', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attendance
    (
        AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeID INT NOT NULL,
        Date DATE NOT NULL,
        CheckIn DATETIME NULL,
        CheckOut DATETIME NULL,
        Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Present', 'Absent', 'Leave')),
        CONSTRAINT UQ_Attendance_Employee_Date UNIQUE (EmployeeID, Date)
    );
END
GO

IF OBJECT_ID('dbo.SalaryRecords', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalaryRecords
    (
        SalaryRecordID INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeID INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
        PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
        Notes NVARCHAR(250) NULL
    );
END
GO

IF OBJECT_ID('dbo.Employees', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(30) NOT NULL,
        Email NVARCHAR(150) NULL,
        Address NVARCHAR(300) NULL,
        Role NVARCHAR(30) NOT NULL,
        Salary DECIMAL(18,2) NOT NULL CHECK (Salary > 0),
        Shift NVARCHAR(30) NOT NULL,
        JoinDate DATE NOT NULL,
        Username NVARCHAR(60) NOT NULL UNIQUE,
        Password NVARCHAR(256) NOT NULL
    );
END
GO

IF OBJECT_ID('dbo.CustomerPurchases', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerPurchases
    (
        CustomerPurchaseID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
        PurchaseDate DATETIME NOT NULL DEFAULT GETDATE(),
        Notes NVARCHAR(250) NULL
    );
END
GO

IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        CustomerID INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(30) NOT NULL,
        Email NVARCHAR(150) NULL,
        Address NVARCHAR(300) NULL,
        LoyaltyPoints INT NOT NULL DEFAULT 0 CHECK (LoyaltyPoints >= 0),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID('dbo.SupplierTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupplierTransactions
    (
        SupplierTransactionID INT IDENTITY(1,1) PRIMARY KEY,
        SupplierID INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        Notes NVARCHAR(250) NULL
    );
END
GO

IF OBJECT_ID('dbo.Suppliers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Suppliers
    (
        SupplierID INT IDENTITY(1,1) PRIMARY KEY,
        SupplierName NVARCHAR(150) NOT NULL,
        CompanyName NVARCHAR(150) NULL,
        Phone NVARCHAR(30) NOT NULL,
        Email NVARCHAR(150) NULL,
        Address NVARCHAR(300) NULL,
        ProductType NVARCHAR(120) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CustomerPurchases_Customers')
BEGIN
    ALTER TABLE dbo.CustomerPurchases
    ADD CONSTRAINT FK_CustomerPurchases_Customers FOREIGN KEY (CustomerID) REFERENCES dbo.Customers(CustomerID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SupplierTransactions_Suppliers')
BEGIN
    ALTER TABLE dbo.SupplierTransactions
    ADD CONSTRAINT FK_SupplierTransactions_Suppliers FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers(SupplierID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SalaryRecords_Employees')
BEGIN
    ALTER TABLE dbo.SalaryRecords
    ADD CONSTRAINT FK_SalaryRecords_Employees FOREIGN KEY (EmployeeID) REFERENCES dbo.Employees(EmployeeID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Attendance_Employees')
BEGIN
    ALTER TABLE dbo.Attendance
    ADD CONSTRAINT FK_Attendance_Employees FOREIGN KEY (EmployeeID) REFERENCES dbo.Employees(EmployeeID);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
    INSERT INTO dbo.Customers (FullName, Phone, Email, Address, LoyaltyPoints, CreatedDate)
    VALUES ('Nadia Rahman', '01710000001', 'nadia@example.com', 'Dhaka', 120, GETDATE()),
           ('Sabbir Hasan', '01710000002', 'sabbir@example.com', 'Chattogram', 80, GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.CustomerPurchases)
BEGIN
    INSERT INTO dbo.CustomerPurchases (CustomerID, Amount, PurchaseDate, Notes)
    VALUES (1, 2500, GETDATE(), 'Initial purchase'),
           (2, 1800, GETDATE(), 'Regular monthly order');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers)
BEGIN
    INSERT INTO dbo.Suppliers (SupplierName, CompanyName, Phone, Email, Address, ProductType, CreatedDate)
    VALUES ('Rafiq Traders', 'Rafiq Trading Co.', '01810000001', 'rafiq@suppliers.com', 'Dhaka', 'Groceries', GETDATE()),
           ('Beverage Hub', 'Beverage Hub Ltd.', '01810000002', 'contact@beveragehub.com', 'Sylhet', 'Drinks', GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SupplierTransactions)
BEGIN
    INSERT INTO dbo.SupplierTransactions (SupplierID, Amount, TransactionDate, Notes)
    VALUES (1, 15000, GETDATE(), 'Monthly stock bill'),
           (2, 8400, GETDATE(), 'Beverage supply');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Employees)
BEGIN
    INSERT INTO dbo.Employees (FullName, Phone, Email, Address, Role, Salary, Shift, JoinDate, Username, Password)
    VALUES ('Arif Khan', '01910000001', 'arif@supershop.local', 'Dhaka', 'Employee', 22000, 'Morning', GETDATE(), 'arif', 'be76331b95dfc399cd776d2fc68021e0db03cc4f'),
           ('Tania Akter', '01910000002', 'tania@supershop.local', 'Dhaka', 'Admin', 32000, 'Evening', GETDATE(), 'tania', 'be76331b95dfc399cd776d2fc68021e0db03cc4f');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SalaryRecords)
BEGIN
    INSERT INTO dbo.SalaryRecords (EmployeeID, Amount, PaymentDate, Notes)
    VALUES (1, 22000, GETDATE(), 'Current month salary'),
           (2, 32000, GETDATE(), 'Current month salary');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Attendance)
BEGIN
    INSERT INTO dbo.Attendance (EmployeeID, Date, CheckIn, CheckOut, Status)
    VALUES (1, CAST(GETDATE() AS DATE), DATEADD(HOUR, 9, CAST(CAST(GETDATE() AS DATE) AS DATETIME)), DATEADD(HOUR, 18, CAST(CAST(GETDATE() AS DATE) AS DATETIME)), 'Present'),
           (2, CAST(GETDATE() AS DATE), DATEADD(HOUR, 10, CAST(CAST(GETDATE() AS DATE) AS DATETIME)), NULL, 'Present');
END
GO
