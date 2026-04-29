using System;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Forms;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ThemeManager.LoadThemePreference();
            ThemeManager.SetTheme(AppTheme.Light); // ✅ Light theme force করা হয়েছে

            try
            {
                HealthCheckResult health = HealthCheckService.ValidateAsync().GetAwaiter().GetResult();
                if (!health.IsHealthy)
                {
                    string message = "Startup validation failed.\n\n" +
                                      (health.Errors.Count > 0 ? string.Join(Environment.NewLine, health.Errors) : string.Empty);
                    MessageBox.Show(message, "Health Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Health Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SeedValidator.EnsureInitializedAsync().GetAwaiter().GetResult();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}