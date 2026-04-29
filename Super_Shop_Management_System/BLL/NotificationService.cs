using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class NotificationService
    {
        private readonly DBHelper _dbHelper = new DBHelper();
        private readonly BackupService _backupService = new BackupService();

        public async Task<List<NotificationItem>> GetDashboardNotificationsAsync(int maxCount = 10)
        {
            List<NotificationItem> notifications = new List<NotificationItem>();

            try
            {
                // Low stock
                const string lowStockSql = @"
SELECT TOP (@Top)
       ProductName,
       StockQuantity,
       ReorderLevel,
       ExpiryDate
FROM Products
WHERE StockQuantity <= ReorderLevel
ORDER BY (StockQuantity - ReorderLevel) ASC;";

                DataTable lowDt = await _dbHelper.ExecuteDataTableAsync(
                    lowStockSql,
                    new SqlParameter("@Top", 5));

                foreach (DataRow row in lowDt.Rows)
                {
                    int stock = Convert.ToInt32(row["StockQuantity"]);
                    int reorder = Convert.ToInt32(row["ReorderLevel"]);
                    string name = row["ProductName"].ToString();
                    DateTime? expiry = row["ExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ExpiryDate"]);
                    string expiryText = expiry.HasValue ? $" (Expiry: {expiry.Value:yyyy-MM-dd})" : string.Empty;

                    notifications.Add(new NotificationItem
                    {
                        Type = "LowStock",
                        Title = "Low stock alert",
                        Message = $"{name}: Stock={stock}, ReorderLevel={reorder}{expiryText}",
                        Severity = NotificationSeverity.Warning,
                        CreatedAt = DateTime.Now
                    });
                }

                // Expiry soon
                const string expirySql = @"
SELECT TOP (@Top)
       ProductName,
       Barcode,
       ExpiryDate
FROM Products
WHERE ExpiryDate IS NOT NULL
  AND ExpiryDate <= DATEADD(DAY, @DaysAhead, CAST(GETDATE() AS DATE))
  AND ExpiryDate >= CAST(GETDATE() AS DATE)
ORDER BY ExpiryDate ASC;";

                DataTable expiryDt = await _dbHelper.ExecuteDataTableAsync(
                    expirySql,
                    new SqlParameter("@Top", 5),
                    new SqlParameter("@DaysAhead", 30));

                foreach (DataRow row in expiryDt.Rows)
                {
                    string name = row["ProductName"].ToString();
                    string barcode = row["Barcode"].ToString();
                    DateTime expiry = Convert.ToDateTime(row["ExpiryDate"]);

                    notifications.Add(new NotificationItem
                    {
                        Type = "Expiry",
                        Title = "Expiry soon",
                        Message = $"{name} ({barcode}) expires on {expiry:yyyy-MM-dd}",
                        Severity = NotificationSeverity.Warning,
                        CreatedAt = DateTime.Now
                    });
                }

                // Pending sales reminder
                const string pendingSql = @"
SELECT TOP (@Top)
       SaleID,
       SaleDate,
       CustomerID,
       GrandTotal,
       PaymentMethod
FROM Sales
WHERE PaymentStatus = 'Pending'
ORDER BY SaleDate DESC;";

                DataTable pendingDt = await _dbHelper.ExecuteDataTableAsync(
                    pendingSql,
                    new SqlParameter("@Top", 5));

                foreach (DataRow row in pendingDt.Rows)
                {
                    int saleId = Convert.ToInt32(row["SaleID"]);
                    DateTime saleDate = Convert.ToDateTime(row["SaleDate"]);
                    decimal grandTotal = Convert.ToDecimal(row["GrandTotal"]);
                    string paymentMethod = row["PaymentMethod"].ToString();

                    notifications.Add(new NotificationItem
                    {
                        Type = "PendingSale",
                        Title = "Pending payment sales",
                        Message = $"Sale #{saleId} ({paymentMethod}) - {grandTotal:C} - {saleDate:yyyy-MM-dd HH:mm}",
                        Severity = NotificationSeverity.Info,
                        CreatedAt = DateTime.Now
                    });
                }

                // Backup reminder
                DateTime? lastBackupUtc = BackupService.GetLastBackupTimestampUtc();
                if (!lastBackupUtc.HasValue)
                {
                    notifications.Add(new NotificationItem
                    {
                        Type = "Backup",
                        Title = "Backup required",
                        Message = "No backup timestamp found. Please create a full SQL backup.",
                        Severity = NotificationSeverity.Warning,
                        CreatedAt = DateTime.Now
                    });
                }
                else
                {
                    double days = (DateTime.UtcNow - lastBackupUtc.Value).TotalDays;
                    if (days >= 7)
                    {
                        notifications.Add(new NotificationItem
                        {
                            Type = "Backup",
                            Title = "Backup reminder",
                            Message = $"Last full backup was {lastBackupUtc.Value:yyyy-MM-dd HH:mm} UTC (~{days:0} days ago).",
                            Severity = NotificationSeverity.Warning,
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                // Login security alerts
                const string loginFailSql = @"
SELECT COUNT(1) AS FailCount
FROM AuditLogs
WHERE ActionType = 'LOGIN_FAIL'
  AND [Timestamp] >= DATEADD(MINUTE, -@Minutes, GETDATE());";

                int failCount = Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(
                    loginFailSql,
                    new SqlParameter("@Minutes", 60)));

                if (failCount >= 3)
                {
                    notifications.Add(new NotificationItem
                    {
                        Type = "LoginSecurity",
                        Title = "Security alert: multiple failed logins",
                        Message = $"Detected {failCount} failed login attempts in the last 60 minutes.",
                        Severity = NotificationSeverity.Error,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("NotificationService.GetDashboardNotificationsAsync", ex);
                // Never fail dashboard startup because notifications failed.
            }

            notifications.Sort((a, b) => SeverityRank(b.Severity).CompareTo(SeverityRank(a.Severity)));
            if (notifications.Count > maxCount)
            {
                notifications = notifications.GetRange(0, maxCount);
            }

            return notifications;
        }

        private static int SeverityRank(NotificationSeverity severity)
        {
            // Higher means more critical.
            if (severity == NotificationSeverity.Error) return 3;
            if (severity == NotificationSeverity.Warning) return 2;
            return 1;
        }
    }
}

