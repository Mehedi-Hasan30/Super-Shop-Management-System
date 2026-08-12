using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public class DashboardUserControl : UserControl
    {
        private readonly DashboardService _dashboardService = new DashboardService();

        public DashboardUserControl()
        {
            Dock = DockStyle.Fill;
            BackColor = ThemeManager.Background;
            Tag = ThemeManager.ThemeExemptTag;

            var title = new Label
            {
                Text = "Dashboard",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var subtitle = new Label
            {
                Text = "Overview of store operations",
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText,
                AutoSize = true,
                Location = new Point(20, 50)
            };
            Controls.Add(subtitle);

            var loadBtn = new RoundedButton
            {
                Text = "Load Metrics",
                Width = 120,
                Height = 36,
                Location = new Point(20, 90),
                CornerRadius = 6
            };
            loadBtn.Click += async (_, __) => await LoadMetricsAsync();
            Controls.Add(loadBtn);

            var sampleCard = UIStyleKit.CreateStatCard("Daily Sales", "0", IconHelper.Glyphs.Sales, ThemeManager.Success, 200, 90);
            sampleCard.Location = new Point(20, 140);
            Controls.Add(sampleCard);

            var sampleCard2 = UIStyleKit.CreateStatCard("Total Products", "0", IconHelper.Glyphs.Products, ThemeManager.Info, 200, 90);
            sampleCard2.Location = new Point(240, 140);
            Controls.Add(sampleCard2);
        }

        private async Task LoadMetricsAsync()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                UseWaitCursor = true;
                var metrics = await _dashboardService.GetDashboardMetricsAsync();
                MessageBox.Show($"Daily Sales: {metrics["DailySales"]}\nMonthly Sales: {metrics["MonthlySales"]}\nTotal Products: {metrics["TotalProducts"]}", "Dashboard Metrics", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading metrics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
            }
        }
    }
}