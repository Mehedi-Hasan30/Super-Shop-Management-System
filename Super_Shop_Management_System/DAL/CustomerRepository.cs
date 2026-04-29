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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public async Task<List<Customer>> GetAllAsync(string searchKeyword = "")
        {
            const string query = @"SELECT c.CustomerID, c.FullName, c.Phone, c.Email, c.Address, c.LoyaltyPoints, c.CreatedDate,
                                          ISNULL(SUM(cp.Amount), 0) AS TotalPurchases
                                   FROM Customers c
                                   LEFT JOIN CustomerPurchases cp ON cp.CustomerID = c.CustomerID
                                   WHERE (@Keyword = '' OR c.FullName LIKE '%' + @Keyword + '%' OR c.Phone LIKE '%' + @Keyword + '%' OR c.Email LIKE '%' + @Keyword + '%')
                                   GROUP BY c.CustomerID, c.FullName, c.Phone, c.Email, c.Address, c.LoyaltyPoints, c.CreatedDate
                                   ORDER BY c.FullName";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query, new SqlParameter("@Keyword", searchKeyword ?? string.Empty));
            List<Customer> customers = new List<Customer>();
            foreach (DataRow row in table.Rows)
            {
                customers.Add(new Customer
                {
                    CustomerID = Convert.ToInt32(row["CustomerID"]),
                    FullName = row["FullName"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString(),
                    Address = row["Address"].ToString(),
                    LoyaltyPoints = Convert.ToInt32(row["LoyaltyPoints"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    TotalPurchases = Convert.ToDecimal(row["TotalPurchases"])
                });
            }

            return customers;
        }

        public async Task<bool> AddAsync(Customer customer)
        {
            const string query = @"INSERT INTO Customers (FullName, Phone, Email, Address, LoyaltyPoints, CreatedDate)
                                   VALUES (@FullName, @Phone, @Email, @Address, @LoyaltyPoints, @CreatedDate)";
            int rows = await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@FullName", customer.FullName),
                new SqlParameter("@Phone", customer.Phone),
                new SqlParameter("@Email", customer.Email),
                new SqlParameter("@Address", customer.Address),
                new SqlParameter("@LoyaltyPoints", customer.LoyaltyPoints),
                new SqlParameter("@CreatedDate", customer.CreatedDate));
            return rows > 0;
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            const string query = @"UPDATE Customers SET FullName=@FullName, Phone=@Phone, Email=@Email, Address=@Address, LoyaltyPoints=@LoyaltyPoints
                                   WHERE CustomerID=@CustomerID";
            int rows = await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@FullName", customer.FullName),
                new SqlParameter("@Phone", customer.Phone),
                new SqlParameter("@Email", customer.Email),
                new SqlParameter("@Address", customer.Address),
                new SqlParameter("@LoyaltyPoints", customer.LoyaltyPoints),
                new SqlParameter("@CustomerID", customer.CustomerID));
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int customerId)
        {
            await _dbHelper.ExecuteTransactionAsync(async (connection, transaction) =>
            {
                SqlCommand deleteHistory = new SqlCommand("DELETE FROM CustomerPurchases WHERE CustomerID=@CustomerID", connection, transaction);
                deleteHistory.Parameters.AddWithValue("@CustomerID", customerId);
                await deleteHistory.ExecuteNonQueryAsync();

                SqlCommand deleteCustomer = new SqlCommand("DELETE FROM Customers WHERE CustomerID=@CustomerID", connection, transaction);
                deleteCustomer.Parameters.AddWithValue("@CustomerID", customerId);
                int affected = await deleteCustomer.ExecuteNonQueryAsync();
                if (affected <= 0)
                {
                    throw new ApplicationException("Customer delete failed.");
                }
            });
            return true;
        }
    }
}
