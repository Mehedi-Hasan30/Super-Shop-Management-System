using System;
using System.Drawing;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public static class DataGridStyler
    {
        public static void ApplyModernStyle(DataGridView grid)
        {
            grid.BackgroundColor = Color.White;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(220, 225, 230);
            ApplyRowHeadersStyle(grid);
            ApplyHeaderStyle(grid);
            ApplyAlternatingRows(grid);
            ApplySelectionStyle(grid);
        }

        private static void ApplyRowHeadersStyle(DataGridView grid)
        {
            grid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 235, 240);
            grid.RowHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 70);
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 150, 240);
            grid.RowHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.RowHeadersWidth = 40;
            grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            grid.RowTemplate.Height = 30;
        }

        private static void ApplyHeaderStyle(DataGridView grid)
        {
            var headerStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 125, 165),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 4, 8, 4)
            };
            grid.ColumnHeadersDefaultCellStyle = headerStyle;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        private static void ApplyAlternatingRows(DataGridView grid)
        {
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250);
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 60);
            grid.AlternatingRowsDefaultCellStyle.Font = new Font("Segoe UI", 9);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 150, 240);
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
        }

        private static void ApplySelectionStyle(DataGridView grid)
        {
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 150, 240);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 60);
            grid.EnableHeadersVisualStyles = false;
        }
    }
}