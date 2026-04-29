using System;

namespace Super_Shop_Management_System.Models
{
    public class SalesHistoryItem
    {
        public int SaleID { get; set; }
        public string CustomerName { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal GrandTotal { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string EmployeeName { get; set; }
    }
}

