using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class InvoiceForm : Form
    {
        private readonly int _saleId;
        private readonly InvoiceService _invoiceService = new InvoiceService();

        private InvoiceData _invoice;

        private ComboBox _cmbPaperSize;
        private Button _btnDirectPrint;
        private Button _btnClose;
        private PrintDocument _printDocument;
        private PrintPreviewDialog _printPreviewDialog;
        private int _currentLineIndex;

        private const int A4_WIDTH_HUNDREDTHS = 827; // 8.27 in * 100
        private const int A4_HEIGHT_HUNDREDTHS = 1169; // 11.69 in * 100
        private const int THERMAL_80_WIDTH_HUNDREDTHS = 315; // 3.15 in * 100 (~80mm)
        private const int THERMAL_80_HEIGHT_HUNDREDTHS = 1100; // long receipt; will paginate

        public InvoiceForm(int saleId)
        {
            _saleId = saleId;
            InitializeComponent();
            ThemeManager.ApplyFormTheme(this);
        }

        private void InitializeComponent()
        {
            Text = "Invoice / Receipt Preview";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 700);

            Label lblPaper = new Label { Text = "Paper Size", Location = new Point(15, 15), AutoSize = true };

            _cmbPaperSize = new ComboBox
            {
                Location = new Point(105, 12),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPaperSize.Items.AddRange(new object[] { "Thermal 80mm", "A4" });
            _cmbPaperSize.SelectedIndex = 0;
            _cmbPaperSize.SelectedIndexChanged += (_, __) => ApplyPaperSize();

            _btnDirectPrint = new Button
            {
                Text = "Direct Print",
                Location = new Point(320, 10),
                Width = 130,
                Height = 30
            };
            FormDesignHelper.ApplyCrudButtonStyle(_btnDirectPrint, ThemeManager.Primary);
            _btnDirectPrint.Click += BtnDirectPrint_Click;

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(460, 10),
                Width = 110,
                Height = 30
            };
            FormDesignHelper.ApplyCrudButtonStyle(_btnClose, Color.FromArgb(127, 140, 141));
            _btnClose.Click += (_, __) => Close();

            Controls.Add(lblPaper);
            Controls.Add(_cmbPaperSize);
            Controls.Add(_btnDirectPrint);
            Controls.Add(_btnClose);

            _printDocument = new PrintDocument();
            _printDocument.BeginPrint += (_, __) => _currentLineIndex = 0;
            _printDocument.PrintPage += PrintDocument_PrintPage;

            _printPreviewDialog = new PrintPreviewDialog
            {
                Document = _printDocument,
                Width = 900,
                Height = 600
            };

            Shown += async (_, __) =>
            {
                await LoadInvoiceAsync();
                ApplyPaperSize();
                using (_printPreviewDialog)
                {
                    _printPreviewDialog.ShowDialog(this);
                }
            };
        }

        private async Task LoadInvoiceAsync()
        {
            _invoice = await _invoiceService.GetInvoiceAsync(_saleId);
        }

        private void ApplyPaperSize()
        {
            if (_printDocument == null) return;

            bool thermal = _cmbPaperSize.SelectedIndex == 0;
            if (thermal)
            {
                _printDocument.DefaultPageSettings.PaperSize = new PaperSize("Thermal 80mm", THERMAL_80_WIDTH_HUNDREDTHS, THERMAL_80_HEIGHT_HUNDREDTHS);
                _printDocument.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            }
            else
            {
                _printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", A4_WIDTH_HUNDREDTHS, A4_HEIGHT_HUNDREDTHS);
                _printDocument.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);
            }
        }

        private void BtnDirectPrint_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyPaperSize();
                _currentLineIndex = 0;
                _printDocument.Print();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("InvoiceForm.BtnDirectPrint_Click", ex);
                MessageBox.Show(ex.Message, "Print", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_invoice == null)
            {
                e.HasMorePages = false;
                return;
            }

            bool thermal = _printDocument.DefaultPageSettings.PaperSize.Width == THERMAL_80_WIDTH_HUNDREDTHS;
            float left = e.MarginBounds.Left;
            float right = e.MarginBounds.Right;
            RectangleF bounds = e.MarginBounds;
            float y = bounds.Top;

            Font titleFont = new Font("Segoe UI", thermal ? 12 : 16, FontStyle.Bold);
            Font normalFont = new Font("Segoe UI", thermal ? 8.5F : 10F, FontStyle.Regular);
            Font boldFont = new Font("Segoe UI", thermal ? 9F : 11F, FontStyle.Bold);

            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far };
            StringFormat leftFormat = new StringFormat { Alignment = StringAlignment.Near };

            // Header
            DrawStringSafe(e.Graphics, _invoice.ShopName ?? "Super Shop", titleFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 20), centerFormat);
            y += titleFont.GetHeight(e.Graphics) + 2;
            DrawStringSafe(e.Graphics, $"Invoice No: {_invoice.SaleID}", boldFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 16), centerFormat);
            y += boldFont.GetHeight(e.Graphics) + 2;

            DrawKeyValue(e.Graphics, "Customer", _invoice.CustomerName, normalFont, Brushes.Black, bounds, ref y);
            DrawKeyValue(e.Graphics, "Cashier", _invoice.EmployeeName, normalFont, Brushes.Black, bounds, ref y);
            DrawKeyValue(e.Graphics, "Date", _invoice.SaleDate.ToString("dd/MM/yyyy HH:mm"), normalFont, Brushes.Black, bounds, ref y);

            y += 4;
            e.Graphics.DrawLine(Pens.Black, left, y, right, y);
            y += 6;

            // Column header (simple, thermal-friendly)
            if (!thermal)
            {
                DrawStringSafe(e.Graphics, "Item", normalFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width * 0.55F, 15), leftFormat);
                DrawStringSafe(e.Graphics, "Qty", normalFont, Brushes.Black, new RectangleF(bounds.Left + bounds.Width * 0.55F, y, bounds.Width * 0.15F, 15), rightFormat);
                DrawStringSafe(e.Graphics, "Unit", normalFont, Brushes.Black, new RectangleF(bounds.Left + bounds.Width * 0.70F, y, bounds.Width * 0.15F, 15), rightFormat);
                DrawStringSafe(e.Graphics, "Sub", normalFont, Brushes.Black, new RectangleF(bounds.Left + bounds.Width * 0.85F, y, bounds.Width * 0.15F, 15), rightFormat);
                y += normalFont.GetHeight(e.Graphics) + 2;
            }

            // Lines
            List<InvoiceLine> lines = _invoice.Lines ?? new List<InvoiceLine>();
            bool allLinesRendered = false;
            for (int i = _currentLineIndex; i < lines.Count; i++)
            {
                InvoiceLine line = lines[i];

                string nameLine = $"{line.Quantity} x {line.ProductName}";
                string barcodeLine = $"Barcode: {line.Barcode}";
                string priceLine = $"Unit: {line.UnitPrice:C}   Sub: {line.SubTotal:C}";

                // Avoid overly long text on thermal to reduce layout issues.
                if (thermal)
                {
                    nameLine = Truncate(nameLine, 28);
                    barcodeLine = Truncate(barcodeLine, 26);
                }

                float blockHeight;
                if (thermal)
                {
                    blockHeight = normalFont.GetHeight(e.Graphics) * 3 + 6;
                }
                else
                {
                    blockHeight = normalFont.GetHeight(e.Graphics) * 2.2F + 8;
                }

                if (y + blockHeight > bounds.Bottom)
                {
                    _currentLineIndex = i;
                    e.HasMorePages = true;
                    return;
                }

                if (thermal)
                {
                    DrawStringSafe(e.Graphics, nameLine, normalFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 15), leftFormat);
                    y += normalFont.GetHeight(e.Graphics) + 1;

                    DrawStringSafe(e.Graphics, barcodeLine, normalFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 15), leftFormat);
                    y += normalFont.GetHeight(e.Graphics) + 1;

                    DrawStringSafe(e.Graphics, priceLine, normalFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 15), rightFormat);
                    y += normalFont.GetHeight(e.Graphics) + 4;
                }
                else
                {
                    // A4: still simple block layout (professional table is possible, but keep stable)
                    DrawStringSafe(e.Graphics, line.ProductName, normalFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width * 0.55F, 20), leftFormat);
                    DrawStringSafe(e.Graphics, line.Quantity.ToString(), normalFont, Brushes.Black, new RectangleF(bounds.Left + bounds.Width * 0.55F, y, bounds.Width * 0.15F, 20), rightFormat);
                    DrawStringSafe(e.Graphics, line.UnitPrice.ToString("0.00"), normalFont, Brushes.Black, new RectangleF(bounds.Left + bounds.Width * 0.70F, y, bounds.Width * 0.15F, 20), rightFormat);
                    DrawStringSafe(e.Graphics, line.SubTotal.ToString("0.00"), normalFont, Brushes.Black, new RectangleF(bounds.Left + bounds.Width * 0.85F, y, bounds.Width * 0.15F, 20), rightFormat);
                    y += normalFont.GetHeight(e.Graphics) + 2;

                    // barcode under item for traceability
                    DrawStringSafe(e.Graphics, Truncate(line.Barcode, 18), normalFont, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 15), leftFormat);
                    y += normalFont.GetHeight(e.Graphics) + 4;
                }
            }

            _currentLineIndex = lines.Count;
            allLinesRendered = true;
            e.HasMorePages = false;

            if (allLinesRendered)
            {
                y += 2;
                e.Graphics.DrawLine(Pens.Black, left, y, right, y);
                y += 8;

                DrawKeyValue(e.Graphics, "Total", _invoice.TotalAmount.ToString("0.00"), boldFont, Brushes.Black, bounds, ref y);
                DrawKeyValue(e.Graphics, "Discount", _invoice.Discount.ToString("0.00"), boldFont, Brushes.Black, bounds, ref y);
                DrawKeyValue(e.Graphics, "VAT", _invoice.VAT.ToString("0.00"), boldFont, Brushes.Black, bounds, ref y);
                DrawKeyValue(e.Graphics, "Grand Total", _invoice.GrandTotal.ToString("0.00"), titleFont, Brushes.Black, bounds, ref y);

                y += 6;
                DrawKeyValue(e.Graphics, "Payment", _invoice.PaymentMethod, normalFont, Brushes.Black, bounds, ref y);
                DrawKeyValue(e.Graphics, "Status", _invoice.PaymentStatus, normalFont, Brushes.Black, bounds, ref y);

                y += 8;
                DrawStringSafe(e.Graphics, "Thank you for shopping with us!", normalFont, Brushes.Black,
                    new RectangleF(bounds.Left, y, bounds.Width, 20), centerFormat);
            }
        }

        private static void DrawStringSafe(Graphics g, string text, Font font, Brush brush, RectangleF rect, StringFormat format)
        {
            if (text == null) text = string.Empty;
            g.DrawString(text, font, brush, rect, format);
        }

        private static void DrawKeyValue(Graphics g, string key, string value, Font font, Brush brush, RectangleF bounds, ref float y)
        {
            string keyText = $"{key}: ";
            SizeF keySize = g.MeasureString(keyText, font);
            float keyWidth = keySize.Width;

            // Draw key (left-aligned) and value (right-aligned) on same line for receipts.
            g.DrawString(keyText, font, brush, new RectangleF(bounds.Left, y, keyWidth, 20));
            g.DrawString(value ?? string.Empty, font, brush,
                new RectangleF(bounds.Left + keyWidth, y, bounds.Width - keyWidth, 20), new StringFormat { Alignment = StringAlignment.Far });
            y += font.GetHeight(g) + 2;
        }

        private static string Truncate(string text, int maxLen)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Length <= maxLen) return text;
            return text.Substring(0, maxLen);
        }
    }
}

