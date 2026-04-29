using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class CategoryRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public List<Category> GetAll(string searchKeyword = "")
        {
            const string query = @"SELECT CategoryID, CategoryName, Description, CreatedDate
                                   FROM Categories
                                   WHERE (@Keyword = '' OR CategoryName LIKE '%' + @Keyword + '%' OR Description LIKE '%' + @Keyword + '%')
                                   ORDER BY CategoryName";

            DataTable table = _dbHelper.ExecuteDataTable(query, new SqlParameter("@Keyword", searchKeyword ?? string.Empty));
            List<Category> categories = new List<Category>();

            foreach (DataRow row in table.Rows)
            {
                categories.Add(new Category
                {
                    CategoryID = Convert.ToInt32(row["CategoryID"]),
                    CategoryName = row["CategoryName"].ToString(),
                    Description = row["Description"].ToString(),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }

            return categories;
        }

        public bool Add(Category category)
        {
            const string query = @"INSERT INTO Categories (CategoryName, Description, CreatedDate)
                                   VALUES (@CategoryName, @Description, @CreatedDate)";

            int affectedRows = _dbHelper.ExecuteNonQuery(query,
                new SqlParameter("@CategoryName", category.CategoryName),
                new SqlParameter("@Description", category.Description),
                new SqlParameter("@CreatedDate", category.CreatedDate));

            return affectedRows > 0;
        }

        public bool Update(Category category)
        {
            const string query = @"UPDATE Categories
                                   SET CategoryName = @CategoryName,
                                       Description = @Description
                                   WHERE CategoryID = @CategoryID";

            int affectedRows = _dbHelper.ExecuteNonQuery(query,
                new SqlParameter("@CategoryName", category.CategoryName),
                new SqlParameter("@Description", category.Description),
                new SqlParameter("@CategoryID", category.CategoryID));

            return affectedRows > 0;
        }

        public bool Delete(int categoryId)
        {
            const string query = "DELETE FROM Categories WHERE CategoryID = @CategoryID";
            int affectedRows = _dbHelper.ExecuteNonQuery(query, new SqlParameter("@CategoryID", categoryId));
            return affectedRows > 0;
        }
    }
}
