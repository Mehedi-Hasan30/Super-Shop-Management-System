using System;

namespace Super_Shop_Management_System.Models
{
    public class Sale
    {
        public int SaleID { get; set; }
        public int? CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal GrandTotal { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
    }
}

