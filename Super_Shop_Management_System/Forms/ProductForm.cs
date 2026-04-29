using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class ProductForm : Form
    {
        private readonly ProductService _productService = new ProductService();
        private readonly CategoryService _categoryService = new CategoryService();

        private DataGridView _grid;
        private TextBox _txtName;
        private TextBox _txtBarcode;
        private ComboBox _cmbCategory;
        private NumericUpDown _numPurchasePrice;
        private NumericUpDown _numSellingPrice;
        private NumericUpDown _numStock;
        private NumericUpDown _numReorder;
        private DateTimePicker _dtpExpiry;
        private TextBox _txtSupplier;
        private TextBox _txtDescription;
        private TextBox _txtSearch;
        private Label _lblStockAlert;

        private int _selectedProductId;

        public ProductForm()
        {
            InitializeComponent();
            Load += ProductForm_Load;
        }

        private void InitializeComponent()
        {
            Text = "Product Management";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1180, 700);

            Label lblName = new Label { Text = "Product Name", Location = new Point(20, 20), AutoSize = true };
            _txtName = new TextBox { Location = new Point(20, 40), Width = 180 };

            Label lblBarcode = new Label { Text = "Barcode", Location = new Point(210, 20), AutoSize = true };
            _txtBarcode = new TextBox { Location = new Point(210, 40), Width = 140 };

            Label lblCategory = new Label { Text = "Category", Location = new Point(360, 20), AutoSize = true };
            _cmbCategory = new ComboBox { Location = new Point(360, 40), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblPurchase = new Label { Text = "Purchase Price", Location = new Point(540, 20), AutoSize = true };
            _numPurchasePrice = new NumericUpDown { Location = new Point(540, 40), Width = 110, DecimalPlaces = 2, Maximum = 999999999, Minimum = 0 };

            Label lblSelling = new Label { Text = "Selling Price", Location = new Point(660, 20), AutoSize = true };
            _numSellingPrice = new NumericUpDown { Location = new Point(660, 40), Width = 110, DecimalPlaces = 2, Maximum = 999999999, Minimum = 0 };

            Label lblStock = new Label { Text = "Stock", Location = new Point(780, 20), AutoSize = true };
            _numStock = new NumericUpDown { Location = new Point(780, 40), Width = 75, Maximum = 1000000, Minimum = 0 };

            Label lblReorder = new Label { Text = "Reorder", Location = new Point(865, 20), AutoSize = true };
            _numReorder = new NumericUpDown { Location = new Point(865, 40), Width = 75, Maximum = 1000000, Minimum = 0 };

            Label lblExpiry = new Label { Text = "Expiry Date", Location = new Point(950, 20), AutoSize = true };
            _dtpExpiry = new DateTimePicker { Location = new Point(950, 40), Width = 170, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            Label lblSupplier = new Label { Text = "Supplier", Location = new Point(20, 75), AutoSize = true };
            _txtSupplier = new TextBox { Location = new Point(20, 95), Width = 260 };

            Label lblDescription = new Label { Text = "Description", Location = new Point(290, 75), AutoSize = true };
            _txtDescription = new TextBox { Location = new Point(290, 95), Width = 350 };

            Button btnAdd = new Button { Text = "Add", Location = new Point(660, 93), Width = 90 };
            btnAdd.Click += BtnAdd_Click;

            Button btnUpdate = new Button { Text = "Update", Location = new Point(760, 93), Width = 90 };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new Button { Text = "Delete", Location = new Point(860, 93), Width = 90 };
            btnDelete.Click += BtnDelete_Click;

            Label lblSearch = new Label { Text = "Search", Location = new Point(20, 135), AutoSize = true };
            _txtSearch = new TextBox { Location = new Point(20, 155), Width = 300 };
            _txtSearch.TextChanged += (_, __) => LoadProducts(_txtSearch.Text);

            _lblStockAlert = new Label
            {
                Text = "Low Stock Items: 0",
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(340, 158)
            };

            _grid = new DataGridView
            {
                Location = new Point(20, 190),
                Width = 1120,
                Height = 450,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product", Width = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Barcode", HeaderText = "Barcode", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryID", HeaderText = "Category ID", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PurchasePrice", HeaderText = "Purchase", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SellingPrice", HeaderText = "Selling", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StockQuantity", HeaderText = "Stock", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReorderLevel", HeaderText = "Reorder", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ExpiryDate", HeaderText = "Expiry", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SupplierName", HeaderText = "Supplier", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Description", Width = 180 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(lblName);
            Controls.Add(_txtName);
            Controls.Add(lblBarcode);
            Controls.Add(_txtBarcode);
            Controls.Add(lblCategory);
            Controls.Add(_cmbCategory);
            Controls.Add(lblPurchase);
            Controls.Add(_numPurchasePrice);
            Controls.Add(lblSelling);
            Controls.Add(_numSellingPrice);
            Controls.Add(lblStock);
            Controls.Add(_numStock);
            Controls.Add(lblReorder);
            Controls.Add(_numReorder);
            Controls.Add(lblExpiry);
            Controls.Add(_dtpExpiry);
            Controls.Add(lblSupplier);
            Controls.Add(_txtSupplier);
            Controls.Add(lblDescription);
            Controls.Add(_txtDescription);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(lblSearch);
            Controls.Add(_txtSearch);
            Controls.Add(_lblStockAlert);
            Controls.Add(_grid);
        }

        private void ProductForm_Load(object sender, EventArgs e)
        {
            LoadCategoryDropdown();
            LoadProducts();
        }

        private void LoadCategoryDropdown()
        {
            List<Category> categories = _categoryService.GetAll();
            _cmbCategory.DataSource = categories;
            _cmbCategory.DisplayMember = "CategoryName";
            _cmbCategory.ValueMember = "CategoryID";
        }

        private void LoadProducts(string search = "")
        {
            try
            {
                List<Product> products = _productService.GetAll(search);
                _grid.DataSource = products;
                int lowStock = 0;
                foreach (Product product in products)
                {
                    if (product.StockQuantity <= product.ReorderLevel)
                    {
                        lowStock++;
                    }
                }

                _lblStockAlert.Text = $"Low Stock Items: {lowStock}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load products: {ex.Message}", "Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            SaveProduct(false);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            SaveProduct(true);
        }

        private void SaveProduct(bool isUpdate)
        {
            try
            {
                Product product = new Product
                {
                    ProductID = _selectedProductId,
                    ProductName = _txtName.Text,
                    Barcode = _txtBarcode.Text,
                    CategoryID = _cmbCategory.SelectedValue == null ? 0 : Convert.ToInt32(_cmbCategory.SelectedValue),
                    PurchasePrice = _numPurchasePrice.Value,
                    SellingPrice = _numSellingPrice.Value,
                    StockQuantity = Convert.ToInt32(_numStock.Value),
                    ReorderLevel = Convert.ToInt32(_numReorder.Value),
                    ExpiryDate = _dtpExpiry.Checked ? _dtpExpiry.Value.Date : (DateTime?)null,
                    SupplierName = _txtSupplier.Text,
                    Description = _txtDescription.Text
                };

                bool success = isUpdate ? _productService.Update(product) : _productService.Add(product);
                if (success)
                {
                    LoadProducts(_txtSearch.Text);
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedProductId <= 0)
                {
                    MessageBox.Show("Select a product first.", "Product", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult result = MessageBox.Show("Delete selected product?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                bool success = _productService.Delete(_selectedProductId);
                if (success)
                {
                    LoadProducts(_txtSearch.Text);
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Product product))
            {
                return;
            }

            _selectedProductId = product.ProductID;
            _txtName.Text = product.ProductName;
            _txtBarcode.Text = product.Barcode;
            _cmbCategory.SelectedValue = product.CategoryID;
            _numPurchasePrice.Value = product.PurchasePrice;
            _numSellingPrice.Value = product.SellingPrice;
            _numStock.Value = product.StockQuantity;
            _numReorder.Value = product.ReorderLevel;
            _txtSupplier.Text = product.SupplierName;
            _txtDescription.Text = product.Description;

            if (product.ExpiryDate.HasValue)
            {
                _dtpExpiry.Checked = true;
                _dtpExpiry.Value = product.ExpiryDate.Value;
            }
            else
            {
                _dtpExpiry.Checked = false;
            }
        }

        private void ClearInputs()
        {
            _selectedProductId = 0;
            _txtName.Clear();
            _txtBarcode.Clear();
            _numPurchasePrice.Value = 0;
            _numSellingPrice.Value = 0;
            _numStock.Value = 0;
            _numReorder.Value = 0;
            _dtpExpiry.Checked = false;
            _txtSupplier.Clear();
            _txtDescription.Clear();
        }
    }
}
