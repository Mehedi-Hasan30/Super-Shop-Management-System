using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Reports;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class DashboardForm : Form
    {
        private readonly DashboardService _dashboardService = new DashboardService();
        private readonly AuthService _authService = new AuthService();
        private readonly NotificationService _notificationService = new NotificationService();
        private readonly string _userRole;

        private Label _lblDailySales;
        private Label _lblMonthlySales;
        private Label _lblTotalProducts;
        private Label _lblTotalCategories;
        private Label _lblLowStock;
        private Label _lblTotalCustomers;
        private Label _lblTotalSuppliers;
        private Label _lblTotalEmployees;
        private Label _lblAttendanceSummary;
        private Label _lblNotifications;

        private FlowLayoutPanel _notificationPanel;
        private static bool _popupShownOnce;

        public DashboardForm(string userRole)
        {
            _userRole = userRole;
            InitializeComponent();
            Load += async (_, __) => await LoadDashboardMetricsAsync();
        }

        private void InitializeComponent()
        {
            Text = "Dashboard - Super Shop Management System";
            WindowState = FormWindowState.Maximized;
            ThemeManager.ApplyFormTheme(this);

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = ThemeManager.Sidebar };

            Panel brandPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = ThemeManager.Sidebar };
            Label brandIcon = IconHelper.CreateIconLabel(IconHelper.Glyphs.Products, 20F, Color.White);
            brandIcon.Location = new Point(20, 20);
            Label brandText = new Label
            {
                Text = "Super Shop",
                ForeColor = Color.White,
                Font = ThemeManager.FontH2,
                AutoSize = true,
                Location = new Point(52, 20)
            };
            Panel brandDivider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ThemeManager.SidebarButton };
            brandPanel.Controls.Add(brandIcon);
            brandPanel.Controls.Add(brandText);
            brandPanel.Controls.Add(brandDivider);

            int top = 16;
            ToolTip toolTip = new ToolTip();
            Button btnCategory = CreateSidebarButton(IconHelper.Glyphs.Inventory, "Category Management", top); top += 44;
            Button btnProduct = CreateSidebarButton(IconHelper.Glyphs.Products, "Product Management", top); top += 44;
            Button btnPosSales = CreateSidebarButton(IconHelper.Glyphs.Sales, "POS Sales", top); top += 44;
            Button btnCustomer = CreateSidebarButton(IconHelper.Glyphs.Customers, "Customer Management", top); top += 44;
            Button btnCustomerSalesHistory = CreateSidebarButton(IconHelper.Glyphs.Reports, "Customer Sales History", top); top += 44;
            Button btnReports = CreateSidebarButton(IconHelper.Glyphs.Reports, "Reports & Analytics", top); top += 44;
            Button btnSettings = CreateSidebarButton(IconHelper.Glyphs.Settings, "Settings", top); top += 44;
            Button btnBackupRestore = CreateSidebarButton(IconHelper.Glyphs.Backup, "Backup & Restore", top); top += 44;
            Button btnSupplier = CreateSidebarButton(IconHelper.Glyphs.Suppliers, "Supplier Management", top); top += 44;
            Button btnEmployee = CreateSidebarButton(IconHelper.Glyphs.Employees, "Employee Management", top); top += 44;
            Button btnAttendance = CreateSidebarButton(IconHelper.Glyphs.Attendance, "Attendance", top); top += 44;
            Button btnAttendanceReport = CreateSidebarButton(IconHelper.Glyphs.Attendance, "Attendance Report", top); top += 44;
            Button btnAuditLogs = CreateSidebarButton(IconHelper.Glyphs.AuditLog, "Audit Logs", top); top += 44;
            Button btnSmokeTest = CreateSidebarButton(IconHelper.Glyphs.Success, "QA Smoke Test", top); top += 44;
            Button btnRefresh = CreateSidebarButton(IconHelper.Glyphs.Refresh, "Refresh Metrics", top); top += 44;
            Button btnLogout = CreateSidebarButton(IconHelper.Glyphs.Logout, "Logout", top);

            toolTip.SetToolTip(btnSettings, "Theme and preferences");
            toolTip.SetToolTip(btnBackupRestore, "Admin-only database backup/restore");

            btnCategory.Click += (_, __) => OpenForm(new CategoryForm());
            btnProduct.Click += (_, __) => OpenForm(new ProductForm());
            btnPosSales.Click += (_, __) => OpenForm(new SalesForm());
            btnCustomer.Click += (_, __) => OpenForm(new CustomerForm());
            btnCustomerSalesHistory.Click += (_, __) => OpenForm(new CustomerSalesHistoryForm());
            btnReports.Click += (_, __) => OpenForm(new ReportsForm());
            btnSettings.Click += (_, __) => OpenForm(new SettingsForm());
            btnBackupRestore.Click += (_, __) => OpenForm(new BackupRestoreForm());
            btnSupplier.Click += (_, __) => OpenForm(new SupplierForm());
            btnEmployee.Click += (_, __) => OpenForm(new EmployeeForm());
            btnAttendance.Click += (_, __) => OpenForm(new AttendanceForm());
            btnAttendanceReport.Click += (_, __) => OpenForm(new AttendanceReportForm());
            btnAuditLogs.Click += (_, __) => OpenForm(new AuditLogViewerForm());
            btnSmokeTest.Click += (_, __) => OpenForm(new SmokeTestReportForm());
            btnRefresh.Click += async (_, __) => await LoadDashboardMetricsAsync();
            btnLogout.Click += BtnLogout_Click;

            if (!string.Equals(_userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                DisableSidebarButton(btnCategory);
                DisableSidebarButton(btnEmployee);
                DisableSidebarButton(btnAuditLogs);
                DisableSidebarButton(btnSmokeTest);
                DisableSidebarButton(btnBackupRestore);
            }

            Panel navScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemeManager.Sidebar };
            navScroll.Controls.Add(btnCategory);
            navScroll.Controls.Add(btnProduct);
            navScroll.Controls.Add(btnPosSales);
            navScroll.Controls.Add(btnCustomer);
            navScroll.Controls.Add(btnCustomerSalesHistory);
            navScroll.Controls.Add(btnReports);
            navScroll.Controls.Add(btnSettings);
            navScroll.Controls.Add(btnBackupRestore);
            navScroll.Controls.Add(btnSupplier);
            navScroll.Controls.Add(btnEmployee);
            navScroll.Controls.Add(btnAttendance);
            navScroll.Controls.Add(btnAttendanceReport);
            navScroll.Controls.Add(btnAuditLogs);
            navScroll.Controls.Add(btnSmokeTest);
            navScroll.Controls.Add(btnRefresh);
            navScroll.Controls.Add(btnLogout);

            sidebar.Controls.Add(navScroll);
            sidebar.Controls.Add(brandPanel);

            Panel content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28), AutoScroll = true };
            Label welcomeLabel = new Label
            {
                Text = $"Welcome back, {SessionManager.FullName}",
                Font = ThemeManager.FontH1,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            Label roleLabel = new Label
            {
                Text = $"{SessionManager.Role} - Here's what's happening in your store today",
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText,
                AutoSize = true,
                Location = new Point(10, welcomeLabel.Bottom + 2)
            };
            content.Controls.Add(welcomeLabel);
            content.Controls.Add(roleLabel);

            FlowLayoutPanel cardContainer = new FlowLayoutPanel
            {
                Location = new Point(6, roleLabel.Bottom + 20),
                Size = new Size(1350, 260),
                WrapContents = true,
                AutoScroll = true
            };

            _lblDailySales = CreateStatCard(cardContainer, "Daily Sales", IconHelper.Glyphs.Sales, ThemeManager.Success);
            _lblMonthlySales = CreateStatCard(cardContainer, "Monthly Sales", IconHelper.Glyphs.Reports, ThemeManager.Primary);
            _lblTotalProducts = CreateStatCard(cardContainer, "Total Products", IconHelper.Glyphs.Products, ThemeManager.Info);
            _lblTotalCategories = CreateStatCard(cardContainer, "Total Categories", IconHelper.Glyphs.Inventory, ThemeManager.Accent);
            _lblLowStock = CreateStatCard(cardContainer, "Low Stock Alerts", IconHelper.Glyphs.Warning, ThemeManager.Warning);
            _lblTotalCustomers = CreateStatCard(cardContainer, "Total Customers", IconHelper.Glyphs.Customers, ThemeManager.Primary);
            _lblTotalSuppliers = CreateStatCard(cardContainer, "Total Suppliers", IconHelper.Glyphs.Suppliers, ThemeManager.Accent);
            _lblTotalEmployees = CreateStatCard(cardContainer, "Total Employees", IconHelper.Glyphs.Employees, ThemeManager.Primary);
            _lblAttendanceSummary = CreateStatCard(cardContainer, "Attendance Summary", IconHelper.Glyphs.Attendance, ThemeManager.Info);
            _lblNotifications = CreateStatCard(cardContainer, "Notifications", IconHelper.Glyphs.Alert, ThemeManager.Danger);

            content.Controls.Add(cardContainer);

            _notificationPanel = new FlowLayoutPanel
            {
                Location = new Point(6, cardContainer.Bottom + 10),
                Size = new Size(1350, 220),
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            content.Controls.Add(_notificationPanel);
            Controls.Add(content);
            Controls.Add(sidebar);
        }

        private static void DisableSidebarButton(Button button)
        {
            button.Enabled = false;
            button.BackColor = ThemeManager.SidebarButtonHover;
            button.ForeColor = Color.FromArgb(140, 140, 140);
        }

        private static Button CreateSidebarButton(string glyph, string text, int top)
        {
            Button button = new Button
            {
                Text = string.Empty,
                Width = 208,
                Height = 38,
                Location = new Point(16, top),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Sidebar,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            UIStyleKit.ApplyRoundedRegion(button, 6);

            Font iconFont = IconHelper.GlyphFont(13F);
            string iconGlyph = IconHelper.GlyphOrFallback(glyph, "-");

            button.Paint += (s, e) =>
            {
                TextRenderer.DrawText(e.Graphics, iconGlyph, iconFont,
                    new Rectangle(14, 0, 24, button.Height), button.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(e.Graphics, text, ThemeManager.FontBody,
                    new Rectangle(44, 0, button.Width - 50, button.Height), button.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
            };

            Color normal = ThemeManager.Sidebar;
            Color hover = ThemeManager.SidebarButtonHover;
            button.MouseEnter += (s, e) => { if (button.Enabled) { button.BackColor = hover; button.Invalidate(); } };
            button.MouseLeave += (s, e) => { if (button.Enabled) { button.BackColor = normal; button.Invalidate(); } };

            return button;
        }

        private static Label CreateStatCard(FlowLayoutPanel container, string title, string glyph, Color accentColor)
        {
            RoundedPanel card = UIStyleKit.CreateStatCard(title, "0", glyph, accentColor, 240, 110);
            card.Margin = new Padding(10);
            container.Controls.Add(card);

            return (Label)card.Controls[1];
        }

        private async Task LoadDashboardMetricsAsync()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                UseWaitCursor = true;
                Dictionary<string, string> metrics = await _dashboardService.GetDashboardMetricsAsync();
                _lblDailySales.Text = metrics["DailySales"];
                _lblMonthlySales.Text = metrics["MonthlySales"];
                _lblTotalProducts.Text = metrics["TotalProducts"];
                _lblTotalCategories.Text = metrics["TotalCategories"];
                _lblLowStock.Text = metrics["LowStock"];
                _lblTotalCustomers.Text = metrics["TotalCustomers"];
                _lblTotalSuppliers.Text = metrics["TotalSuppliers"];
                _lblTotalEmployees.Text = metrics["TotalEmployees"];
                _lblAttendanceSummary.Text = metrics["AttendanceSummary"];

                await LoadNotificationsUiAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("LoadDashboardMetricsAsync", ex);
                MessageBox.Show($"Unable to load dashboard: {ex.Message}", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private async Task LoadNotificationsUiAsync()
        {
            _notificationPanel.Controls.Clear();

            List<NotificationItem> notifications = await _notificationService.GetDashboardNotificationsAsync(8);
            int warningCount = 0;
            int errorCount = 0;
            foreach (NotificationItem n in notifications)
            {
                if (n.Severity == NotificationSeverity.Warning) warningCount++;
                if (n.Severity == NotificationSeverity.Error) errorCount++;
            }

            _lblNotifications.Text = (warningCount + errorCount).ToString();

            foreach (NotificationItem n in notifications)
            {
                Color back;
                Color fore;
                if (n.Severity == NotificationSeverity.Error)
                {
                    back = Color.FromArgb(252, 228, 236);
                    fore = Color.DarkRed;
                }
                else if (n.Severity == NotificationSeverity.Warning)
                {
                    back = Color.FromArgb(255, 243, 205);
                    fore = Color.FromArgb(160, 98, 0);
                }
                else
                {
                    back = Color.FromArgb(220, 235, 245);
                    fore = Color.FromArgb(52, 73, 94);
                }

                Label item = new Label
                {
                    Text = $"{n.Title}: {n.Message}",
                    AutoSize = false,
                    Width = 1320,
                    Height = 50,
                    BackColor = back,
                    ForeColor = fore,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(10, 10, 10, 10)
                };
                _notificationPanel.Controls.Add(item);
                item.Click += (_, __) => OpenForm(new NotificationCenterForm());
            }

            if (!_popupShownOnce)
            {
                bool shouldPopup = errorCount > 0 || warningCount > 0;
                if (shouldPopup)
                {
                    string msg = $"Notifications found: Errors={errorCount}, Warnings={warningCount}.\nOpen Notification Center for details.";
                    _popupShownOnce = true;
                    MessageBox.Show(msg, "Notifications", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private static void OpenForm(Form form)
        {
            using (form)
            {
                form.ShowDialog();
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            _authService.Logout();
            Close();
        }
    }
}
