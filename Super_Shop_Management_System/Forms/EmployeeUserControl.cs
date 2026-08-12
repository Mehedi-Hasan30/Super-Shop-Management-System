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
            // Premium header panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Employees, 32F, ThemeManager.Primary);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            var title = new Label
            {
                Text = "Employee Management",
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
                Size = new Size(520, 220),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(16)
            };

            _txtFullName = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, inputCard.Padding.Top),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtUsername = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtFullName.Bottom + 12),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtPhone = new TextBox
            {
                Location = new Point(inputCard.Padding.Left + 260 + 16, _txtFullName.Top),
                Size = new Size(130, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtEmail = new TextBox
            {
                Location = new Point(inputCard.Padding.Left + 260 + 16, _txtUsername.Bottom + 12),
                Size = new Size(150, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtAddress = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtEmail.Bottom + 20),
                Size = new Size(490, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtRole = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtPhone.Bottom + 12),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _numSalary = new NumericUpDown
            {
                Location = new Point(inputCard.Padding.Left + 260 + 16, _txtEmail.Bottom + 12),
                Size = new Size(150, 30),
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtShift = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtRole.Bottom + 12),
                Size = new Size(150, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _dtpJoinDate = new DateTimePicker
            {
                Location = new Point(inputCard.Padding.Left + 260 + 16, _txtRole.Bottom + 12),
                Size = new Size(150, 30),
                Format = DateTimePickerFormat.Short,
                ForeColor = ThemeManager.MutedText
            };

            _txtPassword = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtShift.Bottom + 12),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText,
                UseSystemPasswordChar = true
            };

            var btnAdd = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add", 80, 36);
            btnAdd.Location = new Point(inputCard.Padding.Left, 20);
            btnAdd.Click += BtnAdd_Click;
            inputCard.Controls.Add(_txtFullName);
            inputCard.Controls.Add(_txtUsername);
            inputCard.Controls.Add(_txtPhone);
            inputCard.Controls.Add(_txtEmail);
            inputCard.Controls.Add(_txtAddress);
            inputCard.Controls.Add(_txtRole);
            inputCard.Controls.Add(_numSalary);
            inputCard.Controls.Add(_txtShift);
            inputCard.Controls.Add(_dtpJoinDate);
            inputCard.Controls.Add(_txtPassword);
            inputCard.Controls.Add(btnAdd);

            var btnUpdate = UIStyleKit.CreateButton(ButtonStyle.Secondary, "Update", 80, 36);
            btnUpdate.Location = new Point(inputCard.Padding.Left + 100, 20);
            btnUpdate.Click += BtnUpdate_Click;

            var btnDelete = UIStyleKit.CreateButton(ButtonStyle.Danger, "Delete", 80, 36);
            btnDelete.Location = new Point(inputCard.Padding.Left + 200, 20);
            btnDelete.Click += BtnDelete_Click;

            // Stats cards row
            var statsPanel = new Panel
            {
                Location = new Point(20, 320),
                Size = new Size(520, 50),
                BackColor = Color.Transparent
            };

            var totalCard = UIStyleKit.CreateStatCard("Employees", "0", IconHelper.Glyphs.Employees, ThemeManager.Info, 180, 40);
            totalCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(totalCard);

            var activeCard = UIStyleKit.CreateStatCard("Active", "0", IconHelper.Glyphs.Employees, ThemeManager.Primary, 180, 40);
            activeCard.Location = new Point(210, 10);
            statsPanel.Controls.Add(activeCard);

            // Grid area
            _grid = new DataGridView
            {
                Location = new Point(20, 380),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            DataGridStyler.ApplyModernStyle(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Full Name", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Username", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Role", HeaderText = "Role", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Salary", HeaderText = "Salary", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Shift", HeaderText = "Shift", Width = 80 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(headerPanel);
            Controls.Add(inputCard);
            Controls.Add(statsPanel);
            Controls.Add(_grid);
        }

        private void EmployeeUserControl_Load(object sender, EventArgs e)
        {
            LoadEmployeesAsync("").GetAwaiter().GetResult();
        }

        private async Task LoadEmployeesAsync(string keyword = "")
        {
            using (var loading = new LoadingIndicator(this, "Loading employees..."))
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
                    ToastNotification.Show("Employee added successfully.");
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
                    ToastNotification.Show("Employee updated successfully.");
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
                ToastNotification.Show("Employee deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}