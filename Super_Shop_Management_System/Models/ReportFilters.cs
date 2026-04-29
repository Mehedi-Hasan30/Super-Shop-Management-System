using System;

namespace Super_Shop_Management_System.Models
{
    public class ReportFilters
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public int? ProductId { get; set; }
        public int? SupplierId { get; set; }

        // "All" | "Paid" | "Pending"
        public string PaymentStatus { get; set; } = "All";
    }
}

