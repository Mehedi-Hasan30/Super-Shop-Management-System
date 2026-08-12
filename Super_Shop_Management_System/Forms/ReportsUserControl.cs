using System;
using System.Data;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

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
            // Premium header panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Reports, 32F, ThemeManager.Primary);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            var title = new Label
            {
                Text = "Reports & Analytics",
                Font = ThemeManager.FontH2,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(60, 25)
            };
            headerPanel.Controls.Add(title);

            var separator = new Panel
            {
                Location = new Point(60, 55),
                Size = new Size(150, 1),
                BackColor = ThemeManager.BorderColor
            };
            headerPanel.Controls.Add(separator);

            // Report filter panel card
            var filterCard = new RoundedPanel
            {
                Location = new Point(20, 90),
                Size = new Size(520, 180),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(16)
            };

            _cmbReportType = new ComboBox
            {
                Location = new Point(filterCard.Padding.Left, filterCard.Padding.Top),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                ForeColor = ThemeManager.MutedText
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

            _dtpFromDate = new DateTimePicker
            {
                Location = new Point(filterCard.Padding.Left, _cmbReportType.Bottom + 16),
                Size = new Size(240, 30),
                Format = DateTimePickerFormat.Short,
                ForeColor = ThemeManager.MutedText
            };

            var lblFrom = new Label
            {
                Text = "From",
                Location = new Point(filterCard.Padding.Left, _cmbReportType.Bottom + 5),
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText
            };

            _dtpToDate = new DateTimePicker
            {
                Location = new Point(filterCard.Padding.Left + 280, _cmbReportType.Bottom + 16),
                Size = new Size(240, 30),
                Format = DateTimePickerFormat.Short,
                ForeColor = ThemeManager.MutedText
            };

            var lblTo = new Label
            {
                Text = "To",
                Location = new Point(filterCard.Padding.Left + 280, _cmbReportType.Bottom + 5),
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText
            };

            _cmbPaymentStatus = new ComboBox
            {
                Location = new Point(filterCard.Padding.Left, _dtpToDate.Bottom + 20),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                ForeColor = ThemeManager.MutedText
            };

            var lblPaymentStatus = new Label
            {
                Text = "Payment Status",
                Location = new Point(filterCard.Padding.Left, _dtpToDate.Bottom + 16),
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText
            };

            var btnLoad = UIStyleKit.CreateButton(ButtonStyle.Primary, "Load Report", 120, 36);
            btnLoad.Location = new Point(filterCard.Padding.Left, _cmbPaymentStatus.Bottom + 20);
            btnLoad.Click += BtnLoad_Click;
            filterCard.Controls.Add(_cmbReportType);
            filterCard.Controls.Add(_dtpFromDate);
            filterCard.Controls.Add(lblFrom);
            filterCard.Controls.Add(_dtpToDate);
            filterCard.Controls.Add(lblTo);
            filterCard.Controls.Add(_cmbPaymentStatus);
            filterCard.Controls.Add(lblPaymentStatus);
            filterCard.Controls.Add(btnLoad);

            // Stats cards row
            var statsPanel = new Panel
            {
                Location = new Point(20, 280),
                Size = new Size(520, 50),
                BackColor = Color.Transparent
            };

            var revenueCard = UIStyleKit.CreateStatCard("Revenue", "0.00", IconHelper.Glyphs.Info, ThemeManager.Primary, 180, 40);
            revenueCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(revenueCard);

            var profitCard = UIStyleKit.CreateStatCard("Profit", "0.00", IconHelper.Glyphs.Success, ThemeManager.Success, 180, 40);
            profitCard.Location = new Point(210, 10);
            statsPanel.Controls.Add(profitCard);

            // Grid area
            _grid = new DataGridView
            {
                Location = new Point(20, 340),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            DataGridStyler.ApplyModernStyle(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReportName", HeaderText = "Report", Width = 200 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Period", HeaderText = "Period", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalSales", HeaderText = "Total Sales", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalProfit", HeaderText = "Total Profit", Width = 120 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(headerPanel);
            Controls.Add(filterCard);
            Controls.Add(statsPanel);
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

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is null))
            {
                // Selection changed handling can be added here
            }
        }
    }
}