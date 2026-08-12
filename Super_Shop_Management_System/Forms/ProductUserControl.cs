using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class ProductUserControl : UserControl
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

public ProductUserControl()
        {
            Dock = DockStyle.Fill;
            BackColor = ThemeManager.Background;
            Tag = ThemeManager.ThemeExemptTag;

            InitializeUserControl();

            Load += ProductUserControl_Load;
        }

        private void InitializeUserControl()
        {
            // Premium header with icon and separator
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1160, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            // Icon label
            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Products, 32F, ThemeManager.Primary);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            // Title
            var title = new Label
            {
                Text = "Product Management",
                Font = ThemeManager.FontH2,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(60, 25)
            };
            headerPanel.Controls.Add(title);

            // Separator line
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
                Size = new Size(1120, 50),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(12)
            };

            _txtSearch = new TextBox
            {
                Text = "",
                Location = new Point(searchCard.Padding.Left, searchCard.Padding.Top),
                Size = new Size(1076, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };
            _txtSearch.TextChanged += (_, __) => LoadProducts(_txtSearch.Text);
            searchCard.Controls.Add(_txtSearch);

            // Add button with UIStyleKit
            var btnSearchAdd = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add Product", 100, 36);
            btnSearchAdd.Location = new Point(searchCard.Padding.Left + 10 + 1076 - 100, 10);
            btnSearchAdd.Click += BtnAdd_Click;
            searchCard.Controls.Add(btnSearchAdd);

            // Stats cards row
            var statsPanel = new Panel
            {
                Location = new Point(20, 150),
                Size = new Size(1120, 80),
                BackColor = Color.Transparent
            };

            // Low Stock stat card
            var lowStockCard = UIStyleKit.CreateStatCard("Low Stock", "0", IconHelper.Glyphs.Alert, ThemeManager.Warning, 200, 60);
            lowStockCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(lowStockCard);

            // Total Products stat card
            var totalProductsCard = UIStyleKit.CreateStatCard("Total Products", "0", IconHelper.Glyphs.Products, ThemeManager.Primary, 200, 60);
            totalProductsCard.Location = new Point(230, 10);
            statsPanel.Controls.Add(totalProductsCard);

            // Category count card
            var categoryCard = UIStyleKit.CreateStatCard("Categories", "0", IconHelper.Glyphs.Inventory, ThemeManager.Info, 200, 60);
            categoryCard.Location = new Point(440, 10);
            statsPanel.Controls.Add(categoryCard);

            // Supplier count card
            var supplierCard = UIStyleKit.CreateStatCard("Suppliers", "0", IconHelper.Glyphs.Suppliers, ThemeManager.Success, 200, 60);
            supplierCard.Location = new Point(650, 10);
            statsPanel.Controls.Add(supplierCard);

            // Stock alert label
            _lblStockAlert = new Label
            {
                Text = $"Low Stock Items: 0",
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.Warning,
                AutoSize = true,
                Location = new Point(860, 25)
            };

            Controls.Add(headerPanel);
            Controls.Add(searchCard);
            Controls.Add(statsPanel);
            Controls.Add(_lblStockAlert);

            // Form fields panel (original)
            var panel = new Panel
            {
                Location = new Point(20, 240),
                Size = new Size(1120, 200),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            // Left side - input fields
            _txtName = new TextBox { Location = new Point(20, 40), Width = 180 };
            _txtBarcode = new TextBox { Location = new Point(210, 40), Width = 140 };
            _cmbCategory = new ComboBox { Location = new Point(360, 40), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
            _numPurchasePrice = new NumericUpDown { Location = new Point(540, 40), Width = 110, DecimalPlaces = 2, Maximum = 999999999, Minimum = 0 };
            _numSellingPrice = new NumericUpDown { Location = new Point(660, 40), Width = 110, DecimalPlaces = 2, Maximum = 999999999, Minimum = 0 };
            _numStock = new NumericUpDown { Location = new Point(780, 40), Width = 75, Maximum = 1000000, Minimum = 0 };
            _numReorder = new NumericUpDown { Location = new Point(865, 40), Width = 75, Maximum = 1000000, Minimum = 0 };
            _dtpExpiry = new DateTimePicker { Location = new Point(950, 40), Width = 170, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _txtSupplier = new TextBox { Location = new Point(20, 100), Width = 260 };
            _txtDescription = new TextBox { Location = new Point(290, 100), Width = 350 };

            // Right side - buttons and grid
            Button btnAdd = new RoundedButton { Text = "Add", Location = new Point(660, 150), Width = 90, CornerRadius = 6 };
            btnAdd.Click += BtnAdd_Click;

            Button btnUpdate = new RoundedButton { Text = "Update", Location = new Point(760, 150), Width = 90, CornerRadius = 6 };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new RoundedButton { Text = "Delete", Location = new Point(860, 150), Width = 90, CornerRadius = 6 };
            btnDelete.Click += BtnDelete_Click;

            _txtSearch = new TextBox { Location = new Point(20, 160), Width = 300 };
            _txtSearch.TextChanged += (_, __) => LoadProducts(_txtSearch.Text);

            _lblStockAlert = new Label
            {
                Text = "Low Stock Items: 0",
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(340, 180)
            };

            // Category dropdown loading
            LoadCategoryDropdown();

            panel.Controls.Add(_txtName);
            panel.Controls.Add(new Label { Text = "Product Name", Location = new Point(20, 20), AutoSize = true });
            panel.Controls.Add(_txtBarcode);
            panel.Controls.Add(new Label { Text = "Barcode", Location = new Point(210, 20), AutoSize = true });
            panel.Controls.Add(_cmbCategory);
            panel.Controls.Add(new Label { Text = "Category", Location = new Point(360, 20), AutoSize = true });
            panel.Controls.Add(_numPurchasePrice);
            panel.Controls.Add(new Label { Text = "Purchase Price", Location = new Point(540, 20), AutoSize = true });
            panel.Controls.Add(_numSellingPrice);
            panel.Controls.Add(new Label { Text = "Selling Price", Location = new Point(660, 20), AutoSize = true });
            panel.Controls.Add(_numStock);
            panel.Controls.Add(new Label { Text = "Stock", Location = new Point(780, 20), AutoSize = true });
            panel.Controls.Add(_numReorder);
            panel.Controls.Add(new Label { Text = "Reorder", Location = new Point(865, 20), AutoSize = true });
            panel.Controls.Add(_dtpExpiry);
            panel.Controls.Add(new Label { Text = "Expiry Date", Location = new Point(950, 20), AutoSize = true });
            panel.Controls.Add(_txtSupplier);
            panel.Controls.Add(new Label { Text = "Supplier", Location = new Point(20, 75), AutoSize = true });
            panel.Controls.Add(_txtDescription);
            panel.Controls.Add(new Label { Text = "Description", Location = new Point(290, 75), AutoSize = true });
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnUpdate);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(_txtSearch);
            panel.Controls.Add(_lblStockAlert);

            // Grid area
            _grid = new DataGridView
            {
                Location = new Point(20, 280),
                Size = new Size(1120, 400),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

DataGridStyler.ApplyModernStyle(_grid);
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

            Controls.Add(title);
            Controls.Add(panel);
            Controls.Add(_grid);
        }

        private void ProductUserControl_Load(object sender, EventArgs e)
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
            using (var loading = new LoadingIndicator(this, "Loading products..."))
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
            _dtpExpiry.Checked = product.ExpiryDate.HasValue;
            _dtpExpiry.Value = product.ExpiryDate.Value;
            _txtSupplier.Text = product.SupplierName;
            _txtDescription.Text = product.Description;
        }

        private void ClearInputs()
        {
            _selectedProductId = 0;
            _txtName.Clear();
            _txtBarcode.Clear();
            _cmbCategory.SelectedValue = 0;
            _numPurchasePrice.Value = 0;
            _numSellingPrice.Value = 0;
            _numStock.Value = 0;
            _numReorder.Value = 0;
            _dtpExpiry.Checked = false;
            _txtSupplier.Clear();
            _txtDescription.Clear();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Product product = new Product
                {
                    ProductID = _selectedProductId,
                    ProductName = _txtName.Text,
                    Barcode = _txtBarcode.Text,
                    CategoryID = _cmbCategory.SelectedValue == null || _cmbCategory.SelectedValue == DBNull.Value ? 0 : Convert.ToInt32(_cmbCategory.SelectedValue),
                    PurchasePrice = _numPurchasePrice.Value,
                    SellingPrice = _numSellingPrice.Value,
                    StockQuantity = Convert.ToInt32(_numStock.Value),
                    ReorderLevel = Convert.ToInt32(_numReorder.Value),
                    ExpiryDate = _dtpExpiry.Checked ? _dtpExpiry.Value.Date : (DateTime?)null,
                    SupplierName = _txtSupplier.Text,
                    Description = _txtDescription.Text
                };

                bool success = _productService.Add(product);
                if (success)
                {
                    LoadProducts(_txtSearch.Text);
                    ClearInputs();
                    ToastNotification.Show("Product added successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                Product product = new Product
                {
                    ProductID = _selectedProductId,
                    ProductName = _txtName.Text,
                    Barcode = _txtBarcode.Text,
                    CategoryID = _cmbCategory.SelectedValue == null || _cmbCategory.SelectedValue == DBNull.Value ? 0 : Convert.ToInt32(_cmbCategory.SelectedValue),
                    PurchasePrice = _numPurchasePrice.Value,
                    SellingPrice = _numSellingPrice.Value,
                    StockQuantity = Convert.ToInt32(_numStock.Value),
                    ReorderLevel = Convert.ToInt32(_numReorder.Value),
                    ExpiryDate = _dtpExpiry.Checked ? _dtpExpiry.Value.Date : (DateTime?)null,
                    SupplierName = _txtSupplier.Text,
                    Description = _txtDescription.Text
                };

                bool success = _productService.Update(product);
                if (success)
                {
                    LoadProducts(_txtSearch.Text);
                    ClearInputs();
                    ToastNotification.Show("Product updated successfully.");
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
                    ToastNotification.Show("Product deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}