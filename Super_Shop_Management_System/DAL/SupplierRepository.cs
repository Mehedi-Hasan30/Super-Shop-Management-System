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
    public class SupplierRepository : ISupplierRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public async Task<List<Supplier>> GetAllAsync(string searchKeyword = "")
        {
            const string query = @"SELECT s.SupplierID, s.SupplierName, s.CompanyName, s.Phone, s.Email, s.Address, s.ProductType, s.CreatedDate,
                                          ISNULL(SUM(st.Amount), 0) AS TotalTransactions
                                   FROM Suppliers s
                                   LEFT JOIN SupplierTransactions st ON st.SupplierID = s.SupplierID
                                   WHERE (@Keyword = '' OR s.SupplierName LIKE '%' + @Keyword + '%' OR s.CompanyName LIKE '%' + @Keyword + '%' OR s.Phone LIKE '%' + @Keyword + '%')
                                   GROUP BY s.SupplierID, s.SupplierName, s.CompanyName, s.Phone, s.Email, s.Address, s.ProductType, s.CreatedDate
                                   ORDER BY s.SupplierName";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query, new SqlParameter("@Keyword", searchKeyword ?? string.Empty));
            List<Supplier> suppliers = new List<Supplier>();
            foreach (DataRow row in table.Rows)
            {
                suppliers.Add(new Supplier
                {
                    SupplierID = Convert.ToInt32(row["SupplierID"]),
                    SupplierName = row["SupplierName"].ToString(),
                    CompanyName = row["CompanyName"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString(),
                    Address = row["Address"].ToString(),
                    ProductType = row["ProductType"].ToString(),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                    TotalTransactions = Convert.ToDecimal(row["TotalTransactions"])
                });
            }

            return suppliers;
        }

        public async Task<bool> AddAsync(Supplier supplier)
        {
            const string query = @"INSERT INTO Suppliers (SupplierName, CompanyName, Phone, Email, Address, ProductType, CreatedDate)
                                   VALUES (@SupplierName, @CompanyName, @Phone, @Email, @Address, @ProductType, @CreatedDate)";
            int rows = await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@SupplierName", supplier.SupplierName),
                new SqlParameter("@CompanyName", supplier.CompanyName),
                new SqlParameter("@Phone", supplier.Phone),
                new SqlParameter("@Email", supplier.Email),
                new SqlParameter("@Address", supplier.Address),
                new SqlParameter("@ProductType", supplier.ProductType),
                new SqlParameter("@CreatedDate", supplier.CreatedDate));
            return rows > 0;
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            const string query = @"UPDATE Suppliers SET SupplierName=@SupplierName, CompanyName=@CompanyName, Phone=@Phone, Email=@Email,
                                   Address=@Address, ProductType=@ProductType WHERE SupplierID=@SupplierID";
            int rows = await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@SupplierName", supplier.SupplierName),
                new SqlParameter("@CompanyName", supplier.CompanyName),
                new SqlParameter("@Phone", supplier.Phone),
                new SqlParameter("@Email", supplier.Email),
                new SqlParameter("@Address", supplier.Address),
                new SqlParameter("@ProductType", supplier.ProductType),
                new SqlParameter("@SupplierID", supplier.SupplierID));
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int supplierId)
        {
            await _dbHelper.ExecuteTransactionAsync(async (connection, transaction) =>
            {
                SqlCommand deleteHistory = new SqlCommand("DELETE FROM SupplierTransactions WHERE SupplierID=@SupplierID", connection, transaction);
                deleteHistory.Parameters.AddWithValue("@SupplierID", supplierId);
                await deleteHistory.ExecuteNonQueryAsync();

                SqlCommand deleteSupplier = new SqlCommand("DELETE FROM Suppliers WHERE SupplierID=@SupplierID", connection, transaction);
                deleteSupplier.Parameters.AddWithValue("@SupplierID", supplierId);
                int affected = await deleteSupplier.ExecuteNonQueryAsync();
                if (affected <= 0)
                {
                    throw new ApplicationException("Supplier delete failed.");
                }
            });
            return true;
        }
    }
}
