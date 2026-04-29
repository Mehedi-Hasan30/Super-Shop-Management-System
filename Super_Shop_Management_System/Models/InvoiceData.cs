using System;
using System.Collections.Generic;

namespace Super_Shop_Management_System.Models
{
    public class InvoiceData
    {
        public int SaleID { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public DateTime SaleDate { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal GrandTotal { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }
}

