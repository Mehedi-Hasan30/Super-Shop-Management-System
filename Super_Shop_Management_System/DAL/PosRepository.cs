using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class PosRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public async Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return null;
            }

            const string query = @"SELECT ProductID, ProductName, Barcode, CategoryID, PurchasePrice, SellingPrice,
                                          StockQuantity, ReorderLevel, ExpiryDate, SupplierName, Description, CreatedDate
                                   FROM Products
                                   WHERE Barcode = @Barcode";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query, new SqlParameter("@Barcode", barcode.Trim()));
            if (table.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = table.Rows[0];
            return new Product
            {
                ProductID = Convert.ToInt32(row["ProductID"]),
                ProductName = row["ProductName"].ToString(),
                Barcode = row["Barcode"].ToString(),
                CategoryID = Convert.ToInt32(row["CategoryID"]),
                PurchasePrice = Convert.ToDecimal(row["PurchasePrice"]),
                SellingPrice = Convert.ToDecimal(row["SellingPrice"]),
                StockQuantity = Convert.ToInt32(row["StockQuantity"]),
                ReorderLevel = Convert.ToInt32(row["ReorderLevel"]),
                ExpiryDate = row["ExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ExpiryDate"]),
                SupplierName = row["SupplierName"].ToString(),
                Description = row["Description"].ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        public async Task<List<Product>> GetProductsAsync(string searchKeyword = "")
        {
            const string query = @"SELECT ProductID, ProductName, Barcode, CategoryID, PurchasePrice, SellingPrice,
                                          StockQuantity, ReorderLevel, ExpiryDate, SupplierName, Description, CreatedDate
                                   FROM Products
                                   WHERE (@Keyword = '' OR ProductName LIKE '%' + @Keyword + '%' OR Barcode LIKE '%' + @Keyword + '%')
                                   ORDER BY ProductName";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query, new SqlParameter("@Keyword", searchKeyword ?? string.Empty));
            List<Product> products = new List<Product>();

            foreach (DataRow row in table.Rows)
            {
                products.Add(new Product
                {
                    ProductID = Convert.ToInt32(row["ProductID"]),
                    ProductName = row["ProductName"].ToString(),
                    Barcode = row["Barcode"].ToString(),
                    CategoryID = Convert.ToInt32(row["CategoryID"]),
                    PurchasePrice = Convert.ToDecimal(row["PurchasePrice"]),
                    SellingPrice = Convert.ToDecimal(row["SellingPrice"]),
                    StockQuantity = Convert.ToInt32(row["StockQuantity"]),
                    ReorderLevel = Convert.ToInt32(row["ReorderLevel"]),
                    ExpiryDate = row["ExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ExpiryDate"]),
                    SupplierName = row["SupplierName"].ToString(),
                    Description = row["Description"].ToString(),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }

            return products;
        }
    }
}
