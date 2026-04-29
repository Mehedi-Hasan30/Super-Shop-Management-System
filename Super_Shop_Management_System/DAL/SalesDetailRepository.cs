using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class SalesDetailRepository
    {
        public async Task InsertDetailsAsync(SqlConnection connection, SqlTransaction transaction, int saleId, IEnumerable<SalesDetail> details)
        {
            if (connection == null)
            {
                throw new ApplicationException("Database connection is required.");
            }

            if (transaction == null)
            {
                throw new ApplicationException("Database transaction is required.");
            }

            if (saleId <= 0)
            {
                throw new ApplicationException("Invalid sale id.");
            }

            if (details == null)
            {
                throw new ApplicationException("Sale details are required.");
            }

            const string insertDetailSql = @"
INSERT INTO SalesDetails
    (SaleID, ProductID, Quantity, UnitPrice, SubTotal)
VALUES
    (@SaleID, @ProductID, @Quantity, @UnitPrice, @SubTotal);";

            foreach (SalesDetail detail in details)
            {
                if (detail == null)
                {
                    continue;
                }

                using (SqlCommand insertDetailCommand = new SqlCommand(insertDetailSql, connection, transaction))
                {
                    insertDetailCommand.Parameters.Add(new SqlParameter("@SaleID", saleId));
                    insertDetailCommand.Parameters.Add(new SqlParameter("@ProductID", detail.ProductID));
                    insertDetailCommand.Parameters.Add(new SqlParameter("@Quantity", detail.Quantity));
                    insertDetailCommand.Parameters.Add(new SqlParameter("@UnitPrice", detail.UnitPrice));
                    insertDetailCommand.Parameters.Add(new SqlParameter("@SubTotal", detail.SubTotal));

                    int rows = await insertDetailCommand.ExecuteNonQueryAsync();
                    if (rows <= 0)
                    {
                        throw new ApplicationException("Failed to insert sale details.");
                    }
                }
            }
        }
    }
}

