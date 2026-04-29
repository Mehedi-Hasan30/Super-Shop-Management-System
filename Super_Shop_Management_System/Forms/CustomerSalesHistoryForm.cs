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
    public class CustomerSalesHistoryForm : Form
    {
        private readonly InvoiceService _invoiceService = new InvoiceService();
        private readonly CustomerService _customerService = new CustomerService();

        private ComboBox _cmbCustomer;
        private ComboBox _cmbPaymentStatus;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private CheckBox _chkFrom;
        private CheckBox _chkTo;
        private Button _btnSearch;
        private DataGridView _grid;
        private Button _btnReprintSelected;

        private TextBox _txtSaleIdSearch;
        private Button _btnReprintBySaleId;

        public CustomerSalesHistoryForm()
        {
            InitializeComponent();
            ThemeManager.ApplyFormTheme(this);
            Load += async (_, __) => await LoadCustomersAsync();
        }

        private void InitializeComponent()
        {
            Text = "Customer Sales History";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1050, 700);

            Label lblCustomer = new Label { Text = "Customer", Location = new Point(20, 20), AutoSize = true };
            _cmbCustomer = new ComboBox
            {
                Location = new Point(90, 17),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblPaymentStatus = new Label { Text = "Payment Status", Location = new Point(370, 20), AutoSize = true };
            _cmbPaymentStatus = new ComboBox
            {
                Location = new Point(500, 17),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaymentStatus.Items.AddRange(new object[] { "All", "Paid", "Pending" });
            _cmbPaymentStatus.SelectedIndex = 0;

            _chkFrom = new CheckBox { Text = "From", Location = new Point(690, 18), AutoSize = true, Checked = false };
            _dtFrom = new DateTimePicker { Location = new Point(730, 15), Width = 120, Format = DateTimePickerFormat.Short, Enabled = false };

            _chkTo = new CheckBox { Text = "To", Location = new Point(860, 18), AutoSize = true, Checked = false };
            _dtTo = new DateTimePicker { Location = new Point(885, 15), Width = 120, Format = DateTimePickerFormat.Short, Enabled = false };

            _chkFrom.CheckedChanged += (_, __) => { _dtFrom.Enabled = _chkFrom.Checked; };
            _chkTo.CheckedChanged += (_, __) => { _dtTo.Enabled = _chkTo.Checked; };

            _btnSearch = new Button { Text = "Search", Location = new Point(20, 50), Width = 100, Height = 35 };
            FormDesignHelper.ApplyCrudButtonStyle(_btnSearch, ThemeManager.Primary);
            _btnSearch.Click += async (_, __) => await SearchAsync();

            _grid = new DataGridView
            {
                Location = new Point(20, 95),
                Width = 990,
                Height = 450,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_grid);

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SaleID", HeaderText = "SaleID", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SaleDate", HeaderText = "Date", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Customer", Width = 220 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "GrandTotal", HeaderText = "Grand Total", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PaymentMethod", HeaderText = "Payment", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PaymentStatus", HeaderText = "Status", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeName", HeaderText = "Employee", Width = 160 });

            _grid.SelectionChanged += (_, __) => RefreshReprintButtonState();

            _btnReprintSelected = new Button
            {
                Text = "Reprint Selected Invoice",
                Location = new Point(20, 560),
                Width = 280,
                Height = 40,
                Enabled = false
            };
            FormDesignHelper.ApplyCrudButtonStyle(_btnReprintSelected, ThemeManager.Primary);
            _btnReprintSelected.Click += BtnReprintSelected_Click;

            // SaleID reprint by search
            Label lblSaleId = new Label { Text = "Reprint by SaleID", Location = new Point(320, 565), AutoSize = true };
            _txtSaleIdSearch = new TextBox { Location = new Point(445, 560), Width = 120 };
            _btnReprintBySaleId = new Button { Text = "Reprint", Location = new Point(580, 555), Width = 90, Height = 45 };
            FormDesignHelper.ApplyCrudButtonStyle(_btnReprintBySaleId, ThemeManager.Primary);
            _btnReprintBySaleId.Click += BtnReprintBySaleId_Click;

            Controls.Add(lblCustomer);
            Controls.Add(_cmbCustomer);
            Controls.Add(lblPaymentStatus);
            Controls.Add(_cmbPaymentStatus);
            Controls.Add(_chkFrom);
            Controls.Add(_dtFrom);
            Controls.Add(_chkTo);
            Controls.Add(_dtTo);
            Controls.Add(_btnSearch);
            Controls.Add(_grid);
            Controls.Add(_btnReprintSelected);
            Controls.Add(lblSaleId);
            Controls.Add(_txtSaleIdSearch);
            Controls.Add(_btnReprintBySaleId);
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                List<Customer> customers = await _customerService.GetAllAsync();
                customers.Insert(0, new Customer { CustomerID = 0, FullName = "All Customers" });

                _cmbCustomer.DataSource = customers;
                _cmbCustomer.DisplayMember = "FullName";
                _cmbCustomer.ValueMember = "CustomerID";
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("CustomerSalesHistoryForm.LoadCustomersAsync", ex);
                MessageBox.Show(ex.Message, "Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                int? customerId = null;
                if (_cmbCustomer.SelectedValue != null)
                {
                    int selected = Convert.ToInt32(_cmbCustomer.SelectedValue);
                    if (selected > 0) customerId = selected;
                }

                DateTime? fromDate = _chkFrom.Checked ? _dtFrom.Value.Date : (DateTime?)null;
                DateTime? toDate = _chkTo.Checked ? _dtTo.Value.Date : (DateTime?)null;
                string paymentStatus = _cmbPaymentStatus.SelectedItem?.ToString() ?? "All";

                List<SalesHistoryItem> results = await _invoiceService.GetCustomerSalesHistoryAsync(customerId, fromDate, toDate, paymentStatus);
                _grid.DataSource = null;
                _grid.DataSource = results;

                RefreshReprintButtonState();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("CustomerSalesHistoryForm.SearchAsync", ex);
                MessageBox.Show(ex.Message, "Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshReprintButtonState()
        {
            _btnReprintSelected.Enabled = _grid.CurrentRow?.DataBoundItem is SalesHistoryItem;
        }

        private void BtnReprintSelected_Click(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is SalesHistoryItem item))
            {
                MessageBox.Show("Select a sale from the list first.", "Reprint", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (InvoiceForm invoiceForm = new InvoiceForm(item.SaleID))
            {
                invoiceForm.ShowDialog(this);
            }
        }

        private void BtnReprintBySaleId_Click(object sender, EventArgs e)
        {
            string text = _txtSaleIdSearch.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Enter a SaleID to reprint.", "Reprint", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!int.TryParse(text, out int saleId) || saleId <= 0)
            {
                MessageBox.Show("Invalid SaleID format.", "Reprint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (InvoiceForm invoiceForm = new InvoiceForm(saleId))
            {
                invoiceForm.ShowDialog(this);
            }
        }
    }
}

