using System;

namespace Super_Shop_Management_System.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int LoyaltyPoints { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalPurchases { get; set; }
    }
}
