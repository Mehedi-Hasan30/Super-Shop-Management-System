using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class SupplierForm : Form
    {
        private readonly SupplierService _service = new SupplierService();
        private DataGridView _grid;
        private TextBox _txtName;
        private TextBox _txtCompany;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtAddress;
        private TextBox _txtProductType;
        private TextBox _txtSearch;
        private int _selectedId;

        public SupplierForm()
        {
            InitializeComponent();
            Load += async (_, __) => await LoadSuppliersAsync();
        }

        private void InitializeComponent()
        {
            Text = "Supplier Management";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1080, 640);

            _txtName = new TextBox { Location = new Point(20, 40), Width = 150 };
            _txtCompany = new TextBox { Location = new Point(180, 40), Width = 170 };
            _txtPhone = new TextBox { Location = new Point(360, 40), Width = 120 };
            _txtEmail = new TextBox { Location = new Point(490, 40), Width = 160 };
            _txtAddress = new TextBox { Location = new Point(660, 40), Width = 170 };
            _txtProductType = new TextBox { Location = new Point(840, 40), Width = 120 };

            Controls.Add(new Label { Text = "Supplier", Location = new Point(20, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Company", Location = new Point(180, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Phone", Location = new Point(360, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Email", Location = new Point(490, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Address", Location = new Point(660, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Product Type", Location = new Point(840, 20), AutoSize = true });

            Button btnAdd = new Button { Text = "Add", Location = new Point(970, 38), Width = 80 };
            Button btnUpdate = new Button { Text = "Update", Location = new Point(970, 72), Width = 80 };
            Button btnDelete = new Button { Text = "Delete", Location = new Point(970, 106), Width = 80 };

            btnAdd.Click += async (_, __) => await SaveAsync(false);
            btnUpdate.Click += async (_, __) => await SaveAsync(true);
            btnDelete.Click += async (_, __) => await DeleteAsync();

            _txtSearch = new TextBox { Location = new Point(20, 105), Width = 300 };
            _txtSearch.TextChanged += async (_, __) => await LoadSuppliersAsync(_txtSearch.Text);

            _grid = new DataGridView
            {
                Location = new Point(20, 140),
                Width = 940,
                Height = 450,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "Supplier", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CompanyName", HeaderText = "Company", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Phone", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", HeaderText = "Address", Width = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductType", HeaderText = "Product Type", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalTransactions", HeaderText = "Transactions", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreatedDate", HeaderText = "Created", Width = 120 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(_txtName);
            Controls.Add(_txtCompany);
            Controls.Add(_txtPhone);
            Controls.Add(_txtEmail);
            Controls.Add(_txtAddress);
            Controls.Add(_txtProductType);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(new Label { Text = "Search", Location = new Point(20, 85), AutoSize = true });
            Controls.Add(_txtSearch);
            Controls.Add(_grid);
        }

        private async Task LoadSuppliersAsync(string keyword = "")
        {
            try
            {
                _grid.DataSource = await _service.GetAllAsync(keyword);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("LoadSuppliersAsync", ex);
                MessageBox.Show(ex.Message, "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task SaveAsync(bool isUpdate)
        {
            try
            {
                Supplier supplier = new Supplier
                {
                    SupplierID = _selectedId,
                    SupplierName = _txtName.Text,
                    CompanyName = _txtCompany.Text,
                    Phone = _txtPhone.Text,
                    Email = _txtEmail.Text,
                    Address = _txtAddress.Text,
                    ProductType = _txtProductType.Text
                };

                bool ok = isUpdate ? await _service.UpdateAsync(supplier) : await _service.AddAsync(supplier);
                if (ok)
                {
                    await LoadSuppliersAsync(_txtSearch.Text);
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SaveSupplier", ex);
                MessageBox.Show(ex.Message, "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (_selectedId <= 0)
                {
                    MessageBox.Show("Select a supplier first.");
                    return;
                }

                if (MessageBox.Show("Delete selected supplier?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                await _service.DeleteAsync(_selectedId);
                await LoadSuppliersAsync(_txtSearch.Text);
                ClearInputs();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("DeleteSupplier", ex);
                MessageBox.Show(ex.Message, "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Supplier supplier))
            {
                return;
            }

            _selectedId = supplier.SupplierID;
            _txtName.Text = supplier.SupplierName;
            _txtCompany.Text = supplier.CompanyName;
            _txtPhone.Text = supplier.Phone;
            _txtEmail.Text = supplier.Email;
            _txtAddress.Text = supplier.Address;
            _txtProductType.Text = supplier.ProductType;
        }

        private void ClearInputs()
        {
            _selectedId = 0;
            _txtName.Clear();
            _txtCompany.Clear();
            _txtPhone.Clear();
            _txtEmail.Clear();
            _txtAddress.Clear();
            _txtProductType.Clear();
        }
    }
}
