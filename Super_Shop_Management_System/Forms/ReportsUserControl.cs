using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public class ReportsUserControl : UserControl
    {
        private readonly ReportsService _reportsService = new ReportsService();

        private DataGridView _grid;
        private ComboBox _cmbReportType;
        private DateTimePicker _dtpFromDate;
        private DateTimePicker _dtpToDate;
        private ComboBox _cmbPaymentStatus;
        private TextBox _txtSearch;

        public ReportsUserControl()
        {
            Dock = DockStyle.Fill;
            BackColor = ThemeManager.Background;
            Tag = ThemeManager.ThemeExemptTag;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var title = new Label
            {
                Text = "Reports & Analytics",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var panel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(500, 200),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _cmbReportType = new ComboBox
            {
                Location = new Point(20, 40),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbReportType.Items.AddRange(new object[]
            {
                "Daily Sales",
                "Monthly Sales",
                "Product Sales",
                "Customer Purchase",
                "Supplier Purchase",
                "Employee Sales",
                "Pending Sales",
                "Paid Sales",
                "Inventory Stock",
                "Low Stock",
                "Expiry Products",
                "Attendance"
            });

            _dtpFromDate = new DateTimePicker { Location = new Point(20, 90), Width = 200, Format = DateTimePickerFormat.Short };
            _dtpToDate = new DateTimePicker { Location = new Point(240, 90), Width = 200, Format = DateTimePickerFormat.Short };

            _cmbPaymentStatus = new ComboBox
            {
                Location = new Point(20, 130),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentStatus.Items.AddRange(new object[] { "All", "Paid", "Pending" });
            _cmbPaymentStatus.SelectedIndex = 0;

            Button btnLoad = new RoundedButton { Text = "Load Report", Location = new Point(20, 170), Width = 120, CornerRadius = 6 };
            btnLoad.Click += BtnLoad_Click;

            _txtSearch = new TextBox { Location = new Point(160, 170), Width = 180 };

            _grid = new DataGridView
            {
                Location = new Point(20, 250),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

DataGridStyler.ApplyModernStyle(_grid);

            Controls.Add(title);
            Controls.Add(panel);
            panel.Controls.Add(_cmbReportType);
            panel.Controls.Add(new Label { Text = "Report Type", Location = new Point(20, 20), AutoSize = true });
            panel.Controls.Add(_dtpFromDate);
            panel.Controls.Add(new Label { Text = "From", Location = new Point(20, 70), AutoSize = true });
            panel.Controls.Add(_dtpToDate);
            panel.Controls.Add(new Label { Text = "To", Location = new Point(240, 70), AutoSize = true });
            panel.Controls.Add(_cmbPaymentStatus);
            panel.Controls.Add(new Label { Text = "Payment Status", Location = new Point(20, 110), AutoSize = true });
            panel.Controls.Add(btnLoad);
            panel.Controls.Add(_txtSearch);
            Controls.Add(_grid);
        }

        private async void ReportsUserControl_Load(object sender, EventArgs e)
        {
            await LoadReportTypesAsync();
        }

        private async Task LoadReportTypesAsync()
        {
            if (_cmbReportType.Items.Count > 0)
                _cmbReportType.SelectedIndex = 0;
        }

        private async void BtnLoad_Click(object sender, EventArgs e)
        {
            using (var loading = new LoadingIndicator(this, "Loading report..."))
            {
                try
                {
                    ReportType reportType = ReportType.DailySales;
                    switch (_cmbReportType.SelectedItem.ToString())
                    {
                        case "Daily Sales": reportType = ReportType.DailySales; break;
                        case "Monthly Sales": reportType = ReportType.MonthlySales; break;
                        case "Product Sales": reportType = ReportType.ProductSales; break;
                        case "Customer Purchase": reportType = ReportType.CustomerPurchase; break;
                        case "Supplier Purchase": reportType = ReportType.SupplierPurchase; break;
                        case "Employee Sales": reportType = ReportType.EmployeeSales; break;
                        case "Pending Sales": reportType = ReportType.PendingSales; break;
                        case "Paid Sales": reportType = ReportType.PaidSales; break;
                        case "Inventory Stock": reportType = ReportType.InventoryStock; break;
                        case "Low Stock": reportType = ReportType.LowStock; break;
                        case "Expiry Products": reportType = ReportType.ExpiryProducts; break;
                        case "Attendance": reportType = ReportType.Attendance; break;
                    }

                    ReportFilters filters = new ReportFilters
                    {
                        FromDate = _dtpFromDate.Value,
                        ToDate = _dtpToDate.Value,
                        PaymentStatus = _cmbPaymentStatus.SelectedItem?.ToString()
                    };

                    DataTable reportData = await _reportsService.GetReportDataAsync(reportType, filters);
                    _grid.DataSource = reportData;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to load report: {ex.Message}", "Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}