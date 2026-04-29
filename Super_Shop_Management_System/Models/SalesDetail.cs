namespace Super_Shop_Management_System.Models
{
    public class SalesDetail
    {
        public int SalesDetailID { get; set; }
        public int SaleID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}

