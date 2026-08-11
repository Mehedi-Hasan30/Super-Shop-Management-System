using System;
using System.Data;
using System.Data.SqlClient;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class UserRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public User GetByUsername(string username)
        {
            const string query = @"SELECT TOP 1 UserID, FullName, Username, Password, Role, Email, SecurityQuestion, SecurityAnswer, CreatedDate, PasswordAlgorithm
                                   FROM Users
                                   WHERE Username = @Username";

            DataTable table = _dbHelper.ExecuteDataTable(query,
                new SqlParameter("@Username", username));

            return table.Rows.Count == 0 ? null : MapUser(table.Rows[0]);
        }

        public User GetByUsernameAndEmail(string username, string email)
        {
            const string query = @"SELECT TOP 1 UserID, FullName, Username, Password, Role, Email, SecurityQuestion, SecurityAnswer, CreatedDate, PasswordAlgorithm
                                   FROM Users
                                   WHERE Username = @Username AND Email = @Email";

            DataTable table = _dbHelper.ExecuteDataTable(query,
                new SqlParameter("@Username", username),
                new SqlParameter("@Email", email));

            return table.Rows.Count == 0 ? null : MapUser(table.Rows[0]);
        }

        public bool UpdatePassword(int userId, string newPasswordHash, string algorithm)
        {
            const string query = @"UPDATE Users
                                   SET Password = @Password, PasswordAlgorithm = @PasswordAlgorithm
                                   WHERE UserID = @UserID";
            int affectedRows = _dbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Password", newPasswordHash),
                new SqlParameter("@PasswordAlgorithm", algorithm),
                new SqlParameter("@UserID", userId));

            return affectedRows > 0;
        }

        private static User MapUser(DataRow row)
        {
            return new User
            {
                UserID = Convert.ToInt32(row["UserID"]),
                FullName = row["FullName"].ToString(),
                Username = row["Username"].ToString(),
                Password = row["Password"].ToString(),
                Role = row["Role"].ToString(),
                Email = row["Email"].ToString(),
                SecurityQuestion = row["SecurityQuestion"].ToString(),
                SecurityAnswer = row["SecurityAnswer"].ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                PasswordAlgorithm = row.Table.Columns.Contains("PasswordAlgorithm") && row["PasswordAlgorithm"] != DBNull.Value
                    ? row["PasswordAlgorithm"].ToString()
                    : SecurityHelper.AlgorithmSha256
            };
        }
    }
}