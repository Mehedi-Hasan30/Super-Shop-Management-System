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
    public class EmployeeUserControl : UserControl
    {
        private readonly EmployeeService _employeeService = new EmployeeService();

        private DataGridView _grid;
        private TextBox _txtFullName;
        private TextBox _txtUsername;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtAddress;
        private TextBox _txtRole;
        private NumericUpDown _numSalary;
        private TextBox _txtShift;
        private DateTimePicker _dtpJoinDate;
        private TextBox _txtPassword;
        private CheckBox _chkSetPassword;
        private TextBox _txtSearch;

        public EmployeeUserControl()
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
                Text = "Employee Management",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var panel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(500, 220),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _txtFullName = new TextBox { Location = new Point(20, 40), Width = 300 };
            _txtUsername = new TextBox { Location = new Point(20, 90), Width = 300 };
            _txtPassword = new TextBox { Location = new Point(20, 140), Width = 300, UseSystemPasswordChar = true };
            _chkSetPassword = new CheckBox { Text = "Set password", Location = new Point(20, 165), AutoSize = true };
            _txtPhone = new TextBox { Location = new Point(340, 40), Width = 150 };
            _txtEmail = new TextBox { Location = new Point(340, 90), Width = 150 };
            _txtAddress = new TextBox { Location = new Point(20, 190), Width = 470 };
            _txtRole = new TextBox { Location = new Point(20, 90), Width = 150 };
            _numSalary = new NumericUpDown { Location = new Point(340, 90), Width = 150, Maximum = 999999999, Minimum = 0 };
            _txtShift = new TextBox { Location = new Point(20, 140), Width = 150 };
            _dtpJoinDate = new DateTimePicker { Location = new Point(340, 140), Width = 150, Format = DateTimePickerFormat.Short };
            _txtSearch = new TextBox { Location = new Point(20, 180), Width = 300 };

            Button btnAdd = new RoundedButton { Text = "Add", Location = new Point(20, 210), Width = 80, CornerRadius = 6 };
            btnAdd.Click += BtnAdd_Click;

            Button btnUpdate = new RoundedButton { Text = "Update", Location = new Point(120, 210), Width = 80, CornerRadius = 6 };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new RoundedButton { Text = "Delete", Location = new Point(220, 210), Width = 80, CornerRadius = 6 };
            btnDelete.Click += BtnDelete_Click;

            _txtSearch.TextChanged += async (_, __) => await LoadEmployeesAsync();

            _grid = new DataGridView
            {
                Location = new Point(20, 240),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Full Name", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Username", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Role", HeaderText = "Role", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Salary", HeaderText = "Salary", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Shift", HeaderText = "Shift", Width = 80 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(title);
            Controls.Add(panel);
            Controls.Add(_grid);
        }

        private void EmployeeUserControl_Load(object sender, EventArgs e)
        {
            LoadEmployeesAsync("").GetAwaiter().GetResult();
        }

        private async Task LoadEmployeesAsync(string keyword = "")
        {
            try
            {
                List<Employee> employees = await _employeeService.GetAllAsync(keyword);
                _grid.DataSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load employees: {ex.Message}", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Employee employee))
            {
                return;
            }

            _txtFullName.Text = employee.FullName;
            _txtUsername.Text = employee.Username;
            _txtRole.Text = employee.Role;
            _numSalary.Value = employee.Salary;
            _txtShift.Text = employee.Shift;
            _dtpJoinDate.Value = employee.JoinDate;
            _txtPassword.Text = "";
            _chkSetPassword.Checked = false;
        }

        private void ClearInputs()
        {
            _txtFullName.Clear();
            _txtUsername.Clear();
            _txtPassword.Clear();
            _chkSetPassword.Checked = false;
            _txtRole.Clear();
            _numSalary.Value = 0;
            _txtShift.Clear();
            _dtpJoinDate.Value = DateTime.Now;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_txtFullName.Text) || string.IsNullOrWhiteSpace(_txtUsername.Text))
                {
                    MessageBox.Show("Full name and username are required.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_numSalary.Value <= 0)
                {
                    MessageBox.Show("Salary must be greater than zero.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtRole.Text) || string.IsNullOrWhiteSpace(_txtShift.Text))
                {
                    MessageBox.Show("Role and shift are required.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Employee employee = new Employee
                {
                    FullName = _txtFullName.Text,
                    Username = _txtUsername.Text,
                    Phone = _txtPhone.Text,
                    Email = _txtEmail.Text,
                    Address = _txtAddress.Text,
                    Role = _txtRole.Text,
                    Salary = _numSalary.Value,
                    Shift = _txtShift.Text,
                    JoinDate = _dtpJoinDate.Value
                };

                if (_chkSetPassword.Checked && !string.IsNullOrWhiteSpace(_txtPassword.Text))
                {
                    employee.Password = _txtPassword.Text;
                }

                bool success = await _employeeService.AddAsync(employee);
                if (success)
                {
                    await LoadEmployeesAsync();
                    ClearInputs();
                    MessageBox.Show("Employee added successfully.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_grid.CurrentRow?.DataBoundItem is Employee employee) || employee.EmployeeID <= 0)
                {
                    MessageBox.Show("Select an employee first.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                employee.FullName = _txtFullName.Text;
                employee.Username = _txtUsername.Text;
                employee.Phone = _txtPhone.Text;
                employee.Email = _txtEmail.Text;
                employee.Address = _txtAddress.Text;
                employee.Role = _txtRole.Text;
                employee.Salary = _numSalary.Value;
                employee.Shift = _txtShift.Text;
                employee.JoinDate = _dtpJoinDate.Value;

                if (_chkSetPassword.Checked && !string.IsNullOrWhiteSpace(_txtPassword.Text))
                {
                    employee.Password = _txtPassword.Text;
                }
                else
                {
                    employee.Password = "";
                }

                bool success = await _employeeService.UpdateAsync(employee, _chkSetPassword.Checked ? _txtPassword.Text : null);
                if (success)
                {
                    await LoadEmployeesAsync();
                    ClearInputs();
                    MessageBox.Show("Employee updated successfully.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_grid.CurrentRow?.DataBoundItem is Employee employee) || employee.EmployeeID <= 0)
                {
                    MessageBox.Show("Select an employee first.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("Delete selected employee?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                await _employeeService.DeleteAsync(employee.EmployeeID);
                await LoadEmployeesAsync();
                ClearInputs();
                MessageBox.Show("Employee deleted successfully.", "Employee", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}