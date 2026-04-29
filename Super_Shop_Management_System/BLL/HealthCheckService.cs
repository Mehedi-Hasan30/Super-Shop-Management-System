using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class HealthCheckService
    {
        public static async Task<HealthCheckResult> ValidateAsync()
        {
            string connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["SuperShopConnection"].ConnectionString;

            return await ValidateInternalAsync(connectionString);
        }

        private static async Task<HealthCheckResult> ValidateInternalAsync(string connectionString)
        {
            HealthCheckResult result = new HealthCheckResult();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                result.IsHealthy = false;
                result.Errors.Add("Database connection failed: " + ex.Message);
                ErrorLogger.Log("HealthCheckService.ValidateInternalAsync", ex);
                return result;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommandValidator validator = new SqlCommandValidator(connection))
                {
                    await connection.OpenAsync(); // ✅ এই line add করা হয়েছে
                    await validator.CheckTableAndColumnsAsync(result);
                }
            }
            catch (Exception ex)
            {
                result.IsHealthy = false;
                result.Errors.Add("Startup validation failed: " + ex.Message);
                ErrorLogger.Log("HealthCheckService.ValidateInternalAsync (schema)", ex);
                return result;
            }

            result.IsHealthy = result.Errors.Count == 0;
            return result;
        }

        private class SqlCommandValidator : IDisposable
        {
            private readonly SqlConnection _connection;

            public SqlCommandValidator(SqlConnection connection)
            {
                _connection = connection;
            }

            public void Dispose()
            {
            }

            public async Task CheckTableAndColumnsAsync(HealthCheckResult result)
            {
                await CheckTableAsync(result, "Products", true);
                await CheckTableAsync(result, "Sales", true);
                await CheckTableAsync(result, "SalesDetails", true);
                await CheckTableAsync(result, "Customers", false);
                await CheckTableAsync(result, "Employees", false);
                await CheckTableAsync(result, "Suppliers", false);
                await CheckTableAsync(result, "SupplierTransactions", false);
                await CheckTableAsync(result, "Attendance", false);
                await CheckTableAsync(result, "AuditLogs", false);

                await CheckColumnAsync(result, "Products", "StockQuantity", true);
                await CheckColumnAsync(result, "Sales", "EmployeeID", true);
                await CheckColumnAsync(result, "Sales", "SaleDate", true);
                await CheckColumnAsync(result, "Sales", "GrandTotal", true);
                await CheckColumnAsync(result, "Sales", "PaymentMethod", true);
                await CheckColumnAsync(result, "Sales", "PaymentStatus", true);
                await CheckColumnAsync(result, "SalesDetails", "SaleID", true);
                await CheckColumnAsync(result, "SalesDetails", "ProductID", true);
                await CheckColumnAsync(result, "SalesDetails", "Quantity", true);
                await CheckColumnAsync(result, "SalesDetails", "UnitPrice", true);
                await CheckColumnAsync(result, "SalesDetails", "SubTotal", true);
            }

            private async Task CheckTableAsync(HealthCheckResult result, string tableName, bool isRequired)
            {
                string sql = "SELECT CASE WHEN OBJECT_ID('dbo." + tableName + "', 'U') IS NULL THEN 0 ELSE 1 END";
                using (SqlCommand cmd = new SqlCommand(sql, _connection))
                {
                    object obj = await cmd.ExecuteScalarAsync();
                    int exists = Convert.ToInt32(obj);
                    if (exists == 0)
                    {
                        if (isRequired)
                            result.Errors.Add("Missing table: dbo." + tableName);
                        else
                            result.Warnings.Add("Missing table: dbo." + tableName + " (some reports/notifications may be unavailable)");
                    }
                }
            }

            private async Task CheckColumnAsync(HealthCheckResult result, string tableName, string columnName, bool isRequired)
            {
                string sql = "SELECT CASE WHEN COL_LENGTH('dbo." + tableName + "', '" + columnName + "') IS NULL THEN 0 ELSE 1 END";
                using (SqlCommand cmd = new SqlCommand(sql, _connection))
                {
                    object obj = await cmd.ExecuteScalarAsync();
                    int exists = Convert.ToInt32(obj);
                    if (exists == 0)
                    {
                        if (isRequired)
                            result.Errors.Add("Missing column: dbo." + tableName + "." + columnName);
                        else
                            result.Warnings.Add("Missing column: dbo." + tableName + "." + columnName + " (some features may be unavailable)");
                    }
                }
            }
        }
    }
}