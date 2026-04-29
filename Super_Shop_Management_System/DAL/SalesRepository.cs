using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class SalesRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();
        private readonly SalesDetailRepository _salesDetailRepository = new SalesDetailRepository();
        private readonly StockRepository _stockRepository = new StockRepository();

        public async Task<int> SaveSaleAsync(Sale sale, List<SalesDetail> details)
        {
            if (sale == null)
            {
                throw new ApplicationException("Sale data is required.");
            }

            if (details == null || details.Count == 0)
            {
                throw new ApplicationException("Sale details are required.");
            }

            int saleId = 0;

            await _dbHelper.ExecuteTransactionAsync(async (connection, transaction) =>
            {
                const string insertSaleSql = @"
INSERT INTO Sales
    (CustomerID, EmployeeID, SaleDate, TotalAmount, Discount, VAT, GrandTotal, PaymentMethod, PaymentStatus)
VALUES
    (@CustomerID, @EmployeeID, @SaleDate, @TotalAmount, @Discount, @VAT, @GrandTotal, @PaymentMethod, @PaymentStatus);
SELECT CAST(SCOPE_IDENTITY() AS int);";

                using (SqlCommand insertSaleCommand = new SqlCommand(insertSaleSql, connection, transaction))
                {
                    insertSaleCommand.Parameters.Add(new SqlParameter("@CustomerID", (object)sale.CustomerID ?? DBNull.Value));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@EmployeeID", sale.EmployeeID));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@SaleDate", sale.SaleDate));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@TotalAmount", sale.TotalAmount));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@Discount", sale.Discount));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@VAT", sale.VAT));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@GrandTotal", sale.GrandTotal));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@PaymentMethod", sale.PaymentMethod ?? string.Empty));
                    insertSaleCommand.Parameters.Add(new SqlParameter("@PaymentStatus", sale.PaymentStatus ?? string.Empty));

                    object idObj = await insertSaleCommand.ExecuteScalarAsync();
                    saleId = Convert.ToInt32(idObj);
                }

                await _salesDetailRepository.InsertDetailsAsync(connection, transaction, saleId, details);

                if (string.Equals(sale.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (SalesDetail detail in details)
                    {
                        await _stockRepository.DeductStockAsync(connection, transaction, detail.ProductID, detail.Quantity);
                    }
                }
            });

            return saleId;
        }

        public async Task<List<Sale>> GetSalesByPaymentStatusAsync(string paymentStatus)
        {
            if (string.IsNullOrWhiteSpace(paymentStatus))
            {
                throw new ApplicationException("Payment status is required.");
            }

            const string query = @"
SELECT SaleID, CustomerID, EmployeeID, SaleDate, TotalAmount, Discount, VAT, GrandTotal, PaymentMethod, PaymentStatus
FROM Sales
WHERE PaymentStatus = @PaymentStatus
ORDER BY SaleDate DESC";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query, new SqlParameter("@PaymentStatus", paymentStatus));
            List<Sale> sales = new List<Sale>();

            foreach (DataRow row in table.Rows)
            {
                sales.Add(new Sale
                {
                    SaleID = Convert.ToInt32(row["SaleID"]),
                    CustomerID = row["CustomerID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CustomerID"]),
                    EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                    SaleDate = Convert.ToDateTime(row["SaleDate"]),
                    TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    Discount = Convert.ToDecimal(row["Discount"]),
                    VAT = Convert.ToDecimal(row["VAT"]),
                    GrandTotal = Convert.ToDecimal(row["GrandTotal"]),
                    PaymentMethod = row["PaymentMethod"].ToString(),
                    PaymentStatus = row["PaymentStatus"].ToString()
                });
            }

            return sales;
        }

        public async Task<bool> MarkSaleAsPaidAndDeductStockAsync(int saleId)
        {
            if (saleId <= 0)
            {
                throw new ApplicationException("Invalid sale id.");
            }

            bool result = false;

            await _dbHelper.ExecuteTransactionAsync(async (connection, transaction) =>
            {
                const string updateSaleSql = @"
UPDATE Sales
SET PaymentStatus = @PaidStatus
WHERE SaleID = @SaleID AND PaymentStatus = @PendingStatus;";

                using (SqlCommand updateCmd = new SqlCommand(updateSaleSql, connection, transaction))
                {
                    updateCmd.Parameters.AddWithValue("@PaidStatus", "Paid");
                    updateCmd.Parameters.AddWithValue("@PendingStatus", "Pending");
                    updateCmd.Parameters.AddWithValue("@SaleID", saleId);

                    int rows = await updateCmd.ExecuteNonQueryAsync();
                    if (rows <= 0)
                    {
                        throw new ApplicationException("Only pending sales can be marked as paid.");
                    }
                }

                const string detailsSql = @"
SELECT ProductID, Quantity
FROM SalesDetails
WHERE SaleID = @SaleID;";

                using (SqlCommand detailsCmd = new SqlCommand(detailsSql, connection, transaction))
                {
                    detailsCmd.Parameters.AddWithValue("@SaleID", saleId);

                    using (SqlDataReader reader = await detailsCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int productId = Convert.ToInt32(reader["ProductID"]);
                            int quantity = Convert.ToInt32(reader["Quantity"]);
                            await _stockRepository.DeductStockAsync(connection, transaction, productId, quantity);
                        }
                    }
                }

                result = true;
            });

            return result;
        }

        public async Task<InvoiceData> GetInvoiceDataAsync(int saleId)
        {
            if (saleId <= 0)
            {
                throw new ApplicationException("Invalid sale id.");
            }

            const string headerSql = @"
SELECT s.SaleID, s.CustomerID, s.EmployeeID, s.SaleDate,
       s.TotalAmount, s.Discount, s.VAT, s.GrandTotal,
       s.PaymentMethod, s.PaymentStatus,
       ISNULL(c.FullName, 'Walk-in Customer') AS CustomerName,
       e.FullName AS EmployeeName
FROM Sales s
LEFT JOIN Customers c ON c.CustomerID = s.CustomerID
INNER JOIN Employees e ON e.EmployeeID = s.EmployeeID
WHERE s.SaleID = @SaleID;";

            DataTable headerTable = await _dbHelper.ExecuteDataTableAsync(headerSql, new SqlParameter("@SaleID", saleId));
            if (headerTable.Rows.Count == 0)
            {
                throw new ApplicationException($"Sale ID {saleId} not found.");
            }

            DataRow headerRow = headerTable.Rows[0];
            InvoiceData invoice = new InvoiceData
            {
                SaleID = Convert.ToInt32(headerRow["SaleID"]),
                CustomerName = headerRow["CustomerName"].ToString(),
                EmployeeName = headerRow["EmployeeName"].ToString(),
                SaleDate = Convert.ToDateTime(headerRow["SaleDate"]),
                TotalAmount = Convert.ToDecimal(headerRow["TotalAmount"]),
                Discount = Convert.ToDecimal(headerRow["Discount"]),
                VAT = Convert.ToDecimal(headerRow["VAT"]),
                GrandTotal = Convert.ToDecimal(headerRow["GrandTotal"]),
                PaymentMethod = headerRow["PaymentMethod"].ToString(),
                PaymentStatus = headerRow["PaymentStatus"].ToString()
            };

            const string detailsSql = @"
SELECT sd.ProductID, p.ProductName, p.Barcode,
       sd.Quantity, sd.UnitPrice, sd.SubTotal
FROM SalesDetails sd
INNER JOIN Products p ON p.ProductID = sd.ProductID
WHERE sd.SaleID = @SaleID
ORDER BY sd.SalesDetailID;";

            DataTable detailsTable = await _dbHelper.ExecuteDataTableAsync(detailsSql, new SqlParameter("@SaleID", saleId));
            foreach (DataRow row in detailsTable.Rows)
            {
                invoice.Lines.Add(new InvoiceLine
                {
                    ProductID = Convert.ToInt32(row["ProductID"]),
                    ProductName = row["ProductName"].ToString(),
                    Barcode = row["Barcode"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    SubTotal = Convert.ToDecimal(row["SubTotal"])
                });
            }

            return invoice;
        }

        public async Task<List<SalesHistoryItem>> GetCustomerSalesHistoryAsync(
            int? customerId,
            DateTime? fromDate,
            DateTime? toDate,
            string paymentStatus)
        {
            if (string.IsNullOrWhiteSpace(paymentStatus))
            {
                throw new ApplicationException("Payment status filter is required.");
            }

            const string query = @"
SELECT s.SaleID,
       ISNULL(c.FullName, 'Walk-in Customer') AS CustomerName,
       s.SaleDate,
       s.GrandTotal,
       s.PaymentMethod,
       s.PaymentStatus,
       e.FullName AS EmployeeName
FROM Sales s
LEFT JOIN Customers c ON c.CustomerID = s.CustomerID
INNER JOIN Employees e ON e.EmployeeID = s.EmployeeID
WHERE (@CustomerID IS NULL OR s.CustomerID = @CustomerID)
  AND (@FromDate IS NULL OR s.SaleDate >= @FromDate)
  AND (@ToDate IS NULL OR s.SaleDate < DATEADD(DAY, 1, @ToDate))
  AND (@PaymentStatus = 'All' OR s.PaymentStatus = @PaymentStatus)
ORDER BY s.SaleDate DESC;";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(
                query,
                new SqlParameter("@CustomerID", customerId.HasValue && customerId.Value > 0 ? (object)customerId.Value : DBNull.Value),
                new SqlParameter("@FromDate", fromDate.HasValue ? (object)fromDate.Value : DBNull.Value),
                new SqlParameter("@ToDate", toDate.HasValue ? (object)toDate.Value : DBNull.Value),
                new SqlParameter("@PaymentStatus", paymentStatus));

            List<SalesHistoryItem> results = new List<SalesHistoryItem>();
            foreach (DataRow row in table.Rows)
            {
                results.Add(new SalesHistoryItem
                {
                    SaleID = Convert.ToInt32(row["SaleID"]),
                    CustomerName = row["CustomerName"].ToString(),
                    SaleDate = Convert.ToDateTime(row["SaleDate"]),
                    GrandTotal = Convert.ToDecimal(row["GrandTotal"]),
                    PaymentMethod = row["PaymentMethod"].ToString(),
                    PaymentStatus = row["PaymentStatus"].ToString(),
                    EmployeeName = row["EmployeeName"].ToString()
                });
            }

            return results;
        }
    }
}

