using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class ReportsForm : Form
    {
        private readonly ReportsService _reportsService = new ReportsService();
        private readonly ExportService _exportService = new ExportService();
        private readonly CustomerService _customerService = new CustomerService();
        private readonly EmployeeService _employeeService = new EmployeeService();
        private readonly SupplierService _supplierService = new SupplierService();
        private readonly PosService _posService = new PosService();

        private TabControl _tabs;

        // Reports tab controls
        private ComboBox _cmbReportType;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private ComboBox _cmbTimePeriod;
        private DateTimePicker _dtAnchor;
        private DateTimePicker _dtCustomFrom;
        private DateTimePicker _dtCustomTo;
        private CheckBox _chkFromCustom;
        private CheckBox _chkToCustom;

        private ComboBox _cmbPaymentStatus;
        private ComboBox _cmbCustomer;
        private ComboBox _cmbEmployee;
        private ComboBox _cmbProduct;
        private ComboBox _cmbSupplier;

        private Button _btnLoadReport;
        private DataGridView _gridReport;
        private Label _lblReportSummary;

        private Button _btnExportCsv;
        private Button _btnExportExcel;
        private Button _btnExportPdf;
        private Button _btnPrintPreview;
        private Button _btnDirectPrint;

        private DataTable _currentReportData;
        private string _currentReportTitle = "Report";

        // Analytics tab controls
        private Chart _chartSalesTrend;
        private Chart _chartRevenueTrend;
        private Chart _chartMonthlyGrossProfit;
        private Chart _chartInventoryMovement;
        private Chart _chartPaymentMethod;
        private Chart _chartPendingVsPaid;

        private DataGridView _gridTopProducts;
        private DataGridView _gridBestCustomers;

        // Profit/loss tab controls
        private Label _lblRevenue;
        private Label _lblPurchaseCost;
        private Label _lblGrossProfit;
        private Label _lblNetProfit;
        private Label _lblPendingRevenue;
        private Label _lblVatCollected;

        private Chart _chartProfitMonthly;

        private TimeAggregationType GetAggregationFromUi()
        {
            if (_cmbTimePeriod.SelectedItem == null) return TimeAggregationType.Custom;
            string value = _cmbTimePeriod.SelectedItem.ToString();
            if (string.Equals(value, "Daily", StringComparison.OrdinalIgnoreCase)) return TimeAggregationType.Daily;
            if (string.Equals(value, "Weekly", StringComparison.OrdinalIgnoreCase)) return TimeAggregationType.Weekly;
            if (string.Equals(value, "Monthly", StringComparison.OrdinalIgnoreCase)) return TimeAggregationType.Monthly;
            return TimeAggregationType.Custom;
        }

        private void SyncDatePickersForTimePeriod()
        {
            DateTime anchor = _dtAnchor.Value.Date;

            string value = _cmbTimePeriod.SelectedItem?.ToString() ?? "Custom";
            if (string.Equals(value, "Daily", StringComparison.OrdinalIgnoreCase))
            {
                _dtFrom.Value = anchor;
                _dtTo.Value = anchor;
                _dtCustomFrom.Enabled = false;
                _dtCustomTo.Enabled = false;
            }
            else if (string.Equals(value, "Weekly", StringComparison.OrdinalIgnoreCase))
            {
                // Week starts Monday
                int diff = (7 + (int)anchor.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                DateTime start = anchor.AddDays(-diff);
                DateTime end = start.AddDays(6);
                _dtFrom.Value = start;
                _dtTo.Value = end;
                _dtCustomFrom.Enabled = false;
                _dtCustomTo.Enabled = false;
            }
            else if (string.Equals(value, "Monthly", StringComparison.OrdinalIgnoreCase))
            {
                DateTime start = new DateTime(anchor.Year, anchor.Month, 1);
                DateTime end = start.AddMonths(1).AddDays(-1);
                _dtFrom.Value = start;
                _dtTo.Value = end;
                _dtCustomFrom.Enabled = false;
                _dtCustomTo.Enabled = false;
            }
            else
            {
                // Custom: use optional custom pickers; otherwise keep existing dtFrom/dtTo
                _dtCustomFrom.Enabled = _chkFromCustom.Checked;
                _dtCustomTo.Enabled = _chkToCustom.Checked;

                if (_chkFromCustom.Checked) _dtFrom.Value = _dtCustomFrom.Value.Date;
                if (_chkToCustom.Checked) _dtTo.Value = _dtCustomTo.Value.Date;
            }
        }

        public ReportsForm()
        {
            InitializeComponent();
            Load += ReportsForm_Load;
        }

        private void InitializeComponent()
        {
            Text = "Reports + Profit/Loss + Analytics";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1400, 820);
            WindowState = FormWindowState.Maximized;
            ThemeManager.ApplyFormTheme(this);

            _tabs = new TabControl { Dock = DockStyle.Fill };

            TabPage tabReports = new TabPage("Reports");
            TabPage tabAnalytics = new TabPage("Analytics");
            TabPage tabProfit = new TabPage("Profit/Loss");

            tabReports.BackColor = ThemeManager.Background;
            tabAnalytics.BackColor = ThemeManager.Background;
            tabProfit.BackColor = ThemeManager.Background;

            tabReports.Padding = new Padding(15);
            tabAnalytics.Padding = new Padding(15);
            tabProfit.Padding = new Padding(15);

            _cmbReportType = new ComboBox { Location = new Point(15, 15), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbReportType.Items.AddRange(new object[]
            {
                "Daily Sales Report",
                "Monthly Sales Report",
                "Product Sales Report",
                "Customer Purchase Report",
                "Supplier Purchase Report",
                "Employee Sales Report",
                "Pending Sales Report",
                "Paid Sales Report",
                "Inventory Stock Report",
                "Low Stock Report",
                "Expiry Product Report",
                "Attendance Report"
            });
            _cmbReportType.SelectedIndex = 0;

            _dtAnchor = new DateTimePicker { Location = new Point(385, 15), Width = 150, Format = DateTimePickerFormat.Short };
            _cmbTimePeriod = new ComboBox
            {
                Location = new Point(540, 15),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbTimePeriod.Items.AddRange(new object[] { "Daily", "Weekly", "Monthly", "Custom" });
            _cmbTimePeriod.SelectedIndex = 2;

            _dtFrom = new DateTimePicker { Location = new Point(690, 15), Width = 140, Format = DateTimePickerFormat.Short };
            _dtTo = new DateTimePicker { Location = new Point(835, 15), Width = 140, Format = DateTimePickerFormat.Short };

            Label lblAnchor = new Label { Text = "Anchor", Location = new Point(385, -2), AutoSize = true };
            Label lblPeriod = new Label { Text = "Period", Location = new Point(540, -2), AutoSize = true };
            Label lblFrom = new Label { Text = "From", Location = new Point(690, -2), AutoSize = true };
            Label lblTo = new Label { Text = "To", Location = new Point(835, -2), AutoSize = true };

            _chkFromCustom = new CheckBox { Text = "Custom From", Location = new Point(690, 45), AutoSize = true, Checked = false };
            _dtCustomFrom = new DateTimePicker { Location = new Point(810, 40), Width = 140, Format = DateTimePickerFormat.Short, Enabled = false };
            _chkToCustom = new CheckBox { Text = "Custom To", Location = new Point(955, 45), AutoSize = true, Checked = false };
            _dtCustomTo = new DateTimePicker { Location = new Point(1085, 40), Width = 140, Format = DateTimePickerFormat.Short, Enabled = false };

            _cmbTimePeriod.SelectedIndexChanged += (_, __) =>
            {
                SyncDatePickersForTimePeriod();
            };
            _dtAnchor.ValueChanged += (_, __) =>
            {
                SyncDatePickersForTimePeriod();
            };
            _chkFromCustom.CheckedChanged += (_, __) => SyncDatePickersForTimePeriod();
            _chkToCustom.CheckedChanged += (_, __) => SyncDatePickersForTimePeriod();
            _dtCustomFrom.ValueChanged += (_, __) => SyncDatePickersForTimePeriod();
            _dtCustomTo.ValueChanged += (_, __) => SyncDatePickersForTimePeriod();

            int filterTop = 95;
            _cmbPaymentStatus = new ComboBox { Location = new Point(15, filterTop), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbPaymentStatus.Items.AddRange(new object[] { "All", "Paid", "Pending" });
            _cmbPaymentStatus.SelectedIndex = 0;

            _cmbCustomer = new ComboBox { Location = new Point(245, filterTop), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbEmployee = new ComboBox { Location = new Point(475, filterTop), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbProduct = new ComboBox { Location = new Point(705, filterTop), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbSupplier = new ComboBox { Location = new Point(935, filterTop), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblPayment = new Label { Text = "Payment Status", Location = new Point(15, filterTop - 18), AutoSize = true };
            Label lblCust = new Label { Text = "Customer", Location = new Point(245, filterTop - 18), AutoSize = true };
            Label lblEmp = new Label { Text = "Employee", Location = new Point(475, filterTop - 18), AutoSize = true };
            Label lblProd = new Label { Text = "Product", Location = new Point(705, filterTop - 18), AutoSize = true };
            Label lblSupp = new Label { Text = "Supplier", Location = new Point(935, filterTop - 18), AutoSize = true };

            _btnLoadReport = new Button { Text = "Load Report", Location = new Point(1165, 90), Width = 150, Height = 45 };
            FormDesignHelper.ApplyCrudButtonStyle(_btnLoadReport, ThemeManager.Primary);
            _btnLoadReport.Click += async (_, __) => await LoadReportAndDashboardAsync();

            _gridReport = new DataGridView
            {
                Location = new Point(15, 150),
                Width = 1320,
                Height = 520,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_gridReport);

            _lblReportSummary = new Label { Location = new Point(15, 675), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            FlowLayoutPanel exportPanel = new FlowLayoutPanel { Location = new Point(15, 710), Width = 500, Height = 45 };
            _btnExportCsv = new Button { Text = "Export CSV", Width = 105 };
            _btnExportExcel = new Button { Text = "Export Excel (CSV)", Width = 165 };
            _btnExportPdf = new Button { Text = "Export PDF (HTML)", Width = 170 };
            _btnPrintPreview = new Button { Text = "Print Preview", Width = 130 };
            _btnDirectPrint = new Button { Text = "Direct Print", Width = 120 };

            FormDesignHelper.ApplyCrudButtonStyle(_btnExportCsv, Color.FromArgb(39, 174, 96));
            FormDesignHelper.ApplyCrudButtonStyle(_btnExportExcel, Color.FromArgb(39, 174, 96));
            FormDesignHelper.ApplyCrudButtonStyle(_btnExportPdf, Color.FromArgb(155, 89, 182));
            FormDesignHelper.ApplyCrudButtonStyle(_btnPrintPreview, Color.FromArgb(52, 73, 94));
            FormDesignHelper.ApplyCrudButtonStyle(_btnDirectPrint, Color.FromArgb(52, 73, 94));

            _btnExportCsv.Click += (_, __) => ExportCurrentReport("csv");
            _btnExportExcel.Click += (_, __) => ExportCurrentReport("excel");
            _btnExportPdf.Click += (_, __) => ExportCurrentReport("pdf");
            _btnPrintPreview.Click += (_, __) => PrintCurrentReport(true);
            _btnDirectPrint.Click += (_, __) => PrintCurrentReport(false);

            exportPanel.Controls.Add(_btnExportCsv);
            exportPanel.Controls.Add(_btnExportExcel);
            exportPanel.Controls.Add(_btnExportPdf);
            exportPanel.Controls.Add(_btnPrintPreview);
            exportPanel.Controls.Add(_btnDirectPrint);

            tabReports.Controls.Add(_cmbReportType);
            tabReports.Controls.Add(lblAnchor);
            tabReports.Controls.Add(_dtAnchor);
            tabReports.Controls.Add(lblPeriod);
            tabReports.Controls.Add(_cmbTimePeriod);
            tabReports.Controls.Add(lblFrom);
            tabReports.Controls.Add(_dtFrom);
            tabReports.Controls.Add(lblTo);
            tabReports.Controls.Add(_dtTo);

            tabReports.Controls.Add(_chkFromCustom);
            tabReports.Controls.Add(_dtCustomFrom);
            tabReports.Controls.Add(_chkToCustom);
            tabReports.Controls.Add(_dtCustomTo);

            tabReports.Controls.Add(lblPayment);
            tabReports.Controls.Add(lblCust);
            tabReports.Controls.Add(lblEmp);
            tabReports.Controls.Add(lblProd);
            tabReports.Controls.Add(lblSupp);

            tabReports.Controls.Add(_cmbPaymentStatus);
            tabReports.Controls.Add(_cmbCustomer);
            tabReports.Controls.Add(_cmbEmployee);
            tabReports.Controls.Add(_cmbProduct);
            tabReports.Controls.Add(_cmbSupplier);
            tabReports.Controls.Add(_btnLoadReport);

            tabReports.Controls.Add(_gridReport);
            tabReports.Controls.Add(_lblReportSummary);
            tabReports.Controls.Add(exportPanel);

            // Analytics tab
            TableLayoutPanel analyticsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            _chartSalesTrend = CreateChart("Sales Trends", SeriesChartType.Line);
            _chartRevenueTrend = CreateChart("Revenue Trend", SeriesChartType.Line);
            _chartMonthlyGrossProfit = CreateChart("Monthly Gross Profit", SeriesChartType.Column);
            _chartInventoryMovement = CreateChart("Inventory Movement Trend", SeriesChartType.Line);
            _chartPaymentMethod = CreateChart("Payment Method Breakdown", SeriesChartType.Pie);
            _chartPendingVsPaid = CreateChart("Pending vs Paid Comparison", SeriesChartType.Column);

            _gridTopProducts = CreateGrid("Top Selling Products");
            _gridBestCustomers = CreateGrid("Best Customers");

            analyticsLayout.Controls.Add(WrapChart(_chartSalesTrend), 0, 0);
            analyticsLayout.Controls.Add(WrapChart(_chartRevenueTrend), 1, 0);
            analyticsLayout.Controls.Add(WrapChart(_chartMonthlyGrossProfit), 0, 1);
            analyticsLayout.Controls.Add(WrapChart(_chartInventoryMovement), 1, 1);
            analyticsLayout.Controls.Add(WrapChart(_chartPaymentMethod), 0, 2);
            analyticsLayout.Controls.Add(WrapChart(_chartPendingVsPaid), 1, 2);

            // Put top products + best customers under the charts (simple docked panels)
            Panel bottomAnalyticsPanel = new Panel { Dock = DockStyle.Bottom, Height = 200 };
            bottomAnalyticsPanel.Controls.Add(_gridTopProducts);
            bottomAnalyticsPanel.Controls.Add(_gridBestCustomers);
            _gridTopProducts.Location = new Point(0, 0);
            _gridTopProducts.Width = bottomAnalyticsPanel.Width / 2 - 10;
            _gridBestCustomers.Location = new Point(bottomAnalyticsPanel.Width / 2, 0);
            _gridBestCustomers.Width = bottomAnalyticsPanel.Width / 2 - 10;
            _gridTopProducts.Height = 180;
            _gridBestCustomers.Height = 180;
            _gridTopProducts.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            _gridBestCustomers.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            tabAnalytics.Controls.Add(analyticsLayout);
            tabAnalytics.Controls.Add(bottomAnalyticsPanel);

            // Profit/Loss tab
            FlowLayoutPanel profitPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 160 };
            profitPanel.WrapContents = false;
            profitPanel.AutoScroll = true;

            _lblRevenue = CreateMetricLabel("Total Revenue");
            _lblPurchaseCost = CreateMetricLabel("Total Purchase Cost");
            _lblGrossProfit = CreateMetricLabel("Gross Profit");
            _lblNetProfit = CreateMetricLabel("Net Profit");
            _lblPendingRevenue = CreateMetricLabel("Pending Revenue");
            _lblVatCollected = CreateMetricLabel("VAT Collected");

            profitPanel.Controls.Add(_lblRevenue);
            profitPanel.Controls.Add(_lblPurchaseCost);
            profitPanel.Controls.Add(_lblGrossProfit);
            profitPanel.Controls.Add(_lblNetProfit);
            profitPanel.Controls.Add(_lblPendingRevenue);
            profitPanel.Controls.Add(_lblVatCollected);

            _chartProfitMonthly = CreateChart("Monthly Profit Chart", SeriesChartType.Column);
            _chartProfitMonthly.Dock = DockStyle.Top;
            _chartProfitMonthly.Height = 430;

            Button btnProfitExportCsv = new Button { Text = "Export Profit CSV", Width = 170, Height = 35, Location = new Point(15, 560) };
            Button btnProfitExportPdf = new Button { Text = "Export Profit PDF (HTML)", Width = 220, Height = 35, Location = new Point(200, 560) };
            Button btnProfitPrintPreview = new Button { Text = "Print Profit Preview", Width = 210, Height = 35, Location = new Point(430, 560) };
            FormDesignHelper.ApplyCrudButtonStyle(btnProfitExportCsv, Color.FromArgb(39, 174, 96));
            FormDesignHelper.ApplyCrudButtonStyle(btnProfitExportPdf, Color.FromArgb(155, 89, 182));
            FormDesignHelper.ApplyCrudButtonStyle(btnProfitPrintPreview, Color.FromArgb(52, 73, 94));

            btnProfitExportCsv.Click += (_, __) => ExportProfitLoss("csv");
            btnProfitExportPdf.Click += (_, __) => ExportProfitLoss("pdf");
            btnProfitPrintPreview.Click += (_, __) => PrintProfitLoss();

            tabProfit.Controls.Add(_chartProfitMonthly);
            tabProfit.Controls.Add(profitPanel);
            tabProfit.Controls.Add(btnProfitExportCsv);
            tabProfit.Controls.Add(btnProfitExportPdf);
            tabProfit.Controls.Add(btnProfitPrintPreview);

            _tabs.TabPages.Add(tabReports);
            _tabs.TabPages.Add(tabAnalytics);
            _tabs.TabPages.Add(tabProfit);

            Controls.Add(_tabs);

            // Initial dates
            _dtAnchor.Value = DateTime.Today;
            SyncDatePickersForTimePeriod();
        }

        private static Label CreateMetricLabel(string title)
        {
            return new Label
            {
                Text = $"{title}: 0",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Margin = new Padding(16, 20, 16, 0)
            };
        }

        private static Chart CreateChart(string title, SeriesChartType chartType)
        {
            Chart chart = new Chart { BackColor = Color.WhiteSmoke };
            chart.Palette = ChartColorPalette.Bright;
            chart.Dock = DockStyle.Fill;
            ChartArea area = new ChartArea();
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas.Add(area);

            chart.Titles.Add(title);
            chart.Legends.Clear();

            Series s = new Series("Series1");
            s.ChartType = chartType;
            s.XValueType = ChartValueType.String;
            s.YValueType = ChartValueType.Double;
            chart.Series.Add(s);

            return chart;
        }

        private static DataGridView CreateGrid(string title)
        {
            DataGridView grid = new DataGridView
            {
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            grid.TopLeftHeaderCell.Value = title;
            return grid;
        }

        private static Control WrapChart(Control chart)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            panel.Controls.Add(chart);
            chart.Dock = DockStyle.Fill;
            return panel;
        }

        private async void ReportsForm_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadFilterCombosAsync();
                await LoadReportAndDashboardAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("ReportsForm_Load", ex);
                MessageBox.Show(ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadFilterCombosAsync()
        {
            // Customer
            List<Customer> customers = await _customerService.GetAllAsync();
            customers.Insert(0, new Customer { CustomerID = 0, FullName = "All Customers" });
            _cmbCustomer.DataSource = customers;
            _cmbCustomer.DisplayMember = "FullName";
            _cmbCustomer.ValueMember = "CustomerID";

            // Employee
            List<Employee> employees = await _employeeService.GetAllAsync();
            employees.Insert(0, new Employee { EmployeeID = 0, FullName = "All Employees" });
            _cmbEmployee.DataSource = employees;
            _cmbEmployee.DisplayMember = "FullName";
            _cmbEmployee.ValueMember = "EmployeeID";

            // Supplier
            List<Supplier> suppliers = await _supplierService.GetAllAsync();
            suppliers.Insert(0, new Supplier { SupplierID = 0, SupplierName = "All Suppliers" });
            _cmbSupplier.DataSource = suppliers;
            _cmbSupplier.DisplayMember = "SupplierName";
            _cmbSupplier.ValueMember = "SupplierID";

            // Product
            List<Product> products = await _posService.GetProductsAsync("");
            products.Insert(0, new Product { ProductID = 0, ProductName = "All Products" });
            _cmbProduct.DataSource = products;
            _cmbProduct.DisplayMember = "ProductName";
            _cmbProduct.ValueMember = "ProductID";
        }

        private ReportType GetSelectedReportType()
        {
            string selected = _cmbReportType.SelectedItem?.ToString() ?? "";
            if (selected.StartsWith("Daily")) return ReportType.DailySales;
            if (selected.StartsWith("Monthly")) return ReportType.MonthlySales;
            if (selected.StartsWith("Product")) return ReportType.ProductSales;
            if (selected.StartsWith("Customer")) return ReportType.CustomerPurchase;
            if (selected.StartsWith("Supplier")) return ReportType.SupplierPurchase;
            if (selected.StartsWith("Employee")) return ReportType.EmployeeSales;
            if (selected.StartsWith("Pending")) return ReportType.PendingSales;
            if (selected.StartsWith("Paid")) return ReportType.PaidSales;
            if (selected.StartsWith("Inventory")) return ReportType.InventoryStock;
            if (selected.StartsWith("Low Stock")) return ReportType.LowStock;
            if (selected.StartsWith("Expiry")) return ReportType.ExpiryProducts;
            return ReportType.Attendance;
        }

        private ReportFilters BuildFilters(ReportType reportType)
        {
            ReportFilters filters = new ReportFilters
            {
                FromDate = _dtFrom.Value.Date,
                ToDate = _dtTo.Value.Date,
                PaymentStatus = _cmbPaymentStatus.SelectedItem?.ToString() ?? "All"
            };

            if (_cmbCustomer.SelectedValue != null)
            {
                int id = Convert.ToInt32(_cmbCustomer.SelectedValue);
                filters.CustomerId = id > 0 ? (int?)id : null;
            }
            if (_cmbEmployee.SelectedValue != null)
            {
                int id = Convert.ToInt32(_cmbEmployee.SelectedValue);
                filters.EmployeeId = id > 0 ? (int?)id : null;
            }
            if (_cmbProduct.SelectedValue != null)
            {
                int id = Convert.ToInt32(_cmbProduct.SelectedValue);
                filters.ProductId = id > 0 ? (int?)id : null;
            }
            if (_cmbSupplier.SelectedValue != null)
            {
                int id = Convert.ToInt32(_cmbSupplier.SelectedValue);
                filters.SupplierId = id > 0 ? (int?)id : null;
            }

            // Keep pending/paid reports consistent regardless of global payment filter selection.
            if (reportType == ReportType.PendingSales) filters.PaymentStatus = "Pending";
            if (reportType == ReportType.PaidSales) filters.PaymentStatus = "Paid";

            return filters;
        }

        private async Task LoadReportAndDashboardAsync()
        {
            ReportType reportType = GetSelectedReportType();
            ReportFilters filters = BuildFilters(reportType);
            ReportFilters allFiltersForFinancials = new ReportFilters
            {
                FromDate = filters.FromDate,
                ToDate = filters.ToDate,
                CustomerId = filters.CustomerId,
                EmployeeId = filters.EmployeeId,
                ProductId = filters.ProductId,
                SupplierId = filters.SupplierId,
                PaymentStatus = "All"
            };

            _currentReportTitle = _cmbReportType.SelectedItem?.ToString() ?? "Report";

            // Load report grid
            _currentReportData = await _reportsService.GetReportDataAsync(reportType, filters);
            _gridReport.DataSource = null;
            _gridReport.DataSource = _currentReportData;

            _lblReportSummary.Text = _currentReportData == null
                ? "No data"
                : $"Rows: {_currentReportData.Rows.Count}";

            // Load profit/loss + monthly profit chart
            ProfitLossReport profitLoss = await _reportsService.GetProfitLossAsync(allFiltersForFinancials);
            _latestProfitLoss = profitLoss;
            UpdateProfitLossUi(profitLoss);

            // Load analytics charts (based on chosen aggregation)
            TimeAggregationType aggregation = GetAggregationFromUi();
            AnalyticsBundle analytics = await _reportsService.GetAnalyticsAsync(allFiltersForFinancials, aggregation);
            BindAnalyticsUi(analytics, aggregation);
        }

        private void UpdateProfitLossUi(ProfitLossReport profitLoss)
        {
            _lblRevenue.Text = $"Total Revenue: {profitLoss.TotalRevenue:C}";
            _lblPurchaseCost.Text = $"Total Purchase Cost: {profitLoss.TotalPurchaseCost:C}";
            _lblGrossProfit.Text = $"Gross Profit: {profitLoss.GrossProfit:C}";
            _lblNetProfit.Text = $"Net Profit: {profitLoss.NetProfit:C}";
            _lblPendingRevenue.Text = $"Pending Revenue: {profitLoss.PendingRevenue:C}";
            _lblVatCollected.Text = $"VAT Collected: {profitLoss.VATCollected:C}";

            // Chart uses monthly gross profit
            if (profitLoss.TotalRevenue == 0 && profitLoss.TotalPurchaseCost == 0)
            {
                // keep existing empty chart
            }
        }

        private void BindAnalyticsUi(AnalyticsBundle analytics, TimeAggregationType aggregation)
        {
            if (analytics == null) return;

            // Sales trend
            BindLineChart(_chartSalesTrend, analytics.SalesTrend, "PeriodLabel", "TotalRevenue");
            // Revenue trend
            BindLineChart(_chartRevenueTrend, analytics.RevenueTrend, "PeriodLabel", "TotalRevenue");

            // Monthly gross profit
            BindColumnChart(_chartMonthlyGrossProfit, analytics.MonthlyGrossProfit, "MonthLabel", "GrossProfit");

            // Inventory movement
            BindLineChart(_chartInventoryMovement, analytics.InventoryMovementTrend, "PeriodLabel", "UnitsSold");

            // Payment method breakdown (Pie)
            BindPieChart(_chartPaymentMethod, analytics.PaymentMethodBreakdown, "PaymentMethod", "GrandTotalRevenue");

            // Pending vs paid (Column)
            BindColumnChart(_chartPendingVsPaid, analytics.PendingVsPaid, "PaymentStatus", "GrandTotalRevenue");

            // Top products
            _gridTopProducts.DataSource = analytics.TopSellingProducts;
            // Best customers
            _gridBestCustomers.DataSource = analytics.BestCustomers;

            // Profit tab chart (monthly profit)
            _chartProfitMonthly.Series.Clear();
            if (analytics.MonthlyGrossProfit != null)
            {
                foreach (Series s in _chartProfitMonthly.Series)
                {
                    // no-op (cleared already)
                }
                BindColumnChart(_chartProfitMonthly, analytics.MonthlyGrossProfit, "MonthLabel", "GrossProfit");
            }
        }

        private static void BindLineChart(Chart chart, DataTable data, string xColumn, string yColumn)
        {
            if (chart == null) return;
            chart.Series.Clear();
            Series series = new Series("Series1");
            series.ChartType = SeriesChartType.Line;
            series.XValueType = ChartValueType.String;
            series.YValueType = ChartValueType.Double;

            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    string x = row[xColumn].ToString();
                    double y = row[yColumn] == DBNull.Value ? 0 : Convert.ToDouble(row[yColumn]);
                    series.Points.AddXY(x, y);
                }
            }

            chart.Series.Add(series);
        }

        private static void BindColumnChart(Chart chart, DataTable data, string xColumn, string yColumn)
        {
            if (chart == null) return;
            chart.Series.Clear();
            Series series = new Series("Series1");
            series.ChartType = SeriesChartType.Column;
            series.XValueType = ChartValueType.String;
            series.YValueType = ChartValueType.Double;

            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    string x = row[xColumn].ToString();
                    double y = row[yColumn] == DBNull.Value ? 0 : Convert.ToDouble(row[yColumn]);
                    series.Points.AddXY(x, y);
                }
            }
            chart.Series.Add(series);
        }

        private static void BindPieChart(Chart chart, DataTable data, string labelColumn, string valueColumn)
        {
            if (chart == null) return;
            chart.Series.Clear();
            Series series = new Series("Series1");
            series.ChartType = SeriesChartType.Pie;
            series.IsValueShownAsLabel = true;

            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    string label = row[labelColumn].ToString();
                    double value = row[valueColumn] == DBNull.Value ? 0 : Convert.ToDouble(row[valueColumn]);
                    series.Points.AddXY(label, value);
                }
            }
            chart.Series.Add(series);
        }

        private void ExportCurrentReport(string mode)
        {
            if (_currentReportData == null || _currentReportData.Rows.Count == 0)
            {
                MessageBox.Show("No data available to export.");
                return;
            }

            try
            {
                string reportsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                System.IO.Directory.CreateDirectory(reportsDir);

                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    if (mode == "csv")
                        dialog.Filter = "CSV (*.csv)|*.csv";
                    else if (mode == "excel")
                        dialog.Filter = "Excel CSV (*.csv)|*.csv";
                    else
                        dialog.Filter = "PDF-compatible HTML (*.html)|*.html";

                    dialog.InitialDirectory = reportsDir;
                    dialog.FileName = $"{_currentReportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.{(mode == "pdf" ? "html" : "csv")}";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (mode == "pdf")
                        {
                            string path = _exportService.ExportDataTableToPdfHtml(_currentReportData, dialog.FileName, _currentReportTitle);
                            MessageBox.Show($"Report exported. Open and print from: {path}");
                        }
                        else if (mode == "excel")
                        {
                            string path = _exportService.ExportDataTableToExcelCsv(_currentReportData, dialog.FileName);
                            MessageBox.Show($"Excel export completed: {path}");
                        }
                        else
                        {
                            string path = _exportService.ExportDataTableToCsv(_currentReportData, dialog.FileName);
                            MessageBox.Show($"CSV export completed: {path}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("ReportsForm.ExportCurrentReport", ex);
                MessageBox.Show(ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintCurrentReport(bool showPreview)
        {
            if (_currentReportData == null || _currentReportData.Rows.Count == 0)
            {
                MessageBox.Show("No data available to print.");
                return;
            }

            try
            {
                _exportService.PrintDataTableWithPreview(_currentReportData, _currentReportTitle, showPreview);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("ReportsForm.PrintCurrentReport", ex);
                MessageBox.Show(ex.Message, "Print", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private ProfitLossReport _latestProfitLoss;

        private void ExportProfitLoss(string mode)
        {
            if (_latestProfitLoss == null)
            {
                MessageBox.Show("Load a report first.");
                return;
            }

            try
            {
                DataTable table = BuildProfitLossDataTable(_latestProfitLoss);
                string reportsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                System.IO.Directory.CreateDirectory(reportsDir);

                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    if (mode == "csv")
                        dialog.Filter = "CSV (*.csv)|*.csv";
                    else
                        dialog.Filter = "PDF-compatible HTML (*.html)|*.html";

                    dialog.InitialDirectory = reportsDir;
                    dialog.FileName = $"ProfitLoss_{DateTime.Now:yyyyMMdd_HHmm}.{(mode == "pdf" ? "html" : "csv")}";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (mode == "pdf")
                        {
                            string path = _exportService.ExportDataTableToPdfHtml(table, dialog.FileName, "Profit/Loss");
                            MessageBox.Show($"Profit/Loss exported. Open and print from: {path}");
                        }
                        else
                        {
                            string path = _exportService.ExportDataTableToCsv(table, dialog.FileName);
                            MessageBox.Show($"Profit/Loss CSV exported: {path}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("ReportsForm.ExportProfitLoss", ex);
                MessageBox.Show(ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintProfitLoss()
        {
            if (_latestProfitLoss == null)
            {
                MessageBox.Show("Load a report first.");
                return;
            }

            DataTable table = BuildProfitLossDataTable(_latestProfitLoss);
            _exportService.PrintDataTableWithPreview(table, "Profit/Loss", true);
        }

        private static DataTable BuildProfitLossDataTable(ProfitLossReport report)
        {
            DataTable t = new DataTable();
            t.Columns.Add("Metric");
            t.Columns.Add("Value");

            t.Rows.Add("Total Revenue", report.TotalRevenue);
            t.Rows.Add("Total Purchase Cost", report.TotalPurchaseCost);
            t.Rows.Add("Gross Profit", report.GrossProfit);
            t.Rows.Add("Net Profit", report.NetProfit);
            t.Rows.Add("Pending Revenue", report.PendingRevenue);
            t.Rows.Add("VAT Collected", report.VATCollected);
            return t;
        }
    }
}

