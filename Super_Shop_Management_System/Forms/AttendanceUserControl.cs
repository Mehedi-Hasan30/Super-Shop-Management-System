using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

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
            // Premium header panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Attendance, 32F, ThemeManager.Warning);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            var title = new Label
            {
                Text = "Attendance Management",
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

            // Input panel card
            var inputCard = new RoundedPanel
            {
                Location = new Point(20, 90),
                Size = new Size(520, 190),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(16)
            };

            _txtCheckIn = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, inputCard.Padding.Top),
                Size = new Size(200, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtCheckOut = new TextBox
            {
                Location = new Point(inputCard.Padding.Left + 230, inputCard.Padding.Top),
                Size = new Size(200, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _cmbEmployee = new ComboBox
            {
                Location = new Point(inputCard.Padding.Left, _txtCheckOut.Bottom + 16),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                ForeColor = ThemeManager.MutedText
            };

            var lblEmployee = new Label
            {
                Text = "Employee",
                Location = new Point(inputCard.Padding.Left, _txtCheckOut.Bottom + 5),
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText
            };

            _dtpDate = new DateTimePicker
            {
                Location = new Point(inputCard.Padding.Left, _cmbEmployee.Bottom + 16),
                Size = new Size(260, 30),
                Format = DateTimePickerFormat.Short,
                ForeColor = ThemeManager.MutedText
            };

            var lblDate = new Label
            {
                Text = "Date",
                Location = new Point(inputCard.Padding.Left, _cmbEmployee.Bottom + 5),
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText
            };

            _cmbStatus = new ComboBox
            {
                Location = new Point(inputCard.Padding.Left, _dtpDate.Bottom + 16),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                ForeColor = ThemeManager.MutedText
            };

            var lblStatus = new Label
            {
                Text = "Status",
                Location = new Point(inputCard.Padding.Left, _dtpDate.Bottom + 5),
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText
            };

            var btnSave = UIStyleKit.CreateButton(ButtonStyle.Primary, "Save Attendance", 120, 36);
            btnSave.Location = new Point(inputCard.Padding.Left, _cmbStatus.Bottom + 20);
            btnSave.Click += BtnSave_Click;
            inputCard.Controls.Add(_txtCheckIn);
            inputCard.Controls.Add(_txtCheckOut);
            inputCard.Controls.Add(_cmbEmployee);
            inputCard.Controls.Add(lblEmployee);
            inputCard.Controls.Add(_dtpDate);
            inputCard.Controls.Add(lblDate);
            inputCard.Controls.Add(_cmbStatus);
            inputCard.Controls.Add(lblStatus);
            inputCard.Controls.Add(btnSave);

            // Stats cards row
            var statsPanel = new Panel
            {
                Location = new Point(20, 290),
                Size = new Size(520, 50),
                BackColor = Color.Transparent
            };

            var todayCard = UIStyleKit.CreateStatCard("Today", "0", IconHelper.Glyphs.Attendance, ThemeManager.Warning, 180, 40);
            todayCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(todayCard);

            var lateCard = UIStyleKit.CreateStatCard("Late", "0", IconHelper.Glyphs.Alert, ThemeManager.Warning, 180, 40);
            lateCard.Location = new Point(210, 10);
            statsPanel.Controls.Add(lateCard);

            // Grid area
            _grid = new DataGridView
            {
                Location = new Point(20, 350),
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
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(headerPanel);
            Controls.Add(inputCard);
            Controls.Add(statsPanel);
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