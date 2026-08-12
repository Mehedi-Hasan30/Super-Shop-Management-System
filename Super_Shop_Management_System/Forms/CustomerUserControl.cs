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
            // Premium header panel with icon and separator
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(960, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Customers, 32F, ThemeManager.Primary);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            var title = new Label
            {
                Text = "Customer Management",
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

            // Search section card
            var searchCard = new RoundedPanel
            {
                Location = new Point(20, 90),
                Size = new Size(920, 50),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(12)
            };

            _txtSearch = new TextBox
            {
                Text = "",
                Location = new Point(searchCard.Padding.Left, searchCard.Padding.Top),
                Size = new Size(876, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };
            _txtSearch.TextChanged += async (_, __) => await LoadCustomersAsync(_txtSearch.Text);
            searchCard.Controls.Add(_txtSearch);

            var btnAdd = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add", 80, 36);
            btnAdd.Location = new Point(searchCard.Padding.Left + 10 + 876 - 80, 10);
            btnAdd.Click += BtnAdd_Click;
            searchCard.Controls.Add(btnAdd);

            // Stats cards row
            var statsPanel = new Panel
            {
                Location = new Point(20, 150),
                Size = new Size(920, 60),
                BackColor = Color.Transparent
            };

            var lowStockCard = UIStyleKit.CreateStatCard("Active", "0", IconHelper.Glyphs.Customers, ThemeManager.Primary, 180, 50);
            lowStockCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(lowStockCard);

            var totalCard = UIStyleKit.CreateStatCard("Total", "0", IconHelper.Glyphs.Customers, ThemeManager.Info, 180, 50);
            totalCard.Location = new Point(210, 10);
            statsPanel.Controls.Add(totalCard);

            // Action buttons panel
            var actionsPanel = new Panel
            {
                Location = new Point(20, 220),
                Size = new Size(920, 40),
                BackColor = Color.Transparent
            };

            var btnAdd2 = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add Customer", 120, 36);
            btnAdd2.Location = new Point(20, 2);
            actionsPanel.Controls.Add(btnAdd2);

            var btnUpdate2 = UIStyleKit.CreateButton(ButtonStyle.Secondary, "Update", 100, 36);
            btnUpdate2.Location = new Point(150, 2);
            actionsPanel.Controls.Add(btnUpdate2);

            var btnDelete2 = UIStyleKit.CreateButton(ButtonStyle.Danger, "Delete", 100, 36);
            btnDelete2.Location = new Point(260, 2);
            actionsPanel.Controls.Add(btnDelete2);

            // Stock alert label
            var lblAlert = new Label
            {
                Text = "Active Customers: 0",
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.Primary,
                AutoSize = true,
                Location = new Point(370, 12)
            };

            // Grid area
            _grid = new DataGridView
            {
                Location = new Point(20, 270),
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

            Controls.Add(headerPanel);
            Controls.Add(searchCard);
            Controls.Add(statsPanel);
            Controls.Add(actionsPanel);
            Controls.Add(lblAlert);
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