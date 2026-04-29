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

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 235, BackColor = ThemeManager.Sidebar };
            sidebar.Controls.Add(new Label
            {
                Text = "Super Shop",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 24)
            });

            int top = 75;
            ToolTip toolTip = new ToolTip();
            Button btnCategory = CreateSidebarButton("Category Management", top); top += 42;
            Button btnProduct = CreateSidebarButton("Product Management", top); top += 42;
            Button btnPosSales = CreateSidebarButton("POS Sales", top); top += 42;
            Button btnCustomer = CreateSidebarButton("Customer Management", top); top += 42;
            Button btnCustomerSalesHistory = CreateSidebarButton("Customer Sales History", top); top += 42;
            Button btnReports = CreateSidebarButton("Reports & Analytics", top); top += 42;
            Button btnSettings = CreateSidebarButton("Settings", top); top += 42;
            Button btnBackupRestore = CreateSidebarButton("Backup & Restore", top); top += 42;
            Button btnSupplier = CreateSidebarButton("Supplier Management", top); top += 42;
            Button btnEmployee = CreateSidebarButton("Employee Management", top); top += 42;
            Button btnAttendance = CreateSidebarButton("Attendance", top); top += 42;
            Button btnAttendanceReport = CreateSidebarButton("Attendance Report", top); top += 42;
            Button btnAuditLogs = CreateSidebarButton("Audit Logs", top); top += 42;
            Button btnSmokeTest = CreateSidebarButton("QA Smoke Test", top); top += 42;
            Button btnRefresh = CreateSidebarButton("Refresh Metrics", top); top += 42;
            Button btnLogout = CreateSidebarButton("Logout", top);

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
                btnCategory.Enabled = false;
                btnCategory.BackColor = Color.FromArgb(80, 80, 80);
                btnEmployee.Enabled = false;
                btnEmployee.BackColor = Color.FromArgb(80, 80, 80);
                btnAuditLogs.Enabled = false;
                btnAuditLogs.BackColor = Color.FromArgb(80, 80, 80);
                btnSmokeTest.Enabled = false;
                btnSmokeTest.BackColor = Color.FromArgb(80, 80, 80);
                btnBackupRestore.Enabled = false;
                btnBackupRestore.BackColor = Color.FromArgb(80, 80, 80);
            }

            sidebar.Controls.Add(btnCategory);
            sidebar.Controls.Add(btnProduct);
            sidebar.Controls.Add(btnPosSales);
            sidebar.Controls.Add(btnCustomer);
            sidebar.Controls.Add(btnCustomerSalesHistory);
            sidebar.Controls.Add(btnReports);
            sidebar.Controls.Add(btnSettings);
            sidebar.Controls.Add(btnBackupRestore);
            sidebar.Controls.Add(btnSupplier);
            sidebar.Controls.Add(btnEmployee);
            sidebar.Controls.Add(btnAttendance);
            sidebar.Controls.Add(btnAttendanceReport);
            sidebar.Controls.Add(btnAuditLogs);
            sidebar.Controls.Add(btnSmokeTest);
            sidebar.Controls.Add(btnRefresh);
            sidebar.Controls.Add(btnLogout);

            Panel content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            content.Controls.Add(new Label
            {
                Text = $"Welcome, {SessionManager.FullName} ({SessionManager.Role})",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(10, 10)
            });

            FlowLayoutPanel cardContainer = new FlowLayoutPanel
            {
                Location = new Point(10, 48),
                Size = new Size(1350, 400),
                WrapContents = true,
                AutoScroll = true
            };

            _lblDailySales = CreateCard(cardContainer, "Daily Sales");
            _lblMonthlySales = CreateCard(cardContainer, "Monthly Sales");
            _lblTotalProducts = CreateCard(cardContainer, "Total Products");
            _lblTotalCategories = CreateCard(cardContainer, "Total Categories");
            _lblLowStock = CreateCard(cardContainer, "Low Stock Alerts");
            _lblTotalCustomers = CreateCard(cardContainer, "Total Customers");
            _lblTotalSuppliers = CreateCard(cardContainer, "Total Suppliers");
            _lblTotalEmployees = CreateCard(cardContainer, "Total Employees");
            _lblAttendanceSummary = CreateCard(cardContainer, "Attendance Summary");
            _lblNotifications = CreateCard(cardContainer, "Notifications");

            content.Controls.Add(cardContainer);

            _notificationPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 450),
                Size = new Size(1350, 170),
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            content.Controls.Add(_notificationPanel);
            Controls.Add(content);
            Controls.Add(sidebar);
        }

        private static Button CreateSidebarButton(string text, int top)
        {
            Button button = new Button { Text = text, Width = 190, Height = 34, Location = new Point(20, top) };
            FormDesignHelper.ApplySidebarButtonStyle(button);
            return button;
        }

        private static Label CreateCard(FlowLayoutPanel container, string title)
        {
            Panel panel = new Panel { Width = 230, Height = 120, BackColor = Color.White, Margin = new Padding(14), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(new Label { Text = title, AutoSize = true, ForeColor = Color.Gray, Location = new Point(15, 15) });
            Label value = FormDesignHelper.CreateCardLabel(title);
            value.Location = new Point(15, 50);
            panel.Controls.Add(value);
            container.Controls.Add(panel);
            return value;
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
