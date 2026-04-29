using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL.Interfaces;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public async Task<List<Employee>> GetAllAsync(string searchKeyword = "")
        {
            const string query = @"SELECT e.EmployeeID, e.FullName, e.Phone, e.Email, e.Address, e.Role, e.Salary, e.Shift, e.JoinDate, e.Username,
                                          ISNULL((SELECT TOP 1 Amount FROM SalaryRecords sr WHERE sr.EmployeeID = e.EmployeeID ORDER BY sr.PaymentDate DESC), 0) AS LatestSalaryPaid
                                   FROM Employees e
                                   WHERE (@Keyword = '' OR e.FullName LIKE '%' + @Keyword + '%' OR e.Phone LIKE '%' + @Keyword + '%' OR e.Username LIKE '%' + @Keyword + '%')
                                   ORDER BY e.FullName";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query, new SqlParameter("@Keyword", searchKeyword ?? string.Empty));
            List<Employee> employees = new List<Employee>();
            foreach (DataRow row in table.Rows)
            {
                employees.Add(new Employee
                {
                    EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                    FullName = row["FullName"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString(),
                    Address = row["Address"].ToString(),
                    Role = row["Role"].ToString(),
                    Salary = Convert.ToDecimal(row["Salary"]),
                    Shift = row["Shift"].ToString(),
                    JoinDate = Convert.ToDateTime(row["JoinDate"]),
                    Username = row["Username"].ToString(),
                    LatestSalaryPaid = Convert.ToDecimal(row["LatestSalaryPaid"])
                });
            }

            return employees;
        }

        public async Task<string> GetPasswordHashByIdAsync(int employeeId)
        {
            const string query = "SELECT Password FROM Employees WHERE EmployeeID=@EmployeeID";
            object result = await _dbHelper.ExecuteScalarAsync(query, new SqlParameter("@EmployeeID", employeeId));
            return result?.ToString();
        }

        public async Task<bool> AddAsync(Employee employee)
        {
            const string query = @"INSERT INTO Employees (FullName, Phone, Email, Address, Role, Salary, Shift, JoinDate, Username, Password)
                                   VALUES (@FullName, @Phone, @Email, @Address, @Role, @Salary, @Shift, @JoinDate, @Username, @Password)";

            int rows = await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@FullName", employee.FullName),
                new SqlParameter("@Phone", employee.Phone),
                new SqlParameter("@Email", employee.Email),
                new SqlParameter("@Address", employee.Address),
                new SqlParameter("@Role", employee.Role),
                new SqlParameter("@Salary", employee.Salary),
                new SqlParameter("@Shift", employee.Shift),
                new SqlParameter("@JoinDate", employee.JoinDate),
                new SqlParameter("@Username", employee.Username),
                new SqlParameter("@Password", employee.Password));

            return rows > 0;
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            const string query = @"UPDATE Employees SET FullName=@FullName, Phone=@Phone, Email=@Email, Address=@Address, Role=@Role,
                                   Salary=@Salary, Shift=@Shift, JoinDate=@JoinDate, Username=@Username, Password=@Password
                                   WHERE EmployeeID=@EmployeeID";
            int rows = await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@FullName", employee.FullName),
                new SqlParameter("@Phone", employee.Phone),
                new SqlParameter("@Email", employee.Email),
                new SqlParameter("@Address", employee.Address),
                new SqlParameter("@Role", employee.Role),
                new SqlParameter("@Salary", employee.Salary),
                new SqlParameter("@Shift", employee.Shift),
                new SqlParameter("@JoinDate", employee.JoinDate),
                new SqlParameter("@Username", employee.Username),
                new SqlParameter("@Password", employee.Password),
                new SqlParameter("@EmployeeID", employee.EmployeeID));
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int employeeId)
        {
            await _dbHelper.ExecuteTransactionAsync(async (connection, transaction) =>
            {
                SqlCommand attendance = new SqlCommand("DELETE FROM Attendance WHERE EmployeeID=@EmployeeID", connection, transaction);
                attendance.Parameters.AddWithValue("@EmployeeID", employeeId);
                await attendance.ExecuteNonQueryAsync();

                SqlCommand salary = new SqlCommand("DELETE FROM SalaryRecords WHERE EmployeeID=@EmployeeID", connection, transaction);
                salary.Parameters.AddWithValue("@EmployeeID", employeeId);
                await salary.ExecuteNonQueryAsync();

                SqlCommand employee = new SqlCommand("DELETE FROM Employees WHERE EmployeeID=@EmployeeID", connection, transaction);
                employee.Parameters.AddWithValue("@EmployeeID", employeeId);
                int rows = await employee.ExecuteNonQueryAsync();
                if (rows <= 0)
                {
                    throw new ApplicationException("Employee delete failed.");
                }
            });

            return true;
        }
    }
}
