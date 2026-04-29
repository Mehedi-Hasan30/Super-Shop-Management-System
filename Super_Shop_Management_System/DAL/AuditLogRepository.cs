using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class AuditLogRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public async Task<int> AddAsync(AuditLog log)
        {
            const string query = @"INSERT INTO AuditLogs (ActionType, TableName, RecordID, [User], [Timestamp], Description)
                                   VALUES (@ActionType, @TableName, @RecordID, @User, @Timestamp, @Description)";

            return await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@ActionType", log.ActionType),
                new SqlParameter("@TableName", log.TableName),
                new SqlParameter("@RecordID", log.RecordID),
                new SqlParameter("@User", log.UserName),
                new SqlParameter("@Timestamp", log.Timestamp),
                new SqlParameter("@Description", log.Description));
        }

        public int Add(AuditLog log)
        {
            const string query = @"INSERT INTO AuditLogs (ActionType, TableName, RecordID, [User], [Timestamp], Description)
                                   VALUES (@ActionType, @TableName, @RecordID, @User, @Timestamp, @Description)";

            return _dbHelper.ExecuteNonQuery(query,
                new SqlParameter("@ActionType", log.ActionType),
                new SqlParameter("@TableName", log.TableName),
                new SqlParameter("@RecordID", log.RecordID),
                new SqlParameter("@User", log.UserName),
                new SqlParameter("@Timestamp", log.Timestamp),
                new SqlParameter("@Description", log.Description));
        }

        public Task<List<AuditLog>> GetRecentAsync(int top = 200)
        {
            return GetFilteredAsync(string.Empty, string.Empty, string.Empty, null, null, top);
        }

        public async Task<List<AuditLog>> GetFilteredAsync(string actionType, string tableName, string userName, DateTime? fromDate, DateTime? toDate, int top = 500)
        {
            const string query = @"SELECT TOP (@Top) AuditLogID, ActionType, TableName, RecordID, [User], [Timestamp], Description
                                   FROM AuditLogs
                                   WHERE (@ActionType = '' OR ActionType = @ActionType)
                                     AND (@TableName = '' OR TableName = @TableName)
                                     AND (@UserName = '' OR [User] LIKE '%' + @UserName + '%')
                                     AND (@FromDate IS NULL OR [Timestamp] >= @FromDate)
                                     AND (@ToDate IS NULL OR [Timestamp] < DATEADD(DAY, 1, @ToDate))
                                   ORDER BY AuditLogID DESC";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query,
                new SqlParameter("@Top", top),
                new SqlParameter("@ActionType", actionType ?? string.Empty),
                new SqlParameter("@TableName", tableName ?? string.Empty),
                new SqlParameter("@UserName", userName ?? string.Empty),
                new SqlParameter("@FromDate", (object)fromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object)toDate ?? DBNull.Value));

            List<AuditLog> logs = new List<AuditLog>();
            foreach (DataRow row in table.Rows)
            {
                logs.Add(new AuditLog
                {
                    AuditLogID = Convert.ToInt32(row["AuditLogID"]),
                    ActionType = row["ActionType"].ToString(),
                    TableName = row["TableName"].ToString(),
                    RecordID = row["RecordID"].ToString(),
                    UserName = row["User"].ToString(),
                    Timestamp = Convert.ToDateTime(row["Timestamp"]),
                    Description = row["Description"].ToString()
                });
            }

            return logs;
        }

        public async Task<List<string>> GetDistinctTableNamesAsync()
        {
            const string query = "SELECT DISTINCT TableName FROM AuditLogs ORDER BY TableName";
            DataTable table = await _dbHelper.ExecuteDataTableAsync(query);
            List<string> values = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                values.Add(row["TableName"].ToString());
            }

            return values;
        }

        public async Task<List<string>> GetDistinctUsersAsync()
        {
            const string query = "SELECT DISTINCT [User] FROM AuditLogs WHERE [User] IS NOT NULL AND [User] <> '' ORDER BY [User]";
            DataTable table = await _dbHelper.ExecuteDataTableAsync(query);
            List<string> values = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                values.Add(row["User"].ToString());
            }

            return values;
        }
    }
}
