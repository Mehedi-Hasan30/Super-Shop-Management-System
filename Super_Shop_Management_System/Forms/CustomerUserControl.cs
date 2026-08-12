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
    public class CustomerUserControl : UserControl
    {
        private readonly CustomerService _customerService = new CustomerService();

        private DataGridView _grid;
        private TextBox _txtFullName;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtAddress;
        private NumericUpDown _numPoints;
        private TextBox _txtSearch;
        private int _selectedId;

        public CustomerUserControl()
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
                Text = "Customer Management",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var panel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(900, 180),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _txtFullName = new TextBox { Location = new Point(20, 40), Width = 180 };
            _txtPhone = new TextBox { Location = new Point(210, 40), Width = 130 };
            _txtEmail = new TextBox { Location = new Point(350, 40), Width = 180 };
            _txtAddress = new TextBox { Location = new Point(540, 40), Width = 180 };
            _numPoints = new NumericUpDown { Location = new Point(730, 40), Width = 90, Maximum = 1000000 };

            Button btnAdd = new RoundedButton { Text = "Add", Location = new Point(840, 38), Width = 60, CornerRadius = 6 };
            btnAdd.Click += BtnAdd_Click;

            Button btnUpdate = new RoundedButton { Text = "Update", Location = new Point(905, 38), Width = 70, CornerRadius = 6 };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new RoundedButton { Text = "Delete", Location = new Point(905, 75), Width = 70, CornerRadius = 6 };
            btnDelete.Click += BtnDelete_Click;

            _txtSearch = new TextBox { Location = new Point(20, 105), Width = 300 };
            _txtSearch.TextChanged += async (_, __) => await LoadCustomersAsync(_txtSearch.Text);

            _grid = new DataGridView
            {
                Location = new Point(20, 140),
                Size = new Size(955, 450),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

DataGridStyler.ApplyModernStyle(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Customer", Width = 170 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Phone", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", HeaderText = "Address", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LoyaltyPoints", HeaderText = "Loyalty", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalPurchases", HeaderText = "Purchase History", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreatedDate", HeaderText = "Registered", Width = 120 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(title);
            Controls.Add(panel);
            Controls.Add(_grid);
        }

        private async void CustomerUserControl_Load(object sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync(string keyword = "")
        {
            using (var loading = new LoadingIndicator(this, "Loading customers..."))
            {
                try
                {
                    List<Customer> customers = await _customerService.GetAllAsync(keyword);
                    _grid.DataSource = customers;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to load customers: {ex.Message}", "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Customer customer))
            {
                return;
            }

            _selectedId = customer.CustomerID;
            _txtFullName.Text = customer.FullName;
            _txtPhone.Text = customer.Phone;
            _txtEmail.Text = customer.Email;
            _txtAddress.Text = customer.Address;
            _numPoints.Value = customer.LoyaltyPoints;
        }

        private void ClearInputs()
        {
            _selectedId = 0;
            _txtFullName.Clear();
            _txtPhone.Clear();
            _txtEmail.Clear();
            _txtAddress.Clear();
            _numPoints.Value = 0;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Customer customer = new Customer
                {
                    CustomerID = _selectedId,
                    FullName = _txtFullName.Text,
                    Phone = _txtPhone.Text,
                    Email = _txtEmail.Text,
                    Address = _txtAddress.Text,
                    LoyaltyPoints = Convert.ToInt32(_numPoints.Value)
                };

                bool ok = await _customerService.AddAsync(customer);
                if (ok)
                {
                    await LoadCustomersAsync(_txtSearch.Text);
                    ClearInputs();
                    ToastNotification.Show("Customer added successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                Customer customer = new Customer
                {
                    CustomerID = _selectedId,
                    FullName = _txtFullName.Text,
                    Phone = _txtPhone.Text,
                    Email = _txtEmail.Text,
                    Address = _txtAddress.Text,
                    LoyaltyPoints = Convert.ToInt32(_numPoints.Value)
                };

                bool ok = await _customerService.UpdateAsync(customer);
                if (ok)
                {
                    await LoadCustomersAsync(_txtSearch.Text);
                    ClearInputs();
                    ToastNotification.Show("Customer updated successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedId <= 0)
                {
                    MessageBox.Show("Select a customer first.", "Customer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("Delete selected customer?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                await _customerService.DeleteAsync(_selectedId);
                await LoadCustomersAsync(_txtSearch.Text);
                ClearInputs();
                ToastNotification.Show("Customer deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}