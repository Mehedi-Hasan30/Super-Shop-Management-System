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
    public class SupplierUserControl : UserControl
    {
        private readonly SupplierService _supplierService = new SupplierService();

        private DataGridView _grid;
        private TextBox _txtName;
        private TextBox _txtCompany;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtAddress;
        private TextBox _txtProductType;
        private TextBox _txtSearch;

        public SupplierUserControl()
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
                Text = "Supplier Management",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var panel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(500, 200),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _txtName = new TextBox { Location = new Point(20, 40), Width = 300 };
            _txtCompany = new TextBox { Location = new Point(20, 90), Width = 300 };
            _txtPhone = new TextBox { Location = new Point(340, 40), Width = 150 };
            _txtEmail = new TextBox { Location = new Point(340, 90), Width = 150 };
            _txtAddress = new TextBox { Location = new Point(20, 140), Width = 470 };
            _txtProductType = new TextBox { Location = new Point(340, 140), Width = 150 };
            _txtSearch = new TextBox { Location = new Point(20, 170), Width = 300 };

            Button btnAdd = new RoundedButton { Text = "Add", Location = new Point(20, 200), Width = 80, CornerRadius = 6 };
            btnAdd.Click += BtnAdd_Click;

            Button btnUpdate = new RoundedButton { Text = "Update", Location = new Point(120, 200), Width = 80, CornerRadius = 6 };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new RoundedButton { Text = "Delete", Location = new Point(220, 200), Width = 80, CornerRadius = 6 };
            btnDelete.Click += BtnDelete_Click;

            _txtSearch.TextChanged += async (_, __) => await LoadSuppliersAsync();

            _grid = new DataGridView
            {
                Location = new Point(20, 250),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "Supplier Name", Width = 200 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CompanyName", HeaderText = "Company Name", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Phone", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", HeaderText = "Address", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductType", HeaderText = "Product Type", Width = 130 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(title);
            Controls.Add(panel);
            Controls.Add(_grid);
        }

        private async void SupplierUserControl_Load(object sender, EventArgs e)
        {
            await LoadSuppliersAsync();
        }

        private async Task LoadSuppliersAsync(string keyword = "")
        {
            try
            {
                List<Supplier> suppliers = await _supplierService.GetAllAsync(keyword);
                _grid.DataSource = suppliers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load suppliers: {ex.Message}", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Supplier supplier))
            {
                return;
            }

            _txtName.Text = supplier.SupplierName;
            _txtCompany.Text = supplier.CompanyName;
            _txtPhone.Text = supplier.Phone;
            _txtEmail.Text = supplier.Email;
            _txtAddress.Text = supplier.Address;
            _txtProductType.Text = supplier.ProductType ?? "";
        }

        private void ClearInputs()
        {
            _txtName.Clear();
            _txtCompany.Clear();
            _txtPhone.Clear();
            _txtEmail.Clear();
            _txtAddress.Clear();
            _txtProductType.Clear();
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_txtName.Text) || string.IsNullOrWhiteSpace(_txtPhone.Text))
                {
                    MessageBox.Show("Supplier name and phone are required.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Supplier supplier = new Supplier
                {
                    SupplierName = _txtName.Text,
                    CompanyName = _txtCompany.Text,
                    Phone = _txtPhone.Text,
                    Email = _txtEmail.Text,
                    Address = _txtAddress.Text,
                    ProductType = _txtProductType.Text
                };

                bool ok = await _supplierService.AddAsync(supplier);
                if (ok)
                {
                    await LoadSuppliersAsync(_txtSearch.Text);
                    ClearInputs();
                    MessageBox.Show("Supplier added successfully.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_grid.CurrentRow?.DataBoundItem is Supplier supplier) || supplier.SupplierID <= 0)
                {
                    MessageBox.Show("Select a supplier first.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                supplier.SupplierName = _txtName.Text;
                supplier.CompanyName = _txtCompany.Text;
                supplier.Phone = _txtPhone.Text;
                supplier.Email = _txtEmail.Text;
                supplier.Address = _txtAddress.Text;
                supplier.ProductType = _txtProductType.Text;

                bool ok = await _supplierService.UpdateAsync(supplier);
                if (ok)
                {
                    await LoadSuppliersAsync(_txtSearch.Text);
                    ClearInputs();
                    MessageBox.Show("Supplier updated successfully.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_grid.CurrentRow?.DataBoundItem is Supplier supplier) || supplier.SupplierID <= 0)
                {
                    MessageBox.Show("Select a supplier first.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("Delete selected supplier?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                await _supplierService.DeleteAsync(supplier.SupplierID);
                await LoadSuppliersAsync(_txtSearch.Text);
                ClearInputs();
                MessageBox.Show("Supplier deleted successfully.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}