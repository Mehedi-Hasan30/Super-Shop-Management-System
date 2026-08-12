using System;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public class SettingsUserControl : UserControl
    {
        public SettingsUserControl()
        {
            Dock = DockStyle.Fill;
            BackColor = ThemeManager.Background;
            Tag = ThemeManager.ThemeExemptTag;

            var title = new Label
            {
                Text = "Settings",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var themeToggle = new RoundedButton
            {
                Text = "Toggle Theme",
                Width = 120,
                Height = 36,
                Location = new Point(20, 70),
                CornerRadius = 6
            };
            themeToggle.Click += (_, __) =>
            {
                ThemeManager.ToggleTheme();
                MessageBox.Show($"Theme switched to {(ThemeManager.CurrentTheme.ToString() == "Dark" ? "Dark" : "Light")}", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            Controls.Add(themeToggle);

            var logoutBtn = new RoundedButton
            {
                Text = "Logout",
                Width = 120,
                Height = 36,
                Location = new Point(160, 70),
                CornerRadius = 6
            };
            logoutBtn.Click += (_, __) =>
            {
                var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    MessageBox.Show("Logged out successfully", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            Controls.Add(logoutBtn);
        }
    }
}