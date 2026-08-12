using System;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public class ShellHeader : UserControl
    {
        public event EventHandler ThemeToggled;

        private readonly string _userRole;
        private Panel _panel;
        private Label _lblTitle;
        private Label _lblUser;
        private RoundedButton _btnThemeToggle;

        public ShellHeader(string userRole)
        {
            _userRole = userRole;
            Dock = DockStyle.Top;
            Height = 64;
            BackColor = ThemeManager.Sidebar;
            Tag = ThemeManager.ThemeExemptTag;

            _panel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Sidebar };

            _lblTitle = new Label
            {
                Text = "Super Shop Management",
                ForeColor = Color.White,
                Font = ThemeManager.FontH2,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            _lblUser = new Label
            {
                Text = $"User: {SessionManager.FullName} ({SessionManager.Role})",
                ForeColor = Color.White,
                Font = ThemeManager.FontBody,
                AutoSize = true,
                Location = new Point(700, 20)
            };

            _btnThemeToggle = new RoundedButton
            {
                Text = "🌙",
                Width = 40,
                Height = 32,
                Location = new Point(940, 18),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                CornerRadius = 6
            };
            _btnThemeToggle.FlatAppearance.BorderSize = 0;
            UIStyleKit.ApplyRoundedRegion(_btnThemeToggle, 6);
            _btnThemeToggle.MouseEnter += (s, e) => _btnThemeToggle.BackColor = ThemeManager.SidebarButtonHover;
            _btnThemeToggle.MouseLeave += (s, e) => _btnThemeToggle.BackColor = Color.Transparent;
            _btnThemeToggle.Click += (s, e) => ThemeToggled?.Invoke(this, EventArgs.Empty);

            // User profile avatar section
            var avatarPanel = new Panel
            {
                Location = new Point(820, 20),
                Size = new Size(100, 32),
                BackColor = Color.Transparent
            };
            var avatarLabel = new Label
            {
                Text = SessionManager.FullName[0].ToString(),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            avatarLabel.Click += (s, e) => MessageBox.Show($"Welcome back, {SessionManager.FullName}!", "User Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            avatarPanel.Controls.Add(avatarLabel);

            _panel.Controls.Add(_lblTitle);
            _panel.Controls.Add(_lblUser);
            _panel.Controls.Add(avatarPanel);
            _panel.Controls.Add(_btnThemeToggle);
            Controls.Add(_panel);
        }

        public void ApplyTheme()
        {
            _panel.BackColor = ThemeManager.Sidebar;
            _lblTitle.ForeColor = Color.White;
            _lblUser.ForeColor = Color.White;
            _btnThemeToggle.BackColor = Color.Transparent;
            _btnThemeToggle.ForeColor = Color.White;
        }
    }
}