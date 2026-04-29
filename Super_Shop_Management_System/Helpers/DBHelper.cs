using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Super_Shop_Management_System.Helpers
{
    public class DBHelper
    {
        private readonly string _connectionString;

        public DBHelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SuperShopConnection"].ConnectionString;
        }

        public DataTable ExecuteDataTable(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                DataTable table = new DataTable();
                connection.Open();
                adapter.Fill(table);
                return table;
            }
        }

        public async Task<DataTable> ExecuteDataTableAsync(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                await connection.OpenAsync(); // ✅ connection open করা হচ্ছে
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }

        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }

        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public async Task<object> ExecuteScalarAsync(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                await connection.OpenAsync();
                return await command.ExecuteScalarAsync();
            }
        }

        public async Task ExecuteTransactionAsync(Func<SqlConnection, SqlTransaction, Task> operation)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await operation(connection, transaction);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}