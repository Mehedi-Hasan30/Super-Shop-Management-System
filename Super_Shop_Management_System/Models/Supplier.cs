using System;

namespace Super_Shop_Management_System.Models
{
    public class Supplier
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ProductType { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalTransactions { get; set; }
    }
}
