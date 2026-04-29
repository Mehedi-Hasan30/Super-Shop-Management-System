using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class ProductRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public List<Product> GetAll(string searchKeyword = "")
        {
            const string query = @"SELECT ProductID, ProductName, Barcode, CategoryID, PurchasePrice, SellingPrice,
                                          StockQuantity, ReorderLevel, ExpiryDate, SupplierName, Description, CreatedDate
                                   FROM Products
                                   WHERE (@Keyword = '' OR ProductName LIKE '%' + @Keyword + '%' OR Barcode LIKE '%' + @Keyword + '%' OR SupplierName LIKE '%' + @Keyword + '%')
                                   ORDER BY ProductName";

            DataTable table = _dbHelper.ExecuteDataTable(query, new SqlParameter("@Keyword", searchKeyword ?? string.Empty));
            List<Product> products = new List<Product>();

            foreach (DataRow row in table.Rows)
            {
                products.Add(MapProduct(row));
            }

            return products;
        }

        public bool Add(Product product)
        {
            const string query = @"INSERT INTO Products
                                   (ProductName, Barcode, CategoryID, PurchasePrice, SellingPrice, StockQuantity, ReorderLevel, ExpiryDate, SupplierName, Description, CreatedDate)
                                   VALUES
                                   (@ProductName, @Barcode, @CategoryID, @PurchasePrice, @SellingPrice, @StockQuantity, @ReorderLevel, @ExpiryDate, @SupplierName, @Description, @CreatedDate)";

            int affectedRows = _dbHelper.ExecuteNonQuery(query, BuildProductParameters(product, false));
            return affectedRows > 0;
        }

        public bool Update(Product product)
        {
            const string query = @"UPDATE Products
                                   SET ProductName = @ProductName,
                                       Barcode = @Barcode,
                                       CategoryID = @CategoryID,
                                       PurchasePrice = @PurchasePrice,
                                       SellingPrice = @SellingPrice,
                                       StockQuantity = @StockQuantity,
                                       ReorderLevel = @ReorderLevel,
                                       ExpiryDate = @ExpiryDate,
                                       SupplierName = @SupplierName,
                                       Description = @Description
                                   WHERE ProductID = @ProductID";

            int affectedRows = _dbHelper.ExecuteNonQuery(query, BuildProductParameters(product, true));
            return affectedRows > 0;
        }

        public bool Delete(int productId)
        {
            const string query = "DELETE FROM Products WHERE ProductID = @ProductID";
            int affectedRows = _dbHelper.ExecuteNonQuery(query, new SqlParameter("@ProductID", productId));
            return affectedRows > 0;
        }

        private static SqlParameter[] BuildProductParameters(Product product, bool includeId)
        {
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@ProductName", product.ProductName),
                new SqlParameter("@Barcode", product.Barcode),
                new SqlParameter("@CategoryID", product.CategoryID),
                new SqlParameter("@PurchasePrice", product.PurchasePrice),
                new SqlParameter("@SellingPrice", product.SellingPrice),
                new SqlParameter("@StockQuantity", product.StockQuantity),
                new SqlParameter("@ReorderLevel", product.ReorderLevel),
                new SqlParameter("@ExpiryDate", (object)product.ExpiryDate ?? DBNull.Value),
                new SqlParameter("@SupplierName", product.SupplierName),
                new SqlParameter("@Description", product.Description),
                new SqlParameter("@CreatedDate", product.CreatedDate)
            };

            if (includeId)
            {
                parameters.Add(new SqlParameter("@ProductID", product.ProductID));
            }

            return parameters.ToArray();
        }

        private static Product MapProduct(DataRow row)
        {
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
    }
}
