using System;

namespace Super_Shop_Management_System.Models
{
    public class ProfitLossReport
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalPurchaseCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal VATCollected { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}

