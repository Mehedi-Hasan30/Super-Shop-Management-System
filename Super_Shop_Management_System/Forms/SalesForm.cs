using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class SalesForm : Form
    {
        private readonly PosService _posService = new PosService();
        private readonly SalesService _salesService = new SalesService();

        private TextBox _txtSearch;
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
        private TextBox _txtBarcode;
        private Label _lblBarcode;

        private Label _lblCartTotal;
        private Label _lblDiscount;
        private Label _lblVat;
        private Label _lblGrandTotal;

        private List<Product> _products = new List<Product>();

        public SalesForm()
        {
            InitializeComponent();
            Load += async (_, __) => await LoadProductsAsync();
        }

        private void InitializeComponent()
        {
            Text = "POS Sales";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            ThemeManager.ApplyFormTheme(this);

            Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 500, Padding = new Padding(12), BackColor = Color.White };
            Panel centerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.White };
            Panel rightPanel = new Panel { Dock = DockStyle.Right, Width = 320, Padding = new Padding(12), BackColor = Color.White };

            _txtSearch = new TextBox { Location = new Point(12, 30), Width = 300 };
            FormDesignHelper.ApplySearchBoxStyle(_txtSearch);

            Button btnSearch = new Button { Text = "Search", Location = new Point(320, 28), Width = 80 };
            FormDesignHelper.ApplyCrudButtonStyle(btnSearch, ThemeManager.Primary);
            btnSearch.Click += async (_, __) => await LoadProductsAsync(_txtSearch.Text);

            _lblBarcode = new Label { Text = "Barcode (Scanner)", Location = new Point(12, 55), AutoSize = true };
            _txtBarcode = new TextBox { Location = new Point(12, 75), Width = 300 };
            _txtBarcode.KeyDown += TxtBarcode_KeyDown;

            _gridProducts = new DataGridView
            {
                Location = new Point(12, 105),
                Width = 460,
                Height = 500,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_gridProducts);
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductID", HeaderText = "ID", Width = 50 });
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product", Width = 180 });
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Barcode", HeaderText = "Barcode", Width = 120 });
            _gridProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SellingPrice", HeaderText = "Price", Width = 90 });

            _numQuantity = new NumericUpDown { Location = new Point(12, 645), Width = 120, Minimum = 1, Maximum = 10000, Value = 1 };
            Button btnAddToCart = new Button { Text = "Add To Cart", Location = new Point(145, 642), Width = 140 };
            FormDesignHelper.ApplyCrudButtonStyle(btnAddToCart, Color.FromArgb(39, 174, 96));
            btnAddToCart.Click += BtnAddToCart_Click;

            leftPanel.Controls.Add(new Label { Text = "Search Product (Name/Barcode)", Location = new Point(12, 10), AutoSize = true });
            leftPanel.Controls.Add(_txtSearch);
            leftPanel.Controls.Add(btnSearch);
            leftPanel.Controls.Add(_lblBarcode);
            leftPanel.Controls.Add(_txtBarcode);
            leftPanel.Controls.Add(_gridProducts);
            leftPanel.Controls.Add(new Label { Text = "Quantity", Location = new Point(12, 625), AutoSize = true });
            leftPanel.Controls.Add(_numQuantity);
            leftPanel.Controls.Add(btnAddToCart);

            _gridCart = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 620,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_gridCart);
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductID", HeaderText = "ID", Width = 60 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product", Width = 220 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Qty", Width = 80 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", Width = 120 });
            _gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SubTotal", HeaderText = "Sub Total", Width = 140 });

            Button btnRemove = new Button { Text = "Remove", Location = new Point(12, 630), Width = 110 };
            Button btnClear = new Button { Text = "Clear Cart", Location = new Point(130, 630), Width = 110 };
            FormDesignHelper.ApplyCrudButtonStyle(btnRemove, Color.FromArgb(192, 57, 43));
            FormDesignHelper.ApplyCrudButtonStyle(btnClear, Color.FromArgb(127, 140, 141));
            btnRemove.Click += BtnRemove_Click;
            btnClear.Click += (_, __) => { _posService.ClearCart(); RefreshCart(); };

            centerPanel.Controls.Add(_gridCart);
            centerPanel.Controls.Add(btnRemove);
            centerPanel.Controls.Add(btnClear);

            _numDiscount = new NumericUpDown { Location = new Point(12, 170), Width = 280, DecimalPlaces = 2, Maximum = 999999999, Minimum = 0 };
            _numVat = new NumericUpDown { Location = new Point(12, 235), Width = 280, DecimalPlaces = 2, Maximum = 100, Minimum = 0, Value = 5 };
            _numDiscount.ValueChanged += (_, __) => RefreshSummary();
            _numVat.ValueChanged += (_, __) => RefreshSummary();

            _lblCartTotal = new Label { Location = new Point(12, 340), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblDiscount = new Label { Location = new Point(12, 370), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblVat = new Label { Location = new Point(12, 400), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _lblGrandTotal = new Label { Location = new Point(12, 430), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = ThemeManager.Primary };

            // Payment section (Cash change is calculated here)
            rightPanel.Controls.Add(new Label { Text = "Payment Method", Location = new Point(12, 80), AutoSize = true });
            _cmbPaymentMethod = new ComboBox
            {
                Location = new Point(12, 100),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "Card", "Mobile Banking" });
            _cmbPaymentMethod.SelectedIndex = -1;
            _cmbPaymentMethod.SelectedIndexChanged += (_, __) => RefreshPaymentUi();

            rightPanel.Controls.Add(_cmbPaymentMethod);

            _lblPaymentStatus = new Label { Text = "Payment Status", Location = new Point(12, 125), AutoSize = true };
            rightPanel.Controls.Add(_lblPaymentStatus);
            _cmbPaymentStatus = new ComboBox
            {
                Location = new Point(12, 145),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentStatus.Items.AddRange(new object[] { "Paid", "Pending" });
            _cmbPaymentStatus.SelectedIndex = 0; // keep Phase 3.2 default behavior as "Paid"
            _cmbPaymentStatus.SelectedIndexChanged += (_, __) => RefreshPaymentUi();
            rightPanel.Controls.Add(_cmbPaymentStatus);

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
            rightPanel.Controls.Add(_lblPaymentAmount);
            rightPanel.Controls.Add(_numPaymentAmount);

            _lblChange = new Label { Text = "Change: -", Location = new Point(12, 215), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            rightPanel.Controls.Add(_lblChange);

            Button btnCheckout = new Button { Text = "Checkout (Phase 3.2.1)", Location = new Point(12, 470), Width = 280, Height = 40 };
            FormDesignHelper.ApplyCrudButtonStyle(btnCheckout, ThemeManager.Primary);
            btnCheckout.Click += BtnCheckout_ClickAsync;

            rightPanel.Controls.Add(new Label { Text = "Billing Summary", Location = new Point(12, 10), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) });
            rightPanel.Controls.Add(new Label { Text = "Discount Amount", Location = new Point(12, 250), AutoSize = true });
            _numDiscount.Location = new Point(12, 270);
            rightPanel.Controls.Add(_numDiscount);
            rightPanel.Controls.Add(new Label { Text = "VAT (%)", Location = new Point(12, 300), AutoSize = true });
            _numVat.Location = new Point(12, 320);
            rightPanel.Controls.Add(_numVat);
            rightPanel.Controls.Add(_lblCartTotal);
            rightPanel.Controls.Add(_lblDiscount);
            rightPanel.Controls.Add(_lblVat);
            rightPanel.Controls.Add(_lblGrandTotal);
            rightPanel.Controls.Add(btnCheckout);

            Controls.Add(centerPanel);
            Controls.Add(leftPanel);
            Controls.Add(rightPanel);

            // Ensure initial payment UI visibility matches the unselected payment method.
            RefreshPaymentUi();
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
                ErrorLogger.Log("SalesForm.LoadProductsAsync", ex);
                MessageBox.Show($"Failed to load products: {ex.Message}", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                ErrorLogger.Log("SalesForm.BtnAddToCart_Click", ex);
                MessageBox.Show(ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            await AddBarcodeToCartAsync();
        }

        private async Task AddBarcodeToCartAsync()
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

                // Fast scan: add quantity 1 per scan.
                _posService.AddToCart(product, 1);
                RefreshCart();
                _txtBarcode.Clear();
                _txtBarcode.Focus();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SalesForm.AddBarcodeToCartAsync", ex);
                MessageBox.Show(ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtBarcode.SelectAll();
            }
            finally
            {
                _txtBarcode.Enabled = true;
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
            PaymentMethod? selected = GetSelectedPaymentMethod();
            SalePaymentStatus? selectedStatus = GetSelectedPaymentStatus();
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

        private PaymentMethod? GetSelectedPaymentMethod()
        {
            if (_cmbPaymentMethod.SelectedItem == null) return null;
            string value = _cmbPaymentMethod.SelectedItem.ToString();

            if (string.Equals(value, "Cash", StringComparison.OrdinalIgnoreCase))
                return PaymentMethod.Cash;
            if (string.Equals(value, "Card", StringComparison.OrdinalIgnoreCase))
                return PaymentMethod.Card;
            if (string.Equals(value, "Mobile Banking", StringComparison.OrdinalIgnoreCase))
                return PaymentMethod.MobileBanking;

            return null;
        }

        private SalePaymentStatus? GetSelectedPaymentStatus()
        {
            if (_cmbPaymentStatus == null || _cmbPaymentStatus.SelectedItem == null) return null;
            string value = _cmbPaymentStatus.SelectedItem.ToString();

            if (string.Equals(value, "Paid", StringComparison.OrdinalIgnoreCase))
                return SalePaymentStatus.Paid;
            if (string.Equals(value, "Pending", StringComparison.OrdinalIgnoreCase))
                return SalePaymentStatus.Pending;

            return null;
        }

        private async void BtnCheckout_ClickAsync(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null) btn.Enabled = false;

            try
            {
                IReadOnlyList<CartItem> cartItems = _posService.GetCartItems();
                if (cartItems == null || cartItems.Count == 0)
                {
                    MessageBox.Show("Cart is empty.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                PaymentMethod? selectedPayment = GetSelectedPaymentMethod();
                if (selectedPayment == null)
                {
                    MessageBox.Show("Please select a payment method.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SalePaymentStatus? selectedPaymentStatus = GetSelectedPaymentStatus();
                if (selectedPaymentStatus == null)
                {
                    MessageBox.Show("Please select a payment status.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                PosTotals totals = _posService.CalculateTotals(_numDiscount.Value, _numVat.Value);
                decimal grandTotal = totals.GrandTotal;
                if (grandTotal <= 0)
                {
                    MessageBox.Show("Grand total must be greater than zero.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal paymentAmount = selectedPayment == PaymentMethod.Cash
                    ? _numPaymentAmount.Value
                    : grandTotal; // card/mobile assume full payment

                int saleId = await _salesService.SaveSaleAsync(
                    cartItems,
                    totals,
                    selectedPayment.Value,
                    paymentAmount,
                    selectedPaymentStatus.Value,
                    customerId: null);

                _posService.ClearCart();
                _numPaymentAmount.Value = 0;
                _cmbPaymentMethod.SelectedIndex = -1;
                RefreshCart();

                MessageBox.Show($"Sale saved successfully. SaleID: {saleId}", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Professional POS cycle: show invoice preview for both Paid and Pending sales.
                try
                {
                    using (InvoiceForm invoiceForm = new InvoiceForm(saleId))
                    {
                        invoiceForm.ShowDialog(this);
                    }
                }
                catch (Exception ex)
                {
                    // Sale is already saved; invoice preview failure should not block checkout completion.
                    ErrorLogger.Log("SalesForm.InvoicePreviewAfterCheckout", ex);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SalesForm.BtnCheckout_ClickAsync", ex);
                MessageBox.Show(ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (btn != null) btn.Enabled = true;
            }
        }
    }
}
