using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class CustomerForm : Form
    {
        private readonly CustomerService _service = new CustomerService();
        private DataGridView _grid;
        private TextBox _txtFullName;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtAddress;
        private NumericUpDown _numPoints;
        private TextBox _txtSearch;
        private int _selectedId;

        public CustomerForm()
        {
            InitializeComponent();
            Load += async (_, __) => await LoadCustomersAsync();
        }

        private void InitializeComponent()
        {
            Text = "Customer Management";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 640);

            _txtFullName = new TextBox { Location = new Point(20, 40), Width = 180 };
            _txtPhone = new TextBox { Location = new Point(210, 40), Width = 130 };
            _txtEmail = new TextBox { Location = new Point(350, 40), Width = 180 };
            _txtAddress = new TextBox { Location = new Point(540, 40), Width = 180 };
            _numPoints = new NumericUpDown { Location = new Point(730, 40), Width = 90, Maximum = 1000000 };

            Controls.Add(new Label { Text = "Full Name", Location = new Point(20, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Phone", Location = new Point(210, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Email", Location = new Point(350, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Address", Location = new Point(540, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Loyalty", Location = new Point(730, 20), AutoSize = true });

            Button btnAdd = new Button { Text = "Add", Location = new Point(840, 38), Width = 60 };
            Button btnUpdate = new Button { Text = "Update", Location = new Point(905, 38), Width = 70 };
            Button btnDelete = new Button { Text = "Delete", Location = new Point(905, 75), Width = 70 };

            btnAdd.Click += async (_, __) => await SaveAsync(false);
            btnUpdate.Click += async (_, __) => await SaveAsync(true);
            btnDelete.Click += async (_, __) => await DeleteAsync();

            _txtSearch = new TextBox { Location = new Point(20, 105), Width = 300 };
            _txtSearch.TextChanged += async (_, __) => await LoadCustomersAsync(_txtSearch.Text);

            _grid = new DataGridView
            {
                Location = new Point(20, 140),
                Width = 955,
                Height = 450,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Customer", Width = 170 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Phone", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", HeaderText = "Address", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LoyaltyPoints", HeaderText = "Loyalty", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalPurchases", HeaderText = "Purchase History", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreatedDate", HeaderText = "Registered", Width = 120 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(_txtFullName);
            Controls.Add(_txtPhone);
            Controls.Add(_txtEmail);
            Controls.Add(_txtAddress);
            Controls.Add(_numPoints);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(new Label { Text = "Search", Location = new Point(20, 85), AutoSize = true });
            Controls.Add(_txtSearch);
            Controls.Add(_grid);
        }

        private async Task LoadCustomersAsync(string keyword = "")
        {
            try
            {
                _grid.DataSource = await _service.GetAllAsync(keyword);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("LoadCustomersAsync", ex);
                MessageBox.Show(ex.Message, "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task SaveAsync(bool isUpdate)
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

                bool ok = isUpdate ? await _service.UpdateAsync(customer) : await _service.AddAsync(customer);
                if (ok)
                {
                    await LoadCustomersAsync(_txtSearch.Text);
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SaveCustomer", ex);
                MessageBox.Show(ex.Message, "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (_selectedId <= 0)
                {
                    MessageBox.Show("Select a customer first.");
                    return;
                }

                if (MessageBox.Show("Delete selected customer?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                await _service.DeleteAsync(_selectedId);
                await LoadCustomersAsync(_txtSearch.Text);
                ClearInputs();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("DeleteCustomer", ex);
                MessageBox.Show(ex.Message, "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
    }
}
