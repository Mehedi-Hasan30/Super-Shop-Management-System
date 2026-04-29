using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Reports
{
    public class AttendanceReportForm : Form
    {
        private readonly AttendanceService _attendanceService = new AttendanceService();
        private readonly EmployeeService _employeeService = new EmployeeService();

        private ComboBox _cmbEmployeeFilter;
        private DateTimePicker _dtMonth;
        private DataGridView _grid;
        private Label _lblSummary;
        private List<Attendance> _currentData = new List<Attendance>();

        public AttendanceReportForm()
        {
            InitializeComponent();
            Load += AttendanceReportForm_Load;
        }

        private async void AttendanceReportForm_Load(object sender, EventArgs e)
        {
            await LoadEmployeeFilterAsync();
            await LoadReportAsync();
        }

        private void InitializeComponent()
        {
            Text = "Attendance Report";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1060, 680);
            ThemeManager.ApplyFormTheme(this);

            _cmbEmployeeFilter = new ComboBox { Location = new Point(20, 38), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _dtMonth = new DateTimePicker { Location = new Point(250, 38), Width = 150, Format = DateTimePickerFormat.Custom, CustomFormat = "MMMM yyyy" };

            Button btnLoad = new Button { Text = "Load Report", Location = new Point(410, 36), Width = 110 };
            Button btnExportExcel = new Button { Text = "Export Excel", Location = new Point(530, 36), Width = 110 };
            Button btnExportPdf = new Button { Text = "Export PDF", Location = new Point(650, 36), Width = 110 };
            Button btnPrint = new Button { Text = "Print", Location = new Point(770, 36), Width = 90 };

            FormDesignHelper.ApplyCrudButtonStyle(btnLoad, ThemeManager.Primary);
            FormDesignHelper.ApplyCrudButtonStyle(btnExportExcel, Color.FromArgb(39, 174, 96));
            FormDesignHelper.ApplyCrudButtonStyle(btnExportPdf, Color.FromArgb(155, 89, 182));
            FormDesignHelper.ApplyCrudButtonStyle(btnPrint, Color.FromArgb(52, 73, 94));

            btnLoad.Click += async (_, __) => await LoadReportAsync();
            btnExportExcel.Click += BtnExportExcel_Click;
            btnExportPdf.Click += BtnExportPdf_Click;
            btnPrint.Click += (_, __) => AttendanceReportExporter.PrintGrid(_grid, "Attendance Report");

            _lblSummary = new Label { Location = new Point(20, 76), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            _grid = new DataGridView
            {
                Location = new Point(20, 105),
                Width = 1000,
                Height = 520,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AttendanceID", HeaderText = "ID", Width = 60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeName", HeaderText = "Employee", Width = 220 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Date", HeaderText = "Date", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckIn", HeaderText = "Check In", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckOut", HeaderText = "Check Out", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 120 });

            Controls.Add(new Label { Text = "Employee", Location = new Point(20, 18), AutoSize = true });
            Controls.Add(_cmbEmployeeFilter);
            Controls.Add(new Label { Text = "Month", Location = new Point(250, 18), AutoSize = true });
            Controls.Add(_dtMonth);
            Controls.Add(btnLoad);
            Controls.Add(btnExportExcel);
            Controls.Add(btnExportPdf);
            Controls.Add(btnPrint);
            Controls.Add(_lblSummary);
            Controls.Add(_grid);
        }

        private async Task LoadEmployeeFilterAsync()
        {
            List<Employee> employees = await _employeeService.GetAllAsync();
            employees.Insert(0, new Employee { EmployeeID = 0, FullName = "All Employees" });
            _cmbEmployeeFilter.DataSource = employees;
            _cmbEmployeeFilter.DisplayMember = "FullName";
            _cmbEmployeeFilter.ValueMember = "EmployeeID";
        }

        private async Task LoadReportAsync()
        {
            try
            {
                int selectedEmployeeId = _cmbEmployeeFilter.SelectedValue == null ? 0 : Convert.ToInt32(_cmbEmployeeFilter.SelectedValue);
                int? filterEmployeeId = selectedEmployeeId > 0 ? selectedEmployeeId : (int?)null;
                _currentData = await _attendanceService.GetAttendanceAsync(filterEmployeeId, _dtMonth.Value);
                _grid.DataSource = null;
                _grid.DataSource = _currentData;

                int total = _currentData.Count;
                int present = 0;
                int absent = 0;
                foreach (Attendance item in _currentData)
                {
                    if (string.Equals(item.Status, "Present", StringComparison.OrdinalIgnoreCase)) present++;
                    if (string.Equals(item.Status, "Absent", StringComparison.OrdinalIgnoreCase)) absent++;
                }

                _lblSummary.Text = $"Total Records: {total} | Present: {present} | Absent: {absent}";
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("LoadReportAsync", ex);
                MessageBox.Show(ex.Message, "Attendance Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            if (_currentData.Count == 0)
            {
                MessageBox.Show("No data available for export.");
                return;
            }

            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports"));
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel CSV (*.csv)|*.csv";
                dialog.FileName = $"Attendance_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string path = AttendanceReportExporter.ExportToExcelCsv(_currentData, dialog.FileName);
                    MessageBox.Show($"Excel export completed.\n{path}");
                }
            }
        }

        private void BtnExportPdf_Click(object sender, EventArgs e)
        {
            if (_currentData.Count == 0)
            {
                MessageBox.Show("No data available for export.");
                return;
            }

            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports"));
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF-compatible HTML (*.html)|*.html";
                dialog.FileName = $"Attendance_{DateTime.Now:yyyyMMdd_HHmm}.html";
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string path = AttendanceReportExporter.ExportToPdfHtml(_currentData, dialog.FileName, "Attendance Report");
                    MessageBox.Show($"Report exported. Open in browser and print to PDF.\n{path}");
                }
            }
        }
    }
}
