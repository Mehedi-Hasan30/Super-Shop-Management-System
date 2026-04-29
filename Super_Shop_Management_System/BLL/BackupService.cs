using System;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.BLL
{
    public class BackupService
    {
        private const string DbBackupExtension = ".bak";

        private string GetConnectionString()
        {
            return System.Configuration.ConfigurationManager
                .ConnectionStrings["SuperShopConnection"].ConnectionString;
        }

        private static string GetDatabaseNameFromConnectionString(string connectionString)
        {
            // Simple parse for "Initial Catalog="
            string marker = "Initial Catalog=";
            int start = connectionString.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0) return "SuperShopDB";
            start += marker.Length;
            int end = connectionString.IndexOf(';', start);
            if (end < 0) end = connectionString.Length;
            string db = connectionString.Substring(start, end - start).Trim();
            return string.IsNullOrWhiteSpace(db) ? "SuperShopDB" : db;
        }

        private static string BuildBackupDirectoryPath()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        public string BuildDefaultBackupFilePath()
        {
            string dir = BuildBackupDirectoryPath();
            return Path.Combine(dir, $"SuperShopDB_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        }

        public async Task<string> CreateFullBackupAsync(string backupFilePath)
        {
            if (string.IsNullOrWhiteSpace(backupFilePath))
            {
                throw new ApplicationException("Backup file path is required.");
            }

            if (!backupFilePath.EndsWith(DbBackupExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException("Backup file must have a .bak extension.");
            }

            string fullPath = Path.GetFullPath(backupFilePath);
            string folder = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                throw new ApplicationException("Backup folder does not exist.");
            }

            string connectionString = GetConnectionString();
            string dbName = GetDatabaseNameFromConnectionString(connectionString);

            // backup folder safety: disallow backups to root or system folders
            if (fullPath.IndexOf("Windows", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new ApplicationException("Backup location is not allowed.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    await connection.OpenAsync();
                    command.CommandTimeout = 0;
                    command.CommandText = $@"
BACKUP DATABASE [{dbName}]
TO DISK = @Path
WITH INIT,
     COMPRESSION,
     STATS = 10;";
                    command.Parameters.AddWithValue("@Path", fullPath);
                    await command.ExecuteNonQueryAsync();
                }

                // Update local reminder state.
                UpdateLastBackupTimestampUtc(DateTime.UtcNow);
                return fullPath;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("BackupService.CreateFullBackupAsync", ex);
                throw new ApplicationException("Backup failed: " + ex.Message);
            }
        }

        public async Task RestoreBackupAsync(string backupFilePath)
        {
            if (string.IsNullOrWhiteSpace(backupFilePath))
            {
                throw new ApplicationException("Backup file path is required.");
            }

            if (!File.Exists(backupFilePath))
            {
                throw new ApplicationException("Backup file does not exist.");
            }

            if (!backupFilePath.EndsWith(DbBackupExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException("Restore file must have a .bak extension.");
            }

            string fullPath = Path.GetFullPath(backupFilePath);
            string connectionString = GetConnectionString();
            string dbName = GetDatabaseNameFromConnectionString(connectionString);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;

                        // Use SINGLE_USER to safely drop/overwrite active connections.
                        cmd.CommandText = $@"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                        await cmd.ExecuteNonQueryAsync();

                        cmd.CommandText = $@"
RESTORE DATABASE [{dbName}]
FROM DISK = @Path
WITH REPLACE,
     RECOVERY;";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Path", fullPath);
                        await cmd.ExecuteNonQueryAsync();

                        cmd.CommandText = $@"ALTER DATABASE [{dbName}] SET MULTI_USER;";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // Update reminder after restore.
                UpdateLastBackupTimestampUtc(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("BackupService.RestoreBackupAsync", ex);
                // Best-effort return to MULTI_USER.
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        await connection.OpenAsync();
                        cmd.CommandText = $@"ALTER DATABASE [{dbName}] SET MULTI_USER;";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    // ignore
                }

                throw new ApplicationException("Restore failed: " + ex.Message);
            }
        }

        private static void UpdateLastBackupTimestampUtc(DateTime utcTime)
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SuperShopManagementSystem");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, "last_backup_utc.txt");
                File.WriteAllText(path, utcTime.ToString("O"));
            }
            catch
            {
                // ignore
            }
        }

        public static DateTime? GetLastBackupTimestampUtc()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SuperShopManagementSystem");
                string path = Path.Combine(folder, "last_backup_utc.txt");
                if (!File.Exists(path)) return null;
                string text = File.ReadAllText(path).Trim();
                if (string.IsNullOrWhiteSpace(text)) return null;
                if (DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    return dt.ToUniversalTime();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

