using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;
using Super_Shop_Management_System.Helpers;

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
            var title = new Label
            {
                Text = "POS Sales",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            // Left panel - Product search and grid
            var leftPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(520, 550),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _txtSearch = new TextBox { Location = new Point(12, 30), Width = 300 };
            _txtSearch.TextChanged += async (_, __) => await LoadProductsAsync(_txtSearch.Text);

            Button btnSearch = new RoundedButton { Text = "Search", Location = new Point(320, 28), Width = 80, CornerRadius = 6 };
            btnSearch.Click += async (_, __) => await LoadProductsAsync(_txtSearch.Text);

            _lblBarcode = new Label { Text = "Barcode (Scanner)", Location = new Point(12, 55), AutoSize = true };
            _txtBarcode = new TextBox { Location = new Point(12, 75), Width = 300 };
            _txtBarcode.KeyDown += TxtBarcode_KeyDown;

_gridProducts = new DataGridView
                {
                    Location = new Point(12, 105),
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

            leftPanel.Controls.Add(_lblBarcode);
            leftPanel.Controls.Add(_txtBarcode);
            leftPanel.Controls.Add(_gridProducts);
            leftPanel.Controls.Add(_txtSearch);
            leftPanel.Controls.Add(btnSearch);

            // Center panel - Cart
            var centerPanel = new Panel
            {
                Location = new Point(560, 60),
                Size = new Size(520, 550),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _gridCart = new DataGridView
            {
                Size = new Size(490, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product", Width = 220 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Qty", Width = 80 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", Width = 120 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SubTotal", HeaderText = "Sub Total", Width = 140 });

            _numQuantity = new NumericUpDown { Location = new Point(12, 645), Width = 120, Minimum = 1, Maximum = 10000, Value = 1 };
            Button btnAddToCart = new RoundedButton { Text = "Add To Cart", Location = new Point(145, 642), Width = 140, CornerRadius = 6 };
            btnAddToCart.Click += BtnAddToCart_Click;

            Button btnRemove = new RoundedButton { Text = "Remove", Location = new Point(12, 630), Width = 110, CornerRadius = 6 };
            btnRemove.Click += BtnRemove_Click;

            Button btnClear = new RoundedButton { Text = "Clear Cart", Location = new Point(130, 630), Width = 110, CornerRadius = 6 };
            btnClear.Click += (_, __) => { _posService.ClearCart(); RefreshCart(); };

            centerPanel.Controls.Add(_gridCart);
            centerPanel.Controls.Add(btnRemove);
            centerPanel.Controls.Add(btnClear);
            centerPanel.Controls.Add(_numQuantity);
            centerPanel.Controls.Add(btnAddToCart);
            centerPanel.Controls.Add(new Label { Text = "Quantity", Location = new Point(12, 625), AutoSize = true });

            // Right panel - Payment summary
            var rightPanel = new Panel
            {
                Location = new Point(1100, 60),
                Size = new Size(320, 550),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _numDiscount = new NumericUpDown { Location = new Point(12, 170), Width = 280, DecimalPlaces = 2, Maximum = 999999999, Minimum = 0 };
            _numDiscount.ValueChanged += (_, __) => RefreshSummary();

            _numVat = new NumericUpDown { Location = new Point(12, 235), Width = 280, DecimalPlaces = 2, Maximum = 100, Minimum = 0, Value = 5 };
            _numVat.ValueChanged += (_, __) => RefreshSummary();

            _lblCartTotal = new Label { Location = new Point(12, 340), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblDiscount = new Label { Location = new Point(12, 370), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblVat = new Label { Location = new Point(12, 400), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblGrandTotal = new Label { Location = new Point(12, 430), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = ThemeManager.Primary };

            _cmbPaymentMethod = new ComboBox
            {
                Location = new Point(12, 80),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "Card", "Mobile Banking" });
            _cmbPaymentMethod.SelectedIndex = -1;
            _cmbPaymentMethod.SelectedIndexChanged += (_, __) => RefreshPaymentUi();

            _lblPaymentStatus = new Label { Text = "Payment Status", Location = new Point(12, 125), AutoSize = true };
            _cmbPaymentStatus = new ComboBox
            {
                Location = new Point(12, 145),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentStatus.Items.AddRange(new object[] { "Paid", "Pending" });
            _cmbPaymentStatus.SelectedIndex = 0;
            _cmbPaymentStatus.SelectedIndexChanged += (_, __) => RefreshPaymentUi();

            _lblPaymentAmount = new Label { Text = "Payment Amount", Location = new Point(12, 170), AutoSize = true };
            _numPaymentAmount = new NumericUpDown
            {
                Location = new Point(12, 190),
                Width = 280,
                DecimalPlaces = 2,
                Maximum = 999999999,
                Minimum = 0,
                Value = 0
            };
            _numPaymentAmount.ValueChanged += (_, __) => RefreshPaymentUi();

            _lblChange = new Label { Text = "Change: -", Location = new Point(12, 215), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            Button btnCheckout = new RoundedButton { Text = "Checkout", Location = new Point(12, 470), Width = 280, Height = 40, CornerRadius = 6 };
            btnCheckout.Click += BtnCheckout_ClickAsync;

            rightPanel.Controls.Add(_cmbPaymentMethod);
            rightPanel.Controls.Add(new Label { Text = "Payment Method", Location = new Point(12, 80), AutoSize = true });
            rightPanel.Controls.Add(_lblPaymentStatus);
            rightPanel.Controls.Add(_cmbPaymentStatus);
            rightPanel.Controls.Add(_lblPaymentAmount);
            rightPanel.Controls.Add(_numPaymentAmount);
            rightPanel.Controls.Add(_lblChange);
            rightPanel.Controls.Add(btnCheckout);
            rightPanel.Controls.Add(new Label { Text = "Discount Amount", Location = new Point(12, 250), AutoSize = true });
            rightPanel.Controls.Add(_numDiscount);
            rightPanel.Controls.Add(new Label { Text = "VAT (%)", Location = new Point(12, 300), AutoSize = true });
            rightPanel.Controls.Add(_numVat);
            rightPanel.Controls.Add(_lblCartTotal);
            rightPanel.Controls.Add(_lblDiscount);
            rightPanel.Controls.Add(_lblVat);
            rightPanel.Controls.Add(_lblGrandTotal);

            Controls.Add(leftPanel);
            Controls.Add(centerPanel);
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