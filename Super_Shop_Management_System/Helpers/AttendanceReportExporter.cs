using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Helpers
{
    public static class AttendanceReportExporter
    {
        public static string ExportToExcelCsv(List<Attendance> data, string filePath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("AttendanceID,EmployeeID,EmployeeName,Date,CheckIn,CheckOut,Status");

            foreach (Attendance item in data)
            {
                builder.AppendLine($"{item.AttendanceID},{item.EmployeeID},\"{Escape(item.EmployeeName)}\",{item.Date:yyyy-MM-dd},{FormatTime(item.CheckIn)},{FormatTime(item.CheckOut)},\"{Escape(item.Status)}\"");
            }

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            return filePath;
        }

        public static string ExportToPdfHtml(List<Attendance> data, string filePath, string reportTitle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<html><head><meta charset='utf-8'/><style>body{font-family:Segoe UI;}table{border-collapse:collapse;width:100%;}th,td{border:1px solid #666;padding:6px;text-align:left;}th{background:#dfe6f2;}h2{color:#2c3e50;}</style></head><body>");
            builder.AppendLine($"<h2>{System.Net.WebUtility.HtmlEncode(reportTitle)}</h2>");
            builder.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            builder.AppendLine("<table><thead><tr><th>ID</th><th>Employee</th><th>Date</th><th>Check In</th><th>Check Out</th><th>Status</th></tr></thead><tbody>");

            foreach (Attendance item in data)
            {
                builder.AppendLine($"<tr><td>{item.AttendanceID}</td><td>{System.Net.WebUtility.HtmlEncode(item.EmployeeName)}</td><td>{item.Date:yyyy-MM-dd}</td><td>{FormatTime(item.CheckIn)}</td><td>{FormatTime(item.CheckOut)}</td><td>{System.Net.WebUtility.HtmlEncode(item.Status)}</td></tr>");
            }

            builder.AppendLine("</tbody></table></body></html>");
            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            return filePath;
        }

        public static void OpenFile(string filePath)
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        public static void PrintGrid(DataGridView grid, string title)
        {
            PrintDocument document = new PrintDocument();
            int rowIndex = 0;
            document.DocumentName = title;
            document.PrintPage += (_, args) =>
            {
                float y = args.MarginBounds.Top;
                using (Font headerFont = new Font("Segoe UI", 10, FontStyle.Bold))
                using (Font rowFont = new Font("Segoe UI", 9))
                {
                    args.Graphics.DrawString(title, headerFont, Brushes.Black, args.MarginBounds.Left, y);
                    y += 26;
                    args.Graphics.DrawString("Employee | Date | Check In | Check Out | Status", headerFont, Brushes.Black, args.MarginBounds.Left, y);
                    y += 22;

                    while (rowIndex < grid.Rows.Count)
                    {
                        DataGridViewRow row = grid.Rows[rowIndex];
                        string dateValue = row.Cells[2].Value == null ? string.Empty : Convert.ToDateTime(row.Cells[2].Value).ToString("yyyy-MM-dd");
                        string line = $"{row.Cells[1].Value} | {dateValue} | {row.Cells[3].Value} | {row.Cells[4].Value} | {row.Cells[5].Value}";
                        args.Graphics.DrawString(line, rowFont, Brushes.Black, args.MarginBounds.Left, y);
                        y += 18;
                        rowIndex++;

                        if (y > args.MarginBounds.Bottom - 25)
                        {
                            args.HasMorePages = true;
                            return;
                        }
                    }
                }
                args.HasMorePages = false;
            };

            using (PrintDialog dialog = new PrintDialog { Document = document })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                document.Print();
            }
        }

        private static string FormatTime(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("HH:mm") : string.Empty;
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\"", "\"\"");
        }
    }
}
