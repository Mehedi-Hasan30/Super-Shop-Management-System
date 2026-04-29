using System;

namespace Super_Shop_Management_System.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public int CategoryID { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string SupplierName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
