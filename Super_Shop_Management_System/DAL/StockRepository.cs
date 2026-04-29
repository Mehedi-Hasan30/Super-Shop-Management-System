using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.DAL
{
    public class StockRepository
    {
        public async Task DeductStockAsync(SqlConnection connection, SqlTransaction transaction, int productId, int quantity)
        {
            if (connection == null)
            {
                throw new ApplicationException("Database connection is required.");
            }

            if (transaction == null)
            {
                throw new ApplicationException("Database transaction is required.");
            }

            if (productId <= 0)
            {
                throw new ApplicationException("Invalid product selected.");
            }

            if (quantity <= 0)
            {
                throw new ApplicationException("Quantity must be greater than zero.");
            }

            // Conditional update prevents stock from going negative.
            const string deductSql = @"
UPDATE Products WITH (ROWLOCK, UPDLOCK)
SET StockQuantity = StockQuantity - @Quantity
WHERE ProductID = @ProductID AND StockQuantity >= @Quantity;";

            using (SqlCommand command = new SqlCommand(deductSql, connection, transaction))
            {
                command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@Quantity", quantity));
                command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@ProductID", productId));

                int rows = await command.ExecuteNonQueryAsync();
                if (rows <= 0)
                {
                    throw new ApplicationException($"Insufficient stock for product ID {productId}. Transaction cannot be completed.");
                }
            }
        }
    }
}

