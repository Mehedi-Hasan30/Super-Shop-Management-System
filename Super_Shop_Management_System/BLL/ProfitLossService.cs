using System;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class ProfitLossService
    {
        private readonly ReportsRepository _reportsRepository = new ReportsRepository();

        public Task<ProfitLossReport> GetProfitLossAsync(ReportFilters filters)
        {
            return _reportsRepository.GetProfitLossSummaryAsync(filters);
        }

        public Task<System.Data.DataTable> GetMonthlyProfitChartAsync(ReportFilters filters)
        {
            return _reportsRepository.GetMonthlyGrossProfitAsync(filters);
        }
    }
}

