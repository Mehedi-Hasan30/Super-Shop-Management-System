using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;

namespace Super_Shop_Management_System.BLL
{
    public class DashboardService
    {
        private readonly DashboardRepository _dashboardRepository = new DashboardRepository();
        private readonly AttendanceRepository _attendanceRepository = new AttendanceRepository();

        public async Task<Dictionary<string, string>> GetDashboardMetricsAsync()
        {
            decimal dailySales = await _dashboardRepository.GetDailySalesAsync();
            decimal monthlySales = await _dashboardRepository.GetMonthlySalesAsync();
            int totalProducts = await _dashboardRepository.GetTotalProductsAsync();
            int totalCategories = await _dashboardRepository.GetTotalCategoriesAsync();
            int lowStock = await _dashboardRepository.GetLowStockCountAsync();
            int totalCustomers = await _dashboardRepository.GetTotalCustomersAsync();
            int totalSuppliers = await _dashboardRepository.GetTotalSuppliersAsync();
            int totalEmployees = await _dashboardRepository.GetTotalEmployeesAsync();
            int attendanceSummary = await _attendanceRepository.GetTodayPresentCountAsync();

            return new Dictionary<string, string>
            {
                ["DailySales"] = dailySales.ToString("C"),
                ["MonthlySales"] = monthlySales.ToString("C"),
                ["TotalProducts"] = totalProducts.ToString(),
                ["TotalCategories"] = totalCategories.ToString(),
                ["LowStock"] = lowStock.ToString(),
                ["TotalCustomers"] = totalCustomers.ToString(),
                ["TotalSuppliers"] = totalSuppliers.ToString(),
                ["TotalEmployees"] = totalEmployees.ToString(),
                ["AttendanceSummary"] = attendanceSummary.ToString()
            };
        }
    }
}
