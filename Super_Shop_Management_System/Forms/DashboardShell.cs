using System;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Animations;

namespace Super_Shop_Management_System.Forms
{
    public class DashboardShell : Form
    {
        private Panel _sidebar;
        private Panel _content;
        private ShellHeader _header;
        private SidebarNavigation _sidebarNav;
        private Panel _moduleContainer;
        private ValueAnimator _kpiAnimator;
        private readonly int[] _kpiValues = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly string[] _kpiLabels = { "DailySales", "MonthlySales", "TotalProducts", "TotalCategories", "LowStock", "TotalCustomers", "TotalSuppliers", "TotalEmployees", "AttendanceSummary", "Notifications" };

        public DashboardShell(string userRole)
        {
            Text = "Super Shop Management System";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;

            ThemeManager.ApplyFormTheme(this);

            // Use ThemeColors from the design system
            Color sidebarColor = ThemeColors.Sidebar;
            Color backgroundColor = ThemeColors.Background;
            Color cardColor = ThemeColors.Card;
            Color textPrimary = ThemeColors.TextPrimary;
            Color textMuted = ThemeColors.TextMuted;
            Color accentColor = ThemeColors.Primary;

            _sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = sidebarColor, Tag = ThemeManager.ThemeExemptTag };
            _sidebar.Paint += (_, __) => { };

            _sidebarNav = new SidebarNavigation(userRole);
            _sidebarNav.Dock = DockStyle.Fill;
            _sidebar.Controls.Add(_sidebarNav);

            var brandPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = sidebarColor };
            var brandIcon = IconHelper.CreateIconLabel(IconHelper.Glyphs.Products, 20F, accentColor);
            brandIcon.Location = new Point(20, 20);
            var brandText = new Label
            {
                Text = "Super Shop",
                ForeColor = Color.White,
                Font = ThemeManager.FontH2,
                AutoSize = true,
                Location = new Point(52, 20)
            };
            var brandDivider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ThemeManager.SidebarButton };
            brandPanel.Controls.Add(brandIcon);
            brandPanel.Controls.Add(brandText);
            brandPanel.Controls.Add(brandDivider);
            _sidebar.Controls.Add(brandPanel);

            _header = new ShellHeader(userRole);
            _header.Dock = DockStyle.Top;
            _header.Height = 64;

            _content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28), BackColor = backgroundColor, AutoScroll = true };

            var welcomeLabel = new Label
            {
                Text = $"Welcome back, {SessionManager.FullName}",
                Font = ThemeManager.FontH1,
                ForeColor = textPrimary,
                AutoSize = true
            };
            _content.Controls.Add(welcomeLabel);

            var roleLabel = new Label
            {
                Text = $"{SessionManager.Role} - Here's what's happening in your store today",
                Font = ThemeManager.FontBody,
                ForeColor = textMuted,
                AutoSize = true,
                Location = new Point(10, welcomeLabel.Bottom + 2)
            };
            _content.Controls.Add(roleLabel);

            // KPI premium cards panel
            var kpiPanel = new Panel
            {
                Location = new Point(10, roleLabel.Bottom + 14),
                Size = new Size(1300, 100),
                BackColor = Color.Transparent
            };
            _content.Controls.Add(kpiPanel);

            // Daily Sales card
            var card1 = UIStyleKit.CreateStatCard("Daily Sales", "0", IconHelper.Glyphs.Sales, ThemeManager.Success, 110, 80);
            card1.Location = new Point(10, 10);
            kpiPanel.Controls.Add(card1);

            // Monthly Sales card
            var card2 = UIStyleKit.CreateStatCard("Monthly Sales", "0", IconHelper.Glyphs.Sales, ThemeManager.Primary, 110, 80);
            card2.Location = new Point(120, 10);
            kpiPanel.Controls.Add(card2);

            // Total Products card
            var card3 = UIStyleKit.CreateStatCard("Total Products", "0", IconHelper.Glyphs.Inventory, ThemeManager.Info, 110, 80);
            card3.Location = new Point(230, 10);
            kpiPanel.Controls.Add(card3);

            // Total Categories card
            var card4 = UIStyleKit.CreateStatCard("Total Categories", "0", IconHelper.Glyphs.Inventory, ThemeManager.Primary, 110, 80);
            card4.Location = new Point(340, 10);
            kpiPanel.Controls.Add(card4);

            // Low Stock card
            var card5 = UIStyleKit.CreateStatCard("Low Stock", "0", IconHelper.Glyphs.Alert, ThemeManager.Warning, 110, 80);
            card5.Location = new Point(450, 10);
            kpiPanel.Controls.Add(card5);

            // Total Customers card
            var card6 = UIStyleKit.CreateStatCard("Total Customers", "0", IconHelper.Glyphs.Customers, ThemeManager.Primary, 110, 80);
            card6.Location = new Point(560, 10);
            kpiPanel.Controls.Add(card6);

            // Total Suppliers card
            var card7 = UIStyleKit.CreateStatCard("Total Suppliers", "0", IconHelper.Glyphs.Suppliers, ThemeManager.Success, 110, 80);
            card7.Location = new Point(670, 10);
            kpiPanel.Controls.Add(card7);

            // Total Employees card
            var card8 = UIStyleKit.CreateStatCard("Total Employees", "0", IconHelper.Glyphs.Employees, ThemeManager.Info, 110, 80);
            card8.Location = new Point(780, 10);
            kpiPanel.Controls.Add(card8);

            // Attendance card
            var card9 = UIStyleKit.CreateStatCard("Attendance", "0", IconHelper.Glyphs.Attendance, ThemeManager.Warning, 110, 80);
            card9.Location = new Point(890, 10);
            kpiPanel.Controls.Add(card9);

            // Notifications card
            var card10 = UIStyleKit.CreateStatCard("Notifications", "0", IconHelper.Glyphs.Info, ThemeManager.Primary, 110, 80);
            card10.Location = new Point(1000, 10);
            kpiPanel.Controls.Add(card10);

            _moduleContainer = new Panel { Dock = DockStyle.Fill, BackColor = backgroundColor, AutoScroll = true, Padding = new Padding(20) };
            _content.Controls.Add(_moduleContainer);

            Controls.Add(_content);
            Controls.Add(_sidebar);
            Controls.Add(_header);

            _sidebarNav.NavButtonClicked += OnNavButtonClicked;
            _header.ThemeToggled += OnThemeToggled;
        }

        private void OnNavButtonClicked(object sender, string moduleName)
        {
            // Smooth transition before loading module
            Transition.AnimateWidth(_moduleContainer, _moduleContainer.Width, _moduleContainer.Width, 200);
            ModuleLoader.LoadModule(_moduleContainer, moduleName);
        }

        private void OnThemeToggled(object sender, EventArgs e)
        {
            ThemeManager.ToggleTheme();
            ApplyThemeToAll();
        }

        private void ApplyThemeToAll()
        {
            ThemeManager.ApplyFormTheme(this);
            _sidebarNav?.ApplyTheme();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _sidebar?.Width = 240;
        }

        // Paint handler for animated KPI cards
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw animated KPI cards background
            using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}