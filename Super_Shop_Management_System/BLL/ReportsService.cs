using System;
using System.Data;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class ReportsService
    {
        private readonly ReportsRepository _reportsRepository = new ReportsRepository();
        private readonly ProfitLossService _profitLossService = new ProfitLossService();

        public Task<DataTable> GetReportDataAsync(ReportType reportType, ReportFilters filters)
        {
            switch (reportType)
            {
                case ReportType.DailySales:
                    return _reportsRepository.GetDailySalesReportAsync(filters);
                case ReportType.MonthlySales:
                    return _reportsRepository.GetMonthlySalesReportAsync(filters);
                case ReportType.ProductSales:
                    return _reportsRepository.GetProductSalesReportAsync(filters);
                case ReportType.CustomerPurchase:
                    return _reportsRepository.GetCustomerPurchaseReportAsync(filters);
                case ReportType.SupplierPurchase:
                    return _reportsRepository.GetSupplierPurchaseReportAsync(filters);
                case ReportType.EmployeeSales:
                    return _reportsRepository.GetEmployeeSalesReportAsync(filters);
                case ReportType.PendingSales:
                    return _reportsRepository.GetPendingSalesAsync(filters);
                case ReportType.PaidSales:
                    return _reportsRepository.GetPaidSalesAsync(filters);
                case ReportType.InventoryStock:
                    return _reportsRepository.GetInventoryStockReportAsync(filters);
                case ReportType.LowStock:
                    return _reportsRepository.GetLowStockReportAsync(filters);
                case ReportType.ExpiryProducts:
                    return _reportsRepository.GetExpiryProductsReportAsync(filters);
                case ReportType.Attendance:
                    return _reportsRepository.GetAttendanceReportAsync(filters.FromDate, filters.ToDate, filters.EmployeeId);
                default:
                    throw new ApplicationException("Unsupported report type.");
            }
        }

        public Task<ProfitLossReport> GetProfitLossAsync(ReportFilters filters)
        {
            return _profitLossService.GetProfitLossAsync(filters);
        }

        public async Task<AnalyticsBundle> GetAnalyticsAsync(ReportFilters filters, TimeAggregationType aggregation)
        {
            AnalyticsBundle bundle = new AnalyticsBundle
            {
                SalesTrend = await _reportsRepository.GetSalesTrendAsync(filters.FromDate, filters.ToDate, aggregation, filters.PaymentStatus ?? "All"),
                RevenueTrend = await _reportsRepository.GetRevenueTrendAsync(filters.FromDate, filters.ToDate, aggregation, filters.PaymentStatus ?? "All"),
                TopSellingProducts = await _reportsRepository.GetTopSellingProductsAsync(filters.FromDate, filters.ToDate, 5, filters.PaymentStatus ?? "All"),
                BestCustomers = await _reportsRepository.GetBestCustomersAsync(filters.FromDate, filters.ToDate, 5, filters.PaymentStatus ?? "All"),
                MonthlyGrossProfit = await _profitLossService.GetMonthlyProfitChartAsync(filters),
                // Phase 3.2.1: Pending sales do not deduct stock, so inventory movement should reflect Paid sales only.
                InventoryMovementTrend = await _reportsRepository.GetInventoryMovementTrendAsync(filters.FromDate, filters.ToDate, aggregation, "Paid"),
                PaymentMethodBreakdown = await _reportsRepository.GetPaymentMethodBreakdownAsync(filters.FromDate, filters.ToDate, filters.PaymentStatus ?? "All"),
                PendingVsPaid = await _reportsRepository.GetPendingVsPaidComparisonAsync(filters.FromDate, filters.ToDate)
            };

            return bundle;
        }
    }
}

