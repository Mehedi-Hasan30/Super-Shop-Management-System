using System;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.DAL
{
    public class DashboardRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public async Task<decimal> GetDailySalesAsync()
        {
            const string query = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Sales WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)";
            return Convert.ToDecimal(await _dbHelper.ExecuteScalarAsync(query));
        }

        public async Task<decimal> GetMonthlySalesAsync()
        {
            const string query = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Sales WHERE MONTH(SaleDate) = MONTH(GETDATE()) AND YEAR(SaleDate) = YEAR(GETDATE())";
            return Convert.ToDecimal(await _dbHelper.ExecuteScalarAsync(query));
        }

        public async Task<int> GetTotalProductsAsync()
        {
            const string query = "SELECT COUNT(1) FROM Products";
            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query));
        }

        public async Task<int> GetTotalCategoriesAsync()
        {
            const string query = "SELECT COUNT(1) FROM Categories";
            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query));
        }

        public async Task<int> GetLowStockCountAsync()
        {
            const string query = "SELECT COUNT(1) FROM Products WHERE StockQuantity <= ReorderLevel";
            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query));
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            const string query = "SELECT COUNT(1) FROM Customers";
            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query));
        }

        public async Task<int> GetTotalSuppliersAsync()
        {
            const string query = "SELECT COUNT(1) FROM Suppliers";
            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query));
        }

        public async Task<int> GetTotalEmployeesAsync()
        {
            const string query = "SELECT COUNT(1) FROM Employees";
            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query));
        }
    }
}
