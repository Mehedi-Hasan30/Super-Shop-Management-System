using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class ReportsRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        private static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        private static string PaymentFilterClause(string salesAlias, string paymentStatusParamName)
        {
            // salesAlias.PaymentStatus and @PaymentStatus in ('All','Paid','Pending')
            // caller handles 'All'.
            return $"(@{paymentStatusParamName} = 'All' OR {salesAlias}.PaymentStatus = @{paymentStatusParamName})";
        }

        public async Task<DataTable> GetDailySalesReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT CAST(s.SaleDate AS DATE) AS SaleDate,
       SUM(s.GrandTotal) AS TotalRevenue,
       SUM(s.VAT) AS VATCollected
FROM Sales s
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
  AND (@ProductID IS NULL OR EXISTS (SELECT 1 FROM SalesDetails sd WHERE sd.SaleID = s.SaleID AND sd.ProductID = @ProductID))
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY CAST(s.SaleDate AS DATE)
ORDER BY SaleDate;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@ProductID", filters.ProductId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }

        public async Task<DataTable> GetMonthlySalesReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT DATENAME(MONTH, s.SaleDate) + ' ' + CAST(YEAR(s.SaleDate) AS NVARCHAR(4)) AS MonthLabel,
       SUM(s.GrandTotal) AS TotalRevenue,
       SUM(s.VAT) AS VATCollected,
       SUM(s.TotalAmount) AS TotalAmount
FROM Sales s
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
  AND (@ProductID IS NULL OR EXISTS (SELECT 1 FROM SalesDetails sd WHERE sd.SaleID = s.SaleID AND sd.ProductID = @ProductID))
GROUP BY YEAR(s.SaleDate), MONTH(s.SaleDate), DATENAME(MONTH, s.SaleDate)
ORDER BY YEAR(s.SaleDate), MONTH(s.SaleDate);";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@ProductID", filters.ProductId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }

        public async Task<DataTable> GetProductSalesReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT p.ProductID,
       p.ProductName,
       p.Barcode,
       SUM(sd.Quantity) AS QuantitySold,
       SUM(sd.SubTotal) AS RevenueSubTotal,
       SUM(sd.Quantity * p.PurchasePrice) AS PurchaseCost
FROM Sales s
INNER JOIN SalesDetails sd ON sd.SaleID = s.SaleID
INNER JOIN Products p ON p.ProductID = sd.ProductID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
  AND (@SupplierID IS NULL OR p.SupplierName = (SELECT SupplierName FROM Suppliers WHERE SupplierID=@SupplierID))
  AND (@ProductID IS NULL OR p.ProductID = @ProductID)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY p.ProductID, p.ProductName, p.Barcode
ORDER BY RevenueSubTotal DESC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@SupplierID", filters.SupplierId),
                P("@ProductID", filters.ProductId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }

        public async Task<DataTable> GetCustomerPurchaseReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT ISNULL(c.FullName, 'Walk-in Customer') AS CustomerName,
       s.CustomerID,
       COUNT(DISTINCT s.SaleID) AS InvoiceCount,
       SUM(s.GrandTotal) AS GrandTotalRevenue,
       SUM(s.TotalAmount) AS TotalAmount
FROM Sales s
LEFT JOIN Customers c ON c.CustomerID = s.CustomerID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
  AND (@ProductID IS NULL OR EXISTS (SELECT 1 FROM SalesDetails sd WHERE sd.SaleID = s.SaleID AND sd.ProductID = @ProductID))
  AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY s.CustomerID, c.FullName
ORDER BY GrandTotalRevenue DESC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@EmployeeID", filters.EmployeeId),
                P("@ProductID", filters.ProductId),
                P("@CustomerID", filters.CustomerId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }

        public async Task<DataTable> GetSupplierPurchaseReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT sp.SupplierID,
       sp.SupplierName,
       COUNT(st.SupplierTransactionID) AS TransactionCount,
       SUM(st.Amount) AS TotalPurchasedAmount
FROM SupplierTransactions st
INNER JOIN Suppliers sp ON sp.SupplierID = st.SupplierID
WHERE st.TransactionDate >= @FromDate
  AND st.TransactionDate < DATEADD(DAY, 1, @ToDate)
  AND (@SupplierID IS NULL OR st.SupplierID = @SupplierID)
GROUP BY sp.SupplierID, sp.SupplierName
ORDER BY TotalPurchasedAmount DESC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@SupplierID", filters.SupplierId));
        }

        public async Task<DataTable> GetEmployeeSalesReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT e.EmployeeID,
       e.FullName AS EmployeeName,
       COUNT(DISTINCT s.SaleID) AS InvoiceCount,
       SUM(s.GrandTotal) AS Revenue
FROM Sales s
INNER JOIN Employees e ON e.EmployeeID = s.EmployeeID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
  AND (@ProductID IS NULL OR EXISTS (SELECT 1 FROM SalesDetails sd WHERE sd.SaleID = s.SaleID AND sd.ProductID = @ProductID))
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY e.EmployeeID, e.FullName
ORDER BY Revenue DESC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@ProductID", filters.ProductId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }

        public async Task<DataTable> GetPendingSalesAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT s.SaleID,
       s.SaleDate,
       ISNULL(c.FullName, 'Walk-in Customer') AS CustomerName,
       e.FullName AS EmployeeName,
       s.TotalAmount,
       s.Discount,
       s.VAT,
       s.GrandTotal,
       s.PaymentMethod,
       s.PaymentStatus
FROM Sales s
LEFT JOIN Customers c ON c.CustomerID = s.CustomerID
INNER JOIN Employees e ON e.EmployeeID = s.EmployeeID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND s.PaymentStatus = 'Pending'
  AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus);";

            // Note: last clause keeps compatibility with PaymentStatus filter "All/Paid/Pending".
            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }

        public async Task<DataTable> GetPaidSalesAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT s.SaleID,
       s.SaleDate,
       ISNULL(c.FullName, 'Walk-in Customer') AS CustomerName,
       e.FullName AS EmployeeName,
       s.TotalAmount,
       s.Discount,
       s.VAT,
       s.GrandTotal,
       s.PaymentMethod,
       s.PaymentStatus
FROM Sales s
LEFT JOIN Customers c ON c.CustomerID = s.CustomerID
INNER JOIN Employees e ON e.EmployeeID = s.EmployeeID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND s.PaymentStatus = 'Paid'
  AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus);";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }

        public async Task<DataTable> GetInventoryStockReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT ProductID,
       ProductName,
       Barcode,
       CategoryID,
       PurchasePrice,
       SellingPrice,
       StockQuantity,
       ReorderLevel,
       ExpiryDate,
       SupplierName
FROM Products
WHERE (@ProductID IS NULL OR ProductID = @ProductID)
  AND (@SupplierID IS NULL OR SupplierName = (SELECT SupplierName FROM Suppliers WHERE SupplierID=@SupplierID))
ORDER BY ProductName;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@ProductID", filters.ProductId),
                P("@SupplierID", filters.SupplierId));
        }

        public async Task<DataTable> GetLowStockReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT ProductID,
       ProductName,
       Barcode,
       CategoryID,
       StockQuantity,
       ReorderLevel,
       ExpiryDate,
       SupplierName
FROM Products
WHERE StockQuantity <= ReorderLevel
  AND (@ProductID IS NULL OR ProductID = @ProductID)
  AND (@SupplierID IS NULL OR SupplierName = (SELECT SupplierName FROM Suppliers WHERE SupplierID=@SupplierID))
ORDER BY StockQuantity ASC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@ProductID", filters.ProductId),
                P("@SupplierID", filters.SupplierId));
        }

        public async Task<DataTable> GetExpiryProductsReportAsync(ReportFilters filters)
        {
            const string sql = @"
SELECT ProductID,
       ProductName,
       Barcode,
       CategoryID,
       StockQuantity,
       ReorderLevel,
       ExpiryDate,
       SupplierName
FROM Products
WHERE ExpiryDate IS NOT NULL
  AND ExpiryDate >= @FromDate
  AND ExpiryDate <= @ToDate
  AND (@ProductID IS NULL OR ProductID = @ProductID)
  AND (@SupplierID IS NULL OR SupplierName = (SELECT SupplierName FROM Suppliers WHERE SupplierID=@SupplierID))
ORDER BY ExpiryDate ASC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate.Date),
                P("@ToDate", filters.ToDate.Date),
                P("@ProductID", filters.ProductId),
                P("@SupplierID", filters.SupplierId));
        }

        public async Task<DataTable> GetAttendanceReportAsync(DateTime fromDate, DateTime toDate, int? employeeId)
        {
            const string sql = @"
SELECT a.AttendanceID,
       e.FullName AS EmployeeName,
       a.Date,
       a.CheckIn,
       a.CheckOut,
       a.Status
FROM Attendance a
INNER JOIN Employees e ON e.EmployeeID = a.EmployeeID
WHERE a.Date >= @FromDate
  AND a.Date <= @ToDate
  AND (@EmployeeID IS NULL OR a.EmployeeID = @EmployeeID)
ORDER BY a.Date DESC, e.FullName;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate.Date),
                P("@ToDate", toDate.Date),
                P("@EmployeeID", employeeId));
        }

        // Analytics queries
        public async Task<DataTable> GetSalesTrendAsync(DateTime fromDate, DateTime toDate, TimeAggregationType aggregation, string paymentStatus)
        {
            string groupSql;
            if (aggregation == TimeAggregationType.Monthly)
            {
                groupSql = @"DATENAME(MONTH, s.SaleDate) + ' ' + CAST(YEAR(s.SaleDate) AS NVARCHAR(4))";
            }
            else if (aggregation == TimeAggregationType.Weekly)
            {
                // Week starts on Monday
                groupSql = @"CONVERT(date, DATEADD(day, -(DATEPART(weekday, s.SaleDate) + 5) % 7, CAST(s.SaleDate AS date)))";
            }
            else
            {
                groupSql = @"CAST(s.SaleDate AS date)";
            }

            const string template = @"
SELECT {0} AS PeriodLabel,
       SUM(s.GrandTotal) AS TotalRevenue
FROM Sales s
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY {0}
ORDER BY {0};";

            string sql = string.Format(template, groupSql);

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate),
                P("@ToDate", toDate),
                P("@PaymentStatus", paymentStatus ?? "All"));
        }

        public async Task<DataTable> GetRevenueTrendAsync(DateTime fromDate, DateTime toDate, TimeAggregationType aggregation, string paymentStatus)
        {
            string groupSql;
            if (aggregation == TimeAggregationType.Monthly)
            {
                groupSql = @"DATENAME(MONTH, s.SaleDate) + ' ' + CAST(YEAR(s.SaleDate) AS NVARCHAR(4))";
            }
            else if (aggregation == TimeAggregationType.Weekly)
            {
                groupSql = @"CONVERT(date, DATEADD(day, -(DATEPART(weekday, s.SaleDate) + 5) % 7, CAST(s.SaleDate AS date)))";
            }
            else
            {
                groupSql = @"CAST(s.SaleDate AS date)";
            }

            const string template = @"
SELECT {0} AS PeriodLabel,
       SUM(s.TotalAmount) AS TotalRevenue
FROM Sales s
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY {0}
ORDER BY {0};";

            string sql = string.Format(template, groupSql);
            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate),
                P("@ToDate", toDate),
                P("@PaymentStatus", paymentStatus ?? "All"));
        }

        public async Task<DataTable> GetTopSellingProductsAsync(DateTime fromDate, DateTime toDate, int top, string paymentStatus)
        {
            const string sql = @"
SELECT TOP (@TopN)
       p.ProductName,
       p.Barcode,
       SUM(sd.Quantity) AS QuantitySold
FROM Sales s
INNER JOIN SalesDetails sd ON sd.SaleID = s.SaleID
INNER JOIN Products p ON p.ProductID = sd.ProductID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY p.ProductName, p.Barcode
ORDER BY QuantitySold DESC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate),
                P("@ToDate", toDate),
                P("@TopN", top),
                P("@PaymentStatus", paymentStatus ?? "All"));
        }

        public async Task<DataTable> GetBestCustomersAsync(DateTime fromDate, DateTime toDate, int top, string paymentStatus)
        {
            const string sql = @"
SELECT TOP (@TopN)
       ISNULL(c.FullName, 'Walk-in Customer') AS CustomerName,
       s.CustomerID,
       SUM(s.GrandTotal) AS GrandTotalRevenue
FROM Sales s
LEFT JOIN Customers c ON c.CustomerID = s.CustomerID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY s.CustomerID, c.FullName
ORDER BY GrandTotalRevenue DESC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate),
                P("@ToDate", toDate),
                P("@TopN", top),
                P("@PaymentStatus", paymentStatus ?? "All"));
        }

        public async Task<DataTable> GetInventoryMovementTrendAsync(DateTime fromDate, DateTime toDate, TimeAggregationType aggregation, string paymentStatus)
        {
            string groupSql;
            if (aggregation == TimeAggregationType.Monthly)
            {
                groupSql = @"DATENAME(MONTH, s.SaleDate) + ' ' + CAST(YEAR(s.SaleDate) AS NVARCHAR(4))";
            }
            else if (aggregation == TimeAggregationType.Weekly)
            {
                groupSql = @"CONVERT(date, DATEADD(day, -(DATEPART(weekday, s.SaleDate) + 5) % 7, CAST(s.SaleDate AS date)))";
            }
            else
            {
                groupSql = @"CAST(s.SaleDate AS date)";
            }

            const string template = @"
SELECT {0} AS PeriodLabel,
       SUM(sd.Quantity) AS UnitsSold
FROM Sales s
INNER JOIN SalesDetails sd ON sd.SaleID = s.SaleID
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY {0}
ORDER BY {0};";

            string sql = string.Format(template, groupSql);

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate),
                P("@ToDate", toDate),
                P("@PaymentStatus", paymentStatus ?? "All"));
        }

        public async Task<DataTable> GetPaymentMethodBreakdownAsync(DateTime fromDate, DateTime toDate, string paymentStatus)
        {
            const string sql = @"
SELECT ISNULL(s.PaymentMethod, 'Unknown') AS PaymentMethod,
       SUM(s.GrandTotal) AS GrandTotalRevenue
FROM Sales s
WHERE s.SaleDate >= @FromDate
  AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
GROUP BY s.PaymentMethod
ORDER BY GrandTotalRevenue DESC;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate),
                P("@ToDate", toDate),
                P("@PaymentStatus", paymentStatus ?? "All"));
        }

        public async Task<DataTable> GetPendingVsPaidComparisonAsync(DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
SELECT PaymentStatus,
       COUNT(*) AS InvoiceCount,
       SUM(GrandTotal) AS GrandTotalRevenue
FROM Sales
WHERE SaleDate >= @FromDate
  AND SaleDate < DATEADD(DAY, 1, @ToDate)
  AND PaymentStatus IN ('Pending','Paid')
GROUP BY PaymentStatus;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", fromDate),
                P("@ToDate", toDate.Date));
        }

        public async Task<ProfitLossReport> GetProfitLossSummaryAsync(ReportFilters filters)
        {
            const string sql = @"
WITH FilteredSales AS (
    SELECT s.SaleID, s.GrandTotal, s.VAT, s.PaymentStatus
    FROM Sales s
    WHERE s.SaleDate >= @FromDate
      AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
      AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
      AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
      AND (@ProductID IS NULL OR EXISTS (SELECT 1 FROM SalesDetails sd WHERE sd.SaleID = s.SaleID AND sd.ProductID = @ProductID))
      AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
)
SELECT
    ISNULL(SUM(fs.GrandTotal), 0) AS TotalRevenue,
    ISNULL(SUM(CASE WHEN fs.PaymentStatus = 'Pending' THEN fs.GrandTotal ELSE 0 END), 0) AS PendingRevenue,
    ISNULL(SUM(fs.VAT), 0) AS VATCollected,
    ISNULL(SUM(sd.Quantity * p.PurchasePrice), 0) AS TotalPurchaseCost
FROM FilteredSales fs
INNER JOIN SalesDetails sd ON sd.SaleID = fs.SaleID
INNER JOIN Products p ON p.ProductID = sd.ProductID;";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@ProductID", filters.ProductId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));

            if (table.Rows.Count == 0)
            {
                return new ProfitLossReport
                {
                    FromDate = filters.FromDate,
                    ToDate = filters.ToDate
                };
            }

            DataRow row = table.Rows[0];
            decimal totalRevenue = Convert.ToDecimal(row["TotalRevenue"]);
            decimal pendingRevenue = Convert.ToDecimal(row["PendingRevenue"]);
            decimal vatCollected = Convert.ToDecimal(row["VATCollected"]);
            decimal purchaseCost = Convert.ToDecimal(row["TotalPurchaseCost"]);

            ProfitLossReport report = new ProfitLossReport
            {
                FromDate = filters.FromDate,
                ToDate = filters.ToDate,
                TotalRevenue = totalRevenue,
                PendingRevenue = pendingRevenue,
                VATCollected = vatCollected,
                TotalPurchaseCost = purchaseCost
            };

            report.GrossProfit = report.TotalRevenue - report.TotalPurchaseCost;
            report.NetProfit = report.GrossProfit;
            return report;
        }

        public async Task<DataTable> GetMonthlyGrossProfitAsync(ReportFilters filters)
        {
            const string sql = @"
WITH RevenueByMonth AS (
    SELECT YEAR(s.SaleDate) AS Y,
           MONTH(s.SaleDate) AS M,
           SUM(s.GrandTotal) AS Revenue
    FROM Sales s
    WHERE s.SaleDate >= @FromDate
      AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
      AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
      AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
      AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
      AND (@ProductID IS NULL OR EXISTS (SELECT 1 FROM SalesDetails sd WHERE sd.SaleID = s.SaleID AND sd.ProductID = @ProductID))
    GROUP BY YEAR(s.SaleDate), MONTH(s.SaleDate)
),
CostByMonth AS (
    SELECT YEAR(s.SaleDate) AS Y,
           MONTH(s.SaleDate) AS M,
           SUM(sd.Quantity * p.PurchasePrice) AS Cost
    FROM Sales s
    INNER JOIN SalesDetails sd ON sd.SaleID = s.SaleID
    INNER JOIN Products p ON p.ProductID = sd.ProductID
    WHERE s.SaleDate >= @FromDate
      AND s.SaleDate < DATEADD(DAY, 1, @ToDate)
      AND (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
      AND (@EmployeeID IS NULL OR s.EmployeeID = @EmployeeID)
      AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
      AND (@ProductID IS NULL OR EXISTS (SELECT 1 FROM SalesDetails sd2 WHERE sd2.SaleID = s.SaleID AND sd2.ProductID = @ProductID))
    GROUP BY YEAR(s.SaleDate), MONTH(s.SaleDate)
)
SELECT 
    DATENAME(MONTH, DATEFROMPARTS(r.Y, r.M, 1)) + ' ' + CAST(r.Y AS NVARCHAR(4)) AS MonthLabel,
    r.Revenue AS Revenue,
    ISNULL(c.Cost, 0) AS PurchaseCost,
    (r.Revenue - ISNULL(c.Cost, 0)) AS GrossProfit
FROM RevenueByMonth r
LEFT JOIN CostByMonth c ON c.Y = r.Y AND c.M = r.M
ORDER BY r.Y, r.M;";

            return await _dbHelper.ExecuteDataTableAsync(
                sql,
                P("@FromDate", filters.FromDate),
                P("@ToDate", filters.ToDate),
                P("@CustomerID", filters.CustomerId),
                P("@EmployeeID", filters.EmployeeId),
                P("@ProductID", filters.ProductId),
                P("@PaymentStatus", filters.PaymentStatus ?? "All"));
        }
    }
}

