using System.Data;

namespace Super_Shop_Management_System.Models
{
    public class AnalyticsBundle
    {
        public DataTable SalesTrend { get; set; }
        public DataTable RevenueTrend { get; set; }
        public DataTable TopSellingProducts { get; set; }
        public DataTable BestCustomers { get; set; }
        public DataTable MonthlyGrossProfit { get; set; }
        public DataTable InventoryMovementTrend { get; set; }
        public DataTable PaymentMethodBreakdown { get; set; }
        public DataTable PendingVsPaid { get; set; }
    }
}

