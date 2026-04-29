using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class SmokeTestRunner
    {
        private readonly DBHelper _dbHelper = new DBHelper();
        private readonly AuthService _authService = new AuthService();

        public async Task<List<SmokeTestResult>> RunAsync()
        {
            List<SmokeTestResult> results = new List<SmokeTestResult>();
            await TestDbConnection(results);
            await TestRequiredTables(results);
            await TestProductCrud(results);
            await TestCustomerCrud(results);
            await TestEmployeeCrud(results);
            TestLogin(results);
            return results;
        }

        public static bool IsOverallPass(List<SmokeTestResult> results)
        {
            return results.Count > 0 && results.All(x => x.IsPassed);
        }

        private async Task TestDbConnection(List<SmokeTestResult> results)
        {
            try
            {
                object value = await _dbHelper.ExecuteScalarAsync("SELECT 1");
                results.Add(new SmokeTestResult { TestName = "Database Connection", IsPassed = Convert.ToInt32(value) == 1, Details = "Connection test executed." });
            }
            catch (Exception ex)
            {
                results.Add(new SmokeTestResult { TestName = "Database Connection", IsPassed = false, Details = ex.Message });
            }
        }

        private async Task TestRequiredTables(List<SmokeTestResult> results)
        {
            string[] tables = { "Users", "Categories", "Products", "Customers", "Employees", "Attendance", "AuditLogs" };
            List<string> missing = new List<string>();

            foreach (string table in tables)
            {
                object count = await _dbHelper.ExecuteScalarAsync("SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME=@TableName", new SqlParameter("@TableName", table));
                if (Convert.ToInt32(count) == 0)
                {
                    missing.Add(table);
                }
            }

            results.Add(new SmokeTestResult
            {
                TestName = "Required Tables",
                IsPassed = missing.Count == 0,
                Details = missing.Count == 0 ? "All required tables found." : "Missing: " + string.Join(", ", missing)
            });
        }

        private async Task TestProductCrud(List<SmokeTestResult> results)
        {
            int productId = 0;
            try
            {
                object categoryIdObj = await _dbHelper.ExecuteScalarAsync("SELECT TOP 1 CategoryID FROM Categories ORDER BY CategoryID");
                if (categoryIdObj == null || categoryIdObj == DBNull.Value)
                {
                    results.Add(new SmokeTestResult { TestName = "Products CRUD", IsPassed = false, Details = "No category found for product test." });
                    return;
                }

                int categoryId = Convert.ToInt32(categoryIdObj);
                string barcode = "SMK" + DateTime.Now.Ticks;
                await _dbHelper.ExecuteNonQueryAsync(@"INSERT INTO Products (ProductName, Barcode, CategoryID, PurchasePrice, SellingPrice, StockQuantity, ReorderLevel, ExpiryDate, SupplierName, Description, CreatedDate)
                                                      VALUES (@ProductName,@Barcode,@CategoryID,@PurchasePrice,@SellingPrice,@StockQuantity,@ReorderLevel,@ExpiryDate,@SupplierName,@Description,@CreatedDate)",
                    new SqlParameter("@ProductName", "Smoke Product"),
                    new SqlParameter("@Barcode", barcode),
                    new SqlParameter("@CategoryID", categoryId),
                    new SqlParameter("@PurchasePrice", 50m),
                    new SqlParameter("@SellingPrice", 70m),
                    new SqlParameter("@StockQuantity", 10),
                    new SqlParameter("@ReorderLevel", 2),
                    new SqlParameter("@ExpiryDate", DBNull.Value),
                    new SqlParameter("@SupplierName", "Smoke"),
                    new SqlParameter("@Description", "Smoke test"),
                    new SqlParameter("@CreatedDate", DateTime.Now));

                object newIdObj = await _dbHelper.ExecuteScalarAsync("SELECT TOP 1 ProductID FROM Products WHERE Barcode=@Barcode", new SqlParameter("@Barcode", barcode));
                productId = Convert.ToInt32(newIdObj);
                await _dbHelper.ExecuteNonQueryAsync("UPDATE Products SET ProductName=@Name WHERE ProductID=@ProductID", new SqlParameter("@Name", "Smoke Product Updated"), new SqlParameter("@ProductID", productId));
                await _dbHelper.ExecuteNonQueryAsync("DELETE FROM Products WHERE ProductID=@ProductID", new SqlParameter("@ProductID", productId));

                results.Add(new SmokeTestResult { TestName = "Products CRUD", IsPassed = true, Details = "Insert, update, and delete succeeded." });
            }
            catch (Exception ex)
            {
                if (productId > 0)
                {
                    await _dbHelper.ExecuteNonQueryAsync("DELETE FROM Products WHERE ProductID=@ProductID", new SqlParameter("@ProductID", productId));
                }
                results.Add(new SmokeTestResult { TestName = "Products CRUD", IsPassed = false, Details = ex.Message });
            }
        }

        private async Task TestCustomerCrud(List<SmokeTestResult> results)
        {
            int customerId = 0;
            try
            {
                string phone = "SMK" + DateTime.Now.Ticks;
                await _dbHelper.ExecuteNonQueryAsync(@"INSERT INTO Customers (FullName, Phone, Email, Address, LoyaltyPoints, CreatedDate)
                                                      VALUES (@FullName,@Phone,@Email,@Address,@LoyaltyPoints,@CreatedDate)",
                    new SqlParameter("@FullName", "Smoke Customer"),
                    new SqlParameter("@Phone", phone),
                    new SqlParameter("@Email", "smoke@qa.local"),
                    new SqlParameter("@Address", "QA"),
                    new SqlParameter("@LoyaltyPoints", 0),
                    new SqlParameter("@CreatedDate", DateTime.Now));

                object newIdObj = await _dbHelper.ExecuteScalarAsync("SELECT TOP 1 CustomerID FROM Customers WHERE Phone=@Phone", new SqlParameter("@Phone", phone));
                customerId = Convert.ToInt32(newIdObj);
                await _dbHelper.ExecuteNonQueryAsync("UPDATE Customers SET FullName=@Name WHERE CustomerID=@CustomerID", new SqlParameter("@Name", "Smoke Customer Updated"), new SqlParameter("@CustomerID", customerId));
                await _dbHelper.ExecuteNonQueryAsync("DELETE FROM Customers WHERE CustomerID=@CustomerID", new SqlParameter("@CustomerID", customerId));

                results.Add(new SmokeTestResult { TestName = "Customers CRUD", IsPassed = true, Details = "Insert, update, and delete succeeded." });
            }
            catch (Exception ex)
            {
                if (customerId > 0)
                {
                    await _dbHelper.ExecuteNonQueryAsync("DELETE FROM Customers WHERE CustomerID=@CustomerID", new SqlParameter("@CustomerID", customerId));
                }
                results.Add(new SmokeTestResult { TestName = "Customers CRUD", IsPassed = false, Details = ex.Message });
            }
        }

        private async Task TestEmployeeCrud(List<SmokeTestResult> results)
        {
            int employeeId = 0;
            try
            {
                string username = "smk" + DateTime.Now.Ticks;
                await _dbHelper.ExecuteNonQueryAsync(@"INSERT INTO Employees (FullName, Phone, Email, Address, Role, Salary, Shift, JoinDate, Username, Password)
                                                      VALUES (@FullName,@Phone,@Email,@Address,@Role,@Salary,@Shift,@JoinDate,@Username,@Password)",
                    new SqlParameter("@FullName", "Smoke Employee"),
                    new SqlParameter("@Phone", "019" + DateTime.Now.Millisecond.ToString("0000000")),
                    new SqlParameter("@Email", "smoke.employee@qa.local"),
                    new SqlParameter("@Address", "QA"),
                    new SqlParameter("@Role", "Employee"),
                    new SqlParameter("@Salary", 10000m),
                    new SqlParameter("@Shift", "Morning"),
                    new SqlParameter("@JoinDate", DateTime.Now.Date),
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", SecurityHelper.HashValue("Smoke@123")));

                object newIdObj = await _dbHelper.ExecuteScalarAsync("SELECT TOP 1 EmployeeID FROM Employees WHERE Username=@Username", new SqlParameter("@Username", username));
                employeeId = Convert.ToInt32(newIdObj);
                await _dbHelper.ExecuteNonQueryAsync("UPDATE Employees SET FullName=@Name WHERE EmployeeID=@EmployeeID", new SqlParameter("@Name", "Smoke Employee Updated"), new SqlParameter("@EmployeeID", employeeId));
                await _dbHelper.ExecuteNonQueryAsync("DELETE FROM Employees WHERE EmployeeID=@EmployeeID", new SqlParameter("@EmployeeID", employeeId));

                results.Add(new SmokeTestResult { TestName = "Employees CRUD", IsPassed = true, Details = "Insert, update, and delete succeeded." });
            }
            catch (Exception ex)
            {
                if (employeeId > 0)
                {
                    await _dbHelper.ExecuteNonQueryAsync("DELETE FROM Employees WHERE EmployeeID=@EmployeeID", new SqlParameter("@EmployeeID", employeeId));
                }
                results.Add(new SmokeTestResult { TestName = "Employees CRUD", IsPassed = false, Details = ex.Message });
            }
        }

        private void TestLogin(List<SmokeTestResult> results)
        {
            try
            {
                _authService.Login("admin", "Admin@123");
                _authService.Logout();
                results.Add(new SmokeTestResult { TestName = "Authentication", IsPassed = true, Details = "Login test with admin succeeded." });
            }
            catch (Exception ex)
            {
                results.Add(new SmokeTestResult { TestName = "Authentication", IsPassed = false, Details = ex.Message });
            }
        }
    }
}
