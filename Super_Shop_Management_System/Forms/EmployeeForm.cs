using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class EmployeeForm : Form
    {
        private readonly EmployeeService _service = new EmployeeService();
        private DataGridView _grid;
        private TextBox _txtFullName;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtAddress;
        private ComboBox _cmbRole;
        private NumericUpDown _numSalary;
        private ComboBox _cmbShift;
        private DateTimePicker _dtJoinDate;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private TextBox _txtSearch;
        private int _selectedId;

        public EmployeeForm()
        {
            InitializeComponent();
            Load += async (_, __) => await LoadEmployeesAsync();
        }

        private void InitializeComponent()
        {
            Text = "Employee Management";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1180, 680);
            ThemeManager.ApplyFormTheme(this);

            _txtFullName = new TextBox { Location = new Point(20, 40), Width = 170 };
            _txtPhone = new TextBox { Location = new Point(200, 40), Width = 120 };
            _txtEmail = new TextBox { Location = new Point(330, 40), Width = 170 };
            _txtAddress = new TextBox { Location = new Point(510, 40), Width = 180 };
            _cmbRole = new ComboBox { Location = new Point(700, 40), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            _numSalary = new NumericUpDown { Location = new Point(810, 40), Width = 100, Maximum = 100000000, DecimalPlaces = 2, Minimum = 0 };
            _cmbShift = new ComboBox { Location = new Point(920, 40), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };

            _cmbRole.Items.AddRange(new object[] { "Admin", "Employee" });
            _cmbShift.Items.AddRange(new object[] { "Morning", "Evening", "Night" });

            _dtJoinDate = new DateTimePicker { Location = new Point(20, 95), Width = 170, Format = DateTimePickerFormat.Short };
            _txtUsername = new TextBox { Location = new Point(200, 95), Width = 170 };
            _txtPassword = new TextBox { Location = new Point(380, 95), Width = 220, PasswordChar = '*' };

            Controls.Add(new Label { Text = "Full Name", Location = new Point(20, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Phone", Location = new Point(200, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Email", Location = new Point(330, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Address", Location = new Point(510, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Role", Location = new Point(700, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Salary", Location = new Point(810, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Shift", Location = new Point(920, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Join Date", Location = new Point(20, 75), AutoSize = true });
            Controls.Add(new Label { Text = "Username", Location = new Point(200, 75), AutoSize = true });
            Controls.Add(new Label { Text = "Password", Location = new Point(380, 75), AutoSize = true });
            Controls.Add(new Label { Text = "Leave blank during update to keep existing password.", Location = new Point(610, 96), AutoSize = true, ForeColor = Color.DimGray });

            Button btnAdd = new Button { Text = "Add", Location = new Point(620, 93), Width = 90 };
            Button btnUpdate = new Button { Text = "Update", Location = new Point(715, 93), Width = 90 };
            Button btnDelete = new Button { Text = "Delete", Location = new Point(810, 93), Width = 90, BackColor = Color.FromArgb(192, 57, 43) };
            FormDesignHelper.ApplyCrudButtonStyle(btnAdd, Color.FromArgb(39, 174, 96));
            FormDesignHelper.ApplyCrudButtonStyle(btnUpdate, ThemeManager.Primary);
            FormDesignHelper.ApplyCrudButtonStyle(btnDelete, Color.FromArgb(192, 57, 43));

            btnAdd.Click += async (_, __) => await SaveAsync(false);
            btnUpdate.Click += async (_, __) => await SaveAsync(true);
            btnDelete.Click += async (_, __) => await DeleteAsync();

            _txtSearch = new TextBox { Location = new Point(20, 150), Width = 300 };
            FormDesignHelper.ApplySearchBoxStyle(_txtSearch);
            _txtSearch.TextChanged += async (_, __) => await LoadEmployeesAsync(_txtSearch.Text);

            _grid = new DataGridView
            {
                Location = new Point(20, 185),
                Width = 1120,
                Height = 440,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Employee", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Phone", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Role", HeaderText = "Role", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Salary", HeaderText = "Salary", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Shift", HeaderText = "Shift", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "JoinDate", HeaderText = "Join Date", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Username", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LatestSalaryPaid", HeaderText = "Latest Salary Paid", Width = 120 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(_txtFullName);
            Controls.Add(_txtPhone);
            Controls.Add(_txtEmail);
            Controls.Add(_txtAddress);
            Controls.Add(_cmbRole);
            Controls.Add(_numSalary);
            Controls.Add(_cmbShift);
            Controls.Add(_dtJoinDate);
            Controls.Add(_txtUsername);
            Controls.Add(_txtPassword);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(new Label { Text = "Search", Location = new Point(20, 130), AutoSize = true });
            Controls.Add(_txtSearch);
            Controls.Add(_grid);
        }

        private async Task LoadEmployeesAsync(string keyword = "")
        {
            try
            {
                _grid.DataSource = await _service.GetAllAsync(keyword);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("LoadEmployeesAsync", ex);
                MessageBox.Show(ex.Message, "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task SaveAsync(bool isUpdate)
        {
            try
            {
                Employee employee = new Employee
                {
                    EmployeeID = _selectedId,
                    FullName = _txtFullName.Text,
                    Phone = _txtPhone.Text,
                    Email = _txtEmail.Text,
                    Address = _txtAddress.Text,
                    Role = _cmbRole.Text,
                    Salary = _numSalary.Value,
                    Shift = _cmbShift.Text,
                    JoinDate = _dtJoinDate.Value.Date,
                    Username = _txtUsername.Text
                };

                bool ok = isUpdate
                    ? await _service.UpdateAsync(employee, _txtPassword.Text)
                    : await _service.AddAsync(new Employee
                    {
                        EmployeeID = employee.EmployeeID,
                        FullName = employee.FullName,
                        Phone = employee.Phone,
                        Email = employee.Email,
                        Address = employee.Address,
                        Role = employee.Role,
                        Salary = employee.Salary,
                        Shift = employee.Shift,
                        JoinDate = employee.JoinDate,
                        Username = employee.Username,
                        Password = _txtPassword.Text
                    });

                if (ok)
                {
                    await LoadEmployeesAsync(_txtSearch.Text);
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SaveEmployee", ex);
                MessageBox.Show(ex.Message, "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (_selectedId <= 0)
                {
                    MessageBox.Show("Select an employee first.");
                    return;
                }

                if (MessageBox.Show("Delete selected employee?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                await _service.DeleteAsync(_selectedId);
                await LoadEmployeesAsync(_txtSearch.Text);
                ClearInputs();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("DeleteEmployee", ex);
                MessageBox.Show(ex.Message, "Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Employee employee))
            {
                return;
            }

            _selectedId = employee.EmployeeID;
            _txtFullName.Text = employee.FullName;
            _txtPhone.Text = employee.Phone;
            _txtEmail.Text = employee.Email;
            _txtAddress.Text = employee.Address;
            _cmbRole.Text = employee.Role;
            _numSalary.Value = employee.Salary;
            _cmbShift.Text = employee.Shift;
            _dtJoinDate.Value = employee.JoinDate;
            _txtUsername.Text = employee.Username;
            _txtPassword.Clear();
        }

        private void ClearInputs()
        {
            _selectedId = 0;
            _txtFullName.Clear();
            _txtPhone.Clear();
            _txtEmail.Clear();
            _txtAddress.Clear();
            _cmbRole.SelectedIndex = -1;
            _numSalary.Value = 0;
            _cmbShift.SelectedIndex = -1;
            _dtJoinDate.Value = DateTime.Today;
            _txtUsername.Clear();
            _txtPassword.Clear();
        }
    }
}
