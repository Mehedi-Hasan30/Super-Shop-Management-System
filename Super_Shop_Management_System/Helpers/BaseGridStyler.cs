using System.Drawing;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public static class BaseGridStyler
    {
        public static void Apply(DataGridView grid)
        {
            grid.BackgroundColor = ThemeManager.GridBackground;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.ForeColor = ThemeManager.GridText;
            grid.DefaultCellStyle.BackColor = ThemeManager.GridBackground;
            grid.DefaultCellStyle.SelectionBackColor = ThemeManager.GridSelectionBack;
            grid.DefaultCellStyle.SelectionForeColor = ThemeManager.GridSelectionFore;
            grid.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.PanelBackground;
        }
    }
}
