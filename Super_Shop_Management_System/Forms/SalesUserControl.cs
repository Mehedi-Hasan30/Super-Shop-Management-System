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
    public class SalesUserControl : UserControl
    {
        private readonly PosService _posService = new PosService();
        private readonly SalesService _salesService = new SalesService();

        private DataGridView _gridProducts;
        private DataGridView _gridCart;
        private NumericUpDown _numQuantity;
        private NumericUpDown _numDiscount;
        private NumericUpDown _numVat;
        private ComboBox _cmbPaymentMethod;
        private ComboBox _cmbPaymentStatus;
        private NumericUpDown _numPaymentAmount;
        private Label _lblPaymentAmount;
        private Label _lblChange;
        private Label _lblPaymentStatus;
        private Label _lblCartTotal;
        private Label _lblDiscount;
        private Label _lblVat;
        private Label _lblGrandTotal;
        private Label _lblBarcode;
        private TextBox _txtBarcode;
        private TextBox _txtSearch;

        private List<Product> _products = new List<Product>();

        public SalesUserControl()
        {
            Dock = DockStyle.Fill;
            BackColor = ThemeManager.Background;
            Tag = ThemeManager.ThemeExemptTag;

            InitializeComponent();

            Load += SalesUserControl_Load;
        }

private void InitializeComponent()
        {
            // Premium header panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1200, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Sales, 32F, ThemeManager.Primary);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            var title = new Label
            {
                Text = "POS Sales",
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

            // Search panel card
            var searchPanel = new RoundedPanel
            {
                Location = new Point(20, 90),
                Size = new Size(1160, 50),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(12)
            };

            _txtSearch = new TextBox
            {
                Text = "",
                Location = new Point(searchPanel.Padding.Left, 10),
                Size = new Size(920, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };
            _txtSearch.TextChanged += async (_, __) => await LoadProductsAsync(_txtSearch.Text);
            searchPanel.Controls.Add(_txtSearch);

            var btnAddProduct = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add Product", 120, 36);
            btnAddProduct.Location = new Point(searchPanel.Padding.Left + 930, 10);
            btnAddProduct.Click += BtnAddToCart_Click;
            searchPanel.Controls.Add(btnAddProduct);

            // KPI stats cards row
            var statsPanel = new Panel
            {
                Location = new Point(20, 150),
                Size = new Size(1160, 60),
                BackColor = Color.Transparent
            };

            // Cart total stat
            var cartTotalCard = UIStyleKit.CreateStatCard("Cart Total", "0.00", IconHelper.Glyphs.Sales, ThemeManager.Primary, 200, 50);
            cartTotalCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(cartTotalCard);

            // Discount stat
            var discountCard = UIStyleKit.CreateStatCard("Discount", "0.00", IconHelper.Glyphs.Alert, ThemeManager.Warning, 200, 50);
            discountCard.Location = new Point(230, 10);
            statsPanel.Controls.Add(discountCard);

            // VAT stat
            var vatCard = UIStyleKit.CreateStatCard("VAT", "0%", IconHelper.Glyphs.Info, ThemeManager.Info, 200, 50);
            vatCard.Location = new Point(440, 10);
            statsPanel.Controls.Add(vatCard);

            // Grid area - Products (left)
            _gridProducts = new DataGridView
            {
                Location = new Point(20, 220),
                Size = new Size(460, 400),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            DataGridStyler.ApplyModernStyle(_gridProducts);
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductID", HeaderText = "ID", Width = 50 });
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product", Width = 180 });
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Barcode", HeaderText = "Barcode", Width = 120 });
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SellingPrice", HeaderText = "Price", Width = 90 });

            // Grid area - Cart (middle)
            _gridCart = new DataGridView
            {
                Location = new Point(500, 220),
                Size = new Size(420, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            DataGridStyler.ApplyModernStyle(_gridCart);
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product", Width = 220 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Qty", Width = 80 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", Width = 120 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SubTotal", HeaderText = "Sub Total", Width = 140 });

            // Cart actions panel
            var cartActionsPanel = new Panel
            {
                Location = new Point(500, 530),
                Size = new Size(420, 50),
                BackColor = Color.Transparent
            };

            var btnAddToCart = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add to Cart", 140, 36);
            btnAddToCart.Location = new Point(10, 5);
            cartActionsPanel.Controls.Add(btnAddToCart);

            var btnRemove = UIStyleKit.CreateButton(ButtonStyle.Danger, "Remove", 100, 36);
            btnRemove.Location = new Point(160, 5);
            cartActionsPanel.Controls.Add(btnRemove);

            var btnClearCart = UIStyleKit.CreateButton(ButtonStyle.Secondary, "Clear Cart", 100, 36);
            btnClearCart.Location = new Point(270, 5);
            cartActionsPanel.Controls.Add(btnClearCart);

            // Right panel - Payment summary
            var rightPanel = new Panel
            {
                Location = new Point(940, 220),
                Size = new Size(240, 350),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(16)
            };

            _numDiscount = new NumericUpDown { Location = new Point(16, 20), Width = 220, DecimalPlaces = 2, Maximum = 999999999, Minimum = 0 };
            _numDiscount.ValueChanged += (_, __) => RefreshSummary();

            _numVat = new NumericUpDown { Location = new Point(16, 70), Width = 220, DecimalPlaces = 2, Maximum = 100, Minimum = 0, Value = 5 };
            _numVat.ValueChanged += (_, __) => RefreshSummary();

            _lblCartTotal = new Label { Location = new Point(16, 120), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblDiscount = new Label { Location = new Point(16, 150), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblVat = new Label { Location = new Point(16, 180), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblGrandTotal = new Label { Location = new Point(16, 210), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = ThemeManager.Primary };

            _cmbPaymentMethod = new ComboBox
            {
                Location = new Point(16, 250),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "Card", "Mobile Banking" });
            _cmbPaymentMethod.SelectedIndex = -1;
            _cmbPaymentMethod.SelectedIndexChanged += (_, __) => RefreshPaymentUi();

            _lblPaymentStatus = new Label { Location = new Point(16, 290), AutoSize = true };
            _cmbPaymentStatus = new ComboBox
            {
                Location = new Point(16, 310),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentStatus.Items.AddRange(new object[] { "Paid", "Pending" });
            _cmbPaymentStatus.SelectedIndex = 0;
            _cmbPaymentStatus.SelectedIndexChanged += (_, __) => RefreshPaymentUi();

            _lblPaymentAmount = new Label { Location = new Point(16, 350), AutoSize = true };
            _numPaymentAmount = new NumericUpDown
            {
                Location = new Point(16, 370),
                Width = 220,
                DecimalPlaces = 2,
                Maximum = 999999999,
                Minimum = 0,
                Value = 0
            };
            _numPaymentAmount.ValueChanged += (_, __) => RefreshPaymentUi();

            _lblChange = new Label { Location = new Point(16, 410), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            var btnCheckout = UIStyleKit.CreateButton(ButtonStyle.Primary, "Checkout", 200, 40);
            btnCheckout.Location = new Point(20, 440);
            btnCheckout.Click += BtnCheckout_ClickAsync;

            rightPanel.Controls.Add(_numDiscount);
            rightPanel.Controls.Add(new Label { Text = "Discount Amount", Location = new Point(16, 5), AutoSize = true });
            rightPanel.Controls.Add(_lblCartTotal);
            rightPanel.Controls.Add(_lblDiscount);
            rightPanel.Controls.Add(new Label { Text = "VAT (%)", Location = new Point(16, 75), AutoSize = true });
            rightPanel.Controls.Add(_numVat);
            rightPanel.Controls.Add(_lblGrandTotal);
            rightPanel.Controls.Add(btnCheckout);
            rightPanel.Controls.Add(_lblPaymentStatus);
            rightPanel.Controls.Add(_cmbPaymentStatus);
            rightPanel.Controls.Add(new Label { Text = "Payment Status", Location = new Point(16, 285), AutoSize = true });
            rightPanel.Controls.Add(_lblPaymentAmount);
            rightPanel.Controls.Add(_numPaymentAmount);
            rightPanel.Controls.Add(new Label { Text = "Payment Amount", Location = new Point(16, 355), AutoSize = true });
            rightPanel.Controls.Add(_numPaymentAmount);
            rightPanel.Controls.Add(_lblChange);

            Controls.Add(headerPanel);
            Controls.Add(searchPanel);
            Controls.Add(statsPanel);
            Controls.Add(_gridProducts);
            Controls.Add(_gridCart);
            Controls.Add(rightPanel);
        }

        private async void SalesUserControl_Load(object sender, EventArgs e)
        {
            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync(string keyword = "")
        {
            try
            {
                _products = await _posService.GetProductsAsync(keyword);
                _gridProducts.DataSource = null;
                _gridProducts.DataSource = _products;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SalesUserControl.LoadProductsAsync", ex);
                MessageBox.Show($"Failed to load products: {ex.Message}", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            AddBarcodeToCartAsync().GetAwaiter().GetResult();
        }

        private async System.Threading.Tasks.Task AddBarcodeToCartAsync()
        {
            string barcode = _txtBarcode.Text?.Trim();
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return;
            }

            _txtBarcode.Enabled = false;
            try
            {
                Product product = await _posService.GetProductByBarcodeAsync(barcode);
                if (product == null)
                {
                    MessageBox.Show("Product not found for the scanned barcode.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtBarcode.SelectAll();
                    return;
                }

                _posService.AddToCart(product, 1);
                RefreshCart();
                _txtBarcode.Clear();
                _txtBarcode.Focus();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SalesUserControl.AddBarcodeToCartAsync", ex);
                MessageBox.Show(ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtBarcode.SelectAll();
            }
            finally
            {
                _txtBarcode.Enabled = true;
            }
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_gridProducts.CurrentRow?.DataBoundItem is Product selectedProduct))
                {
                    MessageBox.Show("Select a product first.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _posService.AddToCart(selectedProduct, Convert.ToInt32(_numQuantity.Value));
                RefreshCart();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SalesUserControl.BtnAddToCart_Click", ex);
                MessageBox.Show(ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (!(_gridCart.CurrentRow?.DataBoundItem is CartItem cartItem))
            {
                MessageBox.Show("Select a cart item to remove.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _posService.RemoveFromCart(cartItem.ProductID);
            RefreshCart();
        }

        private void RefreshCart()
        {
            _gridCart.DataSource = null;
            _gridCart.DataSource = _posService.GetCartItems();
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            PosTotals totals = _posService.CalculateTotals(_numDiscount.Value, _numVat.Value);
            _lblCartTotal.Text = $"Cart Total: {totals.CartTotal:C}";
            _lblDiscount.Text = $"Discount: {totals.DiscountAmount:C}";
            _lblVat.Text = $"VAT ({totals.VatPercentage:0.##}%): {totals.VatAmount:C}";
            _lblGrandTotal.Text = $"Grand Total: {totals.GrandTotal:C}";

            RefreshPaymentUi();
        }

        private void RefreshPaymentUi()
        {
            var selected = (PaymentMethod?)_cmbPaymentMethod.SelectedItem;
            var selectedStatus = (SalePaymentStatus?)_cmbPaymentStatus.SelectedItem;
            bool isCash = selected == PaymentMethod.Cash;

            _lblPaymentAmount.Visible = isCash;
            _numPaymentAmount.Visible = isCash;

            PosTotals totals = _posService.CalculateTotals(_numDiscount.Value, _numVat.Value);
            decimal grandTotal = totals.GrandTotal;

            if (!isCash)
            {
                _lblChange.ForeColor = Color.FromArgb(0, 0, 0);
                _lblChange.Text = "Change: -";
                return;
            }

            decimal paymentAmount = _numPaymentAmount.Value;
            decimal change = paymentAmount - grandTotal;
            _lblChange.ForeColor = change < 0 ? Color.IndianRed : ThemeManager.Primary;

            if (selectedStatus == SalePaymentStatus.Pending)
            {
                _lblChange.Text = change >= 0 ? $"Change: {change:C} (Pending)" : $"Short: {Math.Abs(change):C} (Pending)";
                return;
            }

            _lblChange.Text = $"Change: {change:C}";
        }

        private async void BtnCheckout_ClickAsync(object sender, EventArgs e)
        {
try
        {
            var cartItems = _posService.GetCartItems();
            if (cartItems == null || cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedPaymentIndex = _cmbPaymentMethod.SelectedIndex;
            var selectedStatusIndex = _cmbPaymentStatus.SelectedIndex;

            if (selectedPaymentIndex < 0 || selectedStatusIndex < 0)
            {
                MessageBox.Show("Please select a payment method and payment status.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedPayment = (PaymentMethod)_cmbPaymentMethod.SelectedItem;
            var selectedStatus = (SalePaymentStatus)_cmbPaymentStatus.SelectedItem;

            PosTotals totals = _posService.CalculateTotals(_numDiscount.Value, _numVat.Value);
            decimal grandTotal = totals.GrandTotal;
            if (grandTotal <= 0)
            {
                MessageBox.Show("Grand total must be greater than zero.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal paymentAmount = selectedPayment == PaymentMethod.Cash
                ? _numPaymentAmount.Value
                : grandTotal;

            int saleId = await _salesService.SaveSaleAsync(
                cartItems,
                totals,
                selectedPayment,
                paymentAmount,
                selectedStatus,
                customerId: null);

                _posService.ClearCart();
                _numPaymentAmount.Value = 0;
                _cmbPaymentMethod.SelectedIndex = -1;
                RefreshCart();

                ToastNotification.Show($"Sale saved successfully. SaleID: {saleId}");
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SalesUserControl.BtnCheckout_ClickAsync", ex);
                MessageBox.Show(ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}