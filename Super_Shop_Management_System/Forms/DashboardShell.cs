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

        private readonly string[] _kpiLabels =
        {
            "DailySales",
            "MonthlySales",
            "TotalProducts",
            "TotalCategories",
            "LowStock",
            "TotalCustomers",
            "TotalSuppliers",
            "TotalEmployees",
            "AttendanceSummary",
            "Notifications"
        };


        public DashboardShell(string userRole)
        {
            Text = "Super Shop Management System";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;

            ThemeManager.ApplyFormTheme(this);

            Color sidebarColor = ThemeColors.Sidebar;
            Color backgroundColor = ThemeColors.Background;
            Color textPrimary = ThemeColors.TextPrimary;
            Color textMuted = ThemeColors.TextMuted;
            Color accentColor = ThemeColors.Primary;


            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = sidebarColor,
                Tag = ThemeManager.ThemeExemptTag
            };


            _sidebarNav = new SidebarNavigation(userRole)
            {
                Dock = DockStyle.Fill
            };

            _sidebar.Controls.Add(_sidebarNav);


            var brandPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = sidebarColor
            };


            var brandIcon = IconHelper.CreateIconLabel(
                IconHelper.Glyphs.Products,
                20F,
                accentColor);

            brandIcon.Location = new Point(20, 20);


            var brandText = new Label
            {
                Text = "Super Shop",
                ForeColor = Color.White,
                Font = ThemeManager.FontH2,
                AutoSize = true,
                Location = new Point(52, 20)
            };


            var brandDivider = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = ThemeManager.SidebarButton
            };


            brandPanel.Controls.Add(brandIcon);
            brandPanel.Controls.Add(brandText);
            brandPanel.Controls.Add(brandDivider);

            _sidebar.Controls.Add(brandPanel);



            _header = new ShellHeader(userRole)
            {
                Dock = DockStyle.Top,
                Height = 64
            };


            _content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28),
                BackColor = backgroundColor,
                AutoScroll = true
            };


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



            var kpiPanel = new Panel
            {
                Location = new Point(10, roleLabel.Bottom + 14),
                Size = new Size(1280, 100),
                BackColor = Color.Transparent
            };


            _content.Controls.Add(kpiPanel);



            AddKpiCard(kpiPanel, "Daily Sales", IconHelper.Glyphs.Sales, ThemeManager.Success, 10);
            AddKpiCard(kpiPanel, "Monthly Sales", IconHelper.Glyphs.Sales, ThemeManager.Primary, 120);
            AddKpiCard(kpiPanel, "Total Products", IconHelper.Glyphs.Inventory, ThemeManager.Info, 230);
            AddKpiCard(kpiPanel, "Total Categories", IconHelper.Glyphs.Inventory, ThemeManager.Primary, 340);
            AddKpiCard(kpiPanel, "Low Stock", IconHelper.Glyphs.Alert, ThemeManager.Warning, 450);
            AddKpiCard(kpiPanel, "Total Customers", IconHelper.Glyphs.Customers, ThemeManager.Primary, 560);
            AddKpiCard(kpiPanel, "Total Suppliers", IconHelper.Glyphs.Suppliers, ThemeManager.Success, 670);
            AddKpiCard(kpiPanel, "Total Employees", IconHelper.Glyphs.Employees, ThemeManager.Info, 780);
            AddKpiCard(kpiPanel, "Attendance", IconHelper.Glyphs.Attendance, ThemeManager.Warning, 890);
            AddKpiCard(kpiPanel, "Notifications", IconHelper.Glyphs.Info, ThemeManager.Primary, 1000);



            _moduleContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor,
                AutoScroll = true,
                Padding = new Padding(20)
            };


            _content.Controls.Add(_moduleContainer);


            Controls.Add(_content);
            Controls.Add(_sidebar);
            Controls.Add(_header);


            _sidebarNav.NavButtonClicked += OnNavButtonClicked;
            _header.ThemeToggled += OnThemeToggled;
        }



        private void AddKpiCard(
            Panel parent,
            string title,
            string icon,
            Color color,
            int x)
        {
            var card = UIStyleKit.CreateStatCard(
                title,
                "0",
                icon,
                color,
                110,
                80);

            card.Location = new Point(x, 10);
            parent.Controls.Add(card);
        }



        private void OnNavButtonClicked(object sender, string moduleName)
        {
            Transition.AnimateWidth(
                _moduleContainer,
                _moduleContainer.Width,
                _moduleContainer.Width,
                200);

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

            if (_sidebar != null)
            {
                _sidebar.Width = 240;
            }
        }
    }
}