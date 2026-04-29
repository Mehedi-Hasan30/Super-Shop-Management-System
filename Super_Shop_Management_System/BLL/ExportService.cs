using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.BLL
{
    public class ExportService
    {
        public string ExportDataTableToCsv(DataTable table, string filePath)
        {
            if (table == null)
            {
                throw new ApplicationException("No data available for export.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            StringBuilder builder = new StringBuilder();

            // Header
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0) builder.Append(",");
                builder.Append(EscapeForCsv(table.Columns[i].ColumnName));
            }
            builder.AppendLine();

            // Rows
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i > 0) builder.Append(",");
                    builder.Append(EscapeForCsv(row[i] == DBNull.Value ? string.Empty : row[i].ToString()));
                }
                builder.AppendLine();
            }

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            return filePath;
        }

        public string ExportDataTableToExcelCsv(DataTable table, string filePath)
        {
            // Excel can open CSV directly; we still name it as ".csv" for reliability.
            return ExportDataTableToCsv(table, filePath);
        }

        public string ExportDataTableToPdfHtml(DataTable table, string filePath, string reportTitle)
        {
            if (table == null)
            {
                throw new ApplicationException("No data available for export.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("<html><head><meta charset='utf-8'/>");
            builder.AppendLine("<style>");
            builder.AppendLine("body{font-family:Segoe UI;margin:20px;}table{border-collapse:collapse;width:100%;}");
            builder.AppendLine("th,td{border:1px solid #666;padding:6px;font-size:12px;text-align:left;}");
            builder.AppendLine("th{background:#dfe6f2;}");
            builder.AppendLine("</style></head><body>");

            builder.AppendLine($"<h2>{System.Net.WebUtility.HtmlEncode(reportTitle)}</h2>");
            builder.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            builder.AppendLine("<table>");
            builder.AppendLine("<thead><tr>");

            foreach (DataColumn col in table.Columns)
            {
                builder.AppendLine($"<th>{System.Net.WebUtility.HtmlEncode(col.ColumnName)}</th>");
            }
            builder.AppendLine("</tr></thead><tbody>");

            foreach (DataRow row in table.Rows)
            {
                builder.AppendLine("<tr>");
                foreach (DataColumn col in table.Columns)
                {
                    string value = row[col] == DBNull.Value ? string.Empty : row[col].ToString();
                    builder.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(value)}</td>");
                }
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</tbody></table></body></html>");

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            return filePath;
        }

        public void PrintDataTableWithPreview(DataTable table, string title, bool showPreview = true)
        {
            if (table == null)
            {
                throw new ApplicationException("No data available to print.");
            }

            PrintDocument document = new PrintDocument();
            document.DocumentName = title;

            int currentRow = 0;
            float y = 0;
            int leftMargin = (int)document.DefaultPageSettings.Margins.Left;

            document.PrintPage += (_, args) =>
            {
                Graphics g = args.Graphics;
                float lineHeight = 16f;
                float maxY = args.MarginBounds.Bottom;

                using (Font headerFont = new Font("Segoe UI", 12, FontStyle.Bold))
                using (Font colFont = new Font("Segoe UI", 9))
                {
                    if (currentRow == 0)
                    {
                        args.Graphics.DrawString(title, headerFont, Brushes.Black, args.MarginBounds.Left, args.MarginBounds.Top);
                        y = args.MarginBounds.Top + 24;

                        // Header row
                        StringBuilder headerLine = new StringBuilder();
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            if (i > 0) headerLine.Append(" | ");
                            headerLine.Append(table.Columns[i].ColumnName);
                        }
                        args.Graphics.DrawString(headerLine.ToString(), colFont, Brushes.Black, args.MarginBounds.Left, y);
                        y += lineHeight;
                        args.Graphics.DrawLine(Pens.Black, args.MarginBounds.Left, y, args.MarginBounds.Right, y);
                        y += 4;
                    }

                    while (currentRow < table.Rows.Count)
                    {
                        DataRow row = table.Rows[currentRow];
                        StringBuilder line = new StringBuilder();
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            if (i > 0) line.Append(" | ");
                            line.Append((row[i] == DBNull.Value ? string.Empty : row[i].ToString()));
                        }

                        if (y + lineHeight > maxY)
                        {
                            args.HasMorePages = true;
                            return;
                        }

                        args.Graphics.DrawString(line.ToString(), colFont, Brushes.Black, args.MarginBounds.Left, y);
                        y += lineHeight;
                        currentRow++;
                    }

                    args.HasMorePages = false;
                }
            };

            if (showPreview)
            {
                using (PrintPreviewDialog preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 600 })
                {
                    preview.ShowDialog();
                }
            }
            else
            {
                using (PrintDialog dialog = new PrintDialog { Document = document })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        document.Print();
                    }
                }
            }
        }

        private static string EscapeForCsv(string value)
        {
            if (value == null) value = string.Empty;
            string escaped = value.Replace("\"", "\"\"");
            if (escaped.Contains(",") || escaped.Contains("\"") || escaped.Contains("\n") || escaped.Contains("\r"))
            {
                return $"\"{escaped}\"";
            }
            return escaped;
        }
    }
}

