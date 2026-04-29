using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;
using Super_Shop_Management_System.Reports;

namespace Super_Shop_Management_System.Forms
{
    public class AttendanceForm : Form
    {
        private readonly AttendanceService _attendanceService = new AttendanceService();
        private readonly EmployeeService _employeeService = new EmployeeService();

        private ComboBox _cmbEmployee;
        private DateTimePicker _dtDate;
        private DateTimePicker _dtCheckIn;
        private DateTimePicker _dtCheckOut;
        private ComboBox _cmbStatus;
        private DateTimePicker _dtMonth;
        private DataGridView _grid;

        public AttendanceForm()
        {
            InitializeComponent();
            Load += AttendanceForm_Load;
        }

        private async void AttendanceForm_Load(object sender, EventArgs e)
        {
            await LoadEmployeesAsync();
            await LoadAttendanceAsync();
        }

        private void InitializeComponent()
        {
            Text = "Attendance Management";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 620);
            ThemeManager.ApplyFormTheme(this);

            _cmbEmployee = new ComboBox { Location = new Point(20, 40), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _dtDate = new DateTimePicker { Location = new Point(210, 40), Width = 120, Format = DateTimePickerFormat.Short };
            _dtCheckIn = new DateTimePicker { Location = new Point(340, 40), Width = 120, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            _dtCheckOut = new DateTimePicker { Location = new Point(470, 40), Width = 120, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            _cmbStatus = new ComboBox { Location = new Point(600, 40), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbStatus.Items.AddRange(new object[] { "Present", "Absent", "Leave" });

            Button btnSave = new Button { Text = "Check In/Out Save", Location = new Point(730, 38), Width = 130 };
            FormDesignHelper.ApplyCrudButtonStyle(btnSave, ThemeManager.Primary);
            btnSave.Click += async (_, __) => await SaveAttendanceAsync();

            _dtMonth = new DateTimePicker { Location = new Point(20, 100), Width = 150, Format = DateTimePickerFormat.Custom, CustomFormat = "MMMM yyyy" };
            _dtMonth.ValueChanged += async (_, __) => await LoadAttendanceAsync();

            Button btnReport = new Button { Text = "Refresh Report", Location = new Point(180, 98), Width = 120 };
            FormDesignHelper.ApplyCrudButtonStyle(btnReport, Color.FromArgb(52, 73, 94));
            btnReport.Click += async (_, __) => await LoadAttendanceAsync();

            Button btnOpenReportForm = new Button { Text = "Open Attendance Report", Location = new Point(310, 98), Width = 180 };
            FormDesignHelper.ApplyCrudButtonStyle(btnOpenReportForm, Color.FromArgb(155, 89, 182));
            btnOpenReportForm.Click += (_, __) =>
            {
                using (AttendanceReportForm reportForm = new AttendanceReportForm())
                {
                    reportForm.ShowDialog();
                }
            };

            _grid = new DataGridView
            {
                Location = new Point(20, 140),
                Width = 920,
                Height = 420,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_grid);

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AttendanceID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeName", HeaderText = "Employee", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Date", HeaderText = "Date", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckIn", HeaderText = "Check In", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckOut", HeaderText = "Check Out", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 110 });

            Controls.Add(new Label { Text = "Employee", Location = new Point(20, 20), AutoSize = true });
            Controls.Add(_cmbEmployee);
            Controls.Add(new Label { Text = "Date", Location = new Point(210, 20), AutoSize = true });
            Controls.Add(_dtDate);
            Controls.Add(new Label { Text = "Check In", Location = new Point(340, 20), AutoSize = true });
            Controls.Add(_dtCheckIn);
            Controls.Add(new Label { Text = "Check Out", Location = new Point(470, 20), AutoSize = true });
            Controls.Add(_dtCheckOut);
            Controls.Add(new Label { Text = "Status", Location = new Point(600, 20), AutoSize = true });
            Controls.Add(_cmbStatus);
            Controls.Add(btnSave);
            Controls.Add(new Label { Text = "Monthly Report", Location = new Point(20, 80), AutoSize = true });
            Controls.Add(_dtMonth);
            Controls.Add(btnReport);
            Controls.Add(btnOpenReportForm);
            Controls.Add(_grid);
        }

        private async Task LoadEmployeesAsync()
        {
            List<Employee> employees = await _employeeService.GetAllAsync();
            _cmbEmployee.DataSource = employees;
            _cmbEmployee.DisplayMember = "FullName";
            _cmbEmployee.ValueMember = "EmployeeID";
        }

        private async Task SaveAttendanceAsync()
        {
            try
            {
                Attendance attendance = new Attendance
                {
                    EmployeeID = _cmbEmployee.SelectedValue == null ? 0 : Convert.ToInt32(_cmbEmployee.SelectedValue),
                    Date = _dtDate.Value.Date,
                    CheckIn = _dtCheckIn.Value,
                    CheckOut = _dtCheckOut.Value,
                    Status = _cmbStatus.Text
                };

                await _attendanceService.SaveAttendanceAsync(attendance);
                await LoadAttendanceAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SaveAttendance", ex);
                MessageBox.Show(ex.Message, "Attendance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadAttendanceAsync()
        {
            try
            {
                _grid.DataSource = await _attendanceService.GetAttendanceAsync(null, _dtMonth.Value);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("LoadAttendance", ex);
                MessageBox.Show(ex.Message, "Attendance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
