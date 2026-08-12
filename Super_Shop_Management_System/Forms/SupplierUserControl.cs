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
            // Premium header panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Suppliers, 32F, ThemeManager.Primary);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            var title = new Label
            {
                Text = "Supplier Management",
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
                Size = new Size(520, 160),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(16)
            };

            _txtName = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, inputCard.Padding.Top),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtCompany = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtName.Bottom + 12),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtPhone = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtCompany.Bottom + 12),
                Size = new Size(130, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtEmail = new TextBox
            {
                Location = new Point(inputCard.Padding.Left + 150, _txtCompany.Bottom + 12),
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

            _txtProductType = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtAddress.Bottom + 12),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            var btnAdd = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add", 80, 36);
            btnAdd.Location = new Point(inputCard.Padding.Left, 20);
            btnAdd.Click += BtnAdd_Click;
            inputCard.Controls.Add(_txtName);
            inputCard.Controls.Add(_txtCompany);
            inputCard.Controls.Add(_txtPhone);
            inputCard.Controls.Add(_txtEmail);
            inputCard.Controls.Add(_txtAddress);
            inputCard.Controls.Add(_txtProductType);
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
                Location = new Point(20, 260),
                Size = new Size(520, 50),
                BackColor = Color.Transparent
            };

            var totalCard = UIStyleKit.CreateStatCard("Suppliers", "0", IconHelper.Glyphs.Suppliers, ThemeManager.Success, 180, 40);
            totalCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(totalCard);

            var activeCard = UIStyleKit.CreateStatCard("Active", "0", IconHelper.Glyphs.Suppliers, ThemeManager.Primary, 180, 40);
            activeCard.Location = new Point(210, 10);
            statsPanel.Controls.Add(activeCard);

            // Grid area
            _grid = new DataGridView
            {
                Location = new Point(20, 320),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            DataGridStyler.ApplyModernStyle(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "Supplier Name", Width = 200 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CompanyName", HeaderText = "Company Name", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Phone", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Width = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", HeaderText = "Address", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductType", HeaderText = "Product Type", Width = 130 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(headerPanel);
            Controls.Add(inputCard);
            Controls.Add(statsPanel);
            Controls.Add(_grid);
        }

        private async void SupplierUserControl_Load(object sender, EventArgs e)
        {
            await LoadSuppliersAsync();
        }

        private async Task LoadSuppliersAsync(string keyword = "")
        {
            using (var loading = new LoadingIndicator(this, "Loading suppliers..."))
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
                    ToastNotification.Show("Supplier added successfully.");
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
                    ToastNotification.Show("Supplier updated successfully.");
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
                ToastNotification.Show("Supplier deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}