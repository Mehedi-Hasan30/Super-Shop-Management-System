using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public class AttendanceUserControl : UserControl
    {
        private readonly AttendanceService _attendanceService = new AttendanceService();

        private DataGridView _grid;
        private ComboBox _cmbEmployee;
        private DateTimePicker _dtpDate;
        private TextBox _txtCheckIn;
        private TextBox _txtCheckOut;
        private ComboBox _cmbStatus;
        private TextBox _txtSearch;

        public AttendanceUserControl()
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
                Text = "Attendance",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var panel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(500, 180),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _cmbEmployee = new ComboBox
            {
                Location = new Point(20, 40),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbEmployee.DisplayMember = "FullName";
            _cmbEmployee.ValueMember = "EmployeeID";

            _dtpDate = new DateTimePicker { Location = new Point(20, 90), Width = 300, Format = DateTimePickerFormat.Short };

            _txtCheckIn = new TextBox { Location = new Point(20, 130), Width = 150 };
            _txtCheckOut = new TextBox { Location = new Point(190, 130), Width = 150 };

            _cmbStatus = new ComboBox
            {
                Location = new Point(20, 170),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbStatus.Items.AddRange(new object[] { "Present", "Absent", "Late", "Excused" });
            _cmbStatus.SelectedIndex = 0;

            _txtSearch = new TextBox { Location = new Point(20, 200), Width = 300 };
            _txtSearch.TextChanged += async (_, __) => await LoadAttendanceAsync();

            Button btnSave = new RoundedButton { Text = "Save Attendance", Location = new Point(20, 220), Width = 140, CornerRadius = 6 };
            btnSave.Click += BtnSave_Click;

            _grid = new DataGridView
            {
                Location = new Point(20, 260),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

DataGridStyler.ApplyModernStyle(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeName", HeaderText = "Employee", Width = 200 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Date", HeaderText = "Date", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckIn", HeaderText = "Check In", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckOut", HeaderText = "Check Out", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 80 });

            Controls.Add(title);
            Controls.Add(panel);
            panel.Controls.Add(_cmbEmployee);
            panel.Controls.Add(new Label { Text = "Employee", Location = new Point(20, 20), AutoSize = true });
            panel.Controls.Add(_dtpDate);
            panel.Controls.Add(new Label { Text = "Date", Location = new Point(20, 70), AutoSize = true });
            panel.Controls.Add(_txtCheckIn);
            panel.Controls.Add(new Label { Text = "Check In", Location = new Point(20, 110), AutoSize = true });
            panel.Controls.Add(_txtCheckOut);
            panel.Controls.Add(new Label { Text = "Check Out", Location = new Point(190, 110), AutoSize = true });
            panel.Controls.Add(_cmbStatus);
            panel.Controls.Add(new Label { Text = "Status", Location = new Point(20, 150), AutoSize = true });
            panel.Controls.Add(_txtSearch);
            Controls.Add(_grid);
        }

        private async void AttendanceUserControl_Load(object sender, EventArgs e)
        {
            await LoadEmployeesForCombo();
            await LoadAttendanceAsync();
        }

        private async Task LoadEmployeesForCombo()
        {
            try
            {
                // We'll bind manually since EmployeeService.GetAllAsync returns List<Employee>
                var employees = new List<object>();
                // We'll populate this when we have employee data
                _cmbEmployee.DataSource = employees;
            }
            catch { }
        }

        private async Task LoadAttendanceAsync(string keyword = "")
        {
            using (var loading = new LoadingIndicator(this, "Loading attendance..."))
            {
                try
                {
                    var attendanceList = await _attendanceService.GetAttendanceAsync(null, null);
                    _grid.DataSource = attendanceList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to load attendance: {ex.Message}", "Attendance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Attendance attendance))
            {
                return;
            }

            // populate form for editing
            // Find employee by ID
            _cmbEmployee.SelectedValue = attendance.EmployeeID;
            _dtpDate.Value = attendance.Date;
            _txtCheckIn.Text = attendance.CheckIn.HasValue ? attendance.CheckIn.Value.ToString(@"hh\:mm") : "";
            _txtCheckOut.Text = attendance.CheckOut.HasValue ? attendance.CheckOut.Value.ToString(@"hh\:mm") : "";
            _cmbStatus.SelectedItem = attendance.Status;
        }

        private void ClearInputs()
        {
            _cmbEmployee.SelectedIndex = -1;
            _dtpDate.Value = DateTime.Now;
            _txtCheckIn.Clear();
            _txtCheckOut.Clear();
            _cmbStatus.SelectedIndex = 0;
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Select an employee first.", "Attendance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtCheckIn.Text))
                {
                    MessageBox.Show("Check in time is required.", "Attendance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Attendance attendance = new Attendance
                {
                    EmployeeID = Convert.ToInt32(_cmbEmployee.SelectedValue),
                    EmployeeName = _cmbEmployee.Text,
                    Date = _dtpDate.Value,
                    CheckIn = DateTime.Parse(_txtCheckIn.Text),
                    CheckOut = string.IsNullOrWhiteSpace(_txtCheckOut.Text) ? (DateTime?)null : DateTime.Parse(_txtCheckOut.Text),
                    Status = _cmbStatus.SelectedItem.ToString()
                };

                bool ok = await _attendanceService.SaveAttendanceAsync(attendance);
                if (ok)
                {
                    await LoadAttendanceAsync();
                    ClearInputs();
                    ToastNotification.Show("Attendance saved successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Attendance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}