using System.Drawing;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public static class FormDesignHelper
    {
        public static void ApplySearchBoxStyle(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        }

        public static void ApplyCrudButtonStyle(Button button, Color? color = null)
        {
            button.Height = 32;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = color ?? ThemeManager.Primary;
            button.ForeColor = Color.White;
        }

        public static void ApplySidebarButtonStyle(Button button)
        {
            button.BackColor = ThemeManager.SidebarButton;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        public static Label CreateCardLabel(string title, string value = "0")
        {
            return new Label
            {
                Text = value,
                AutoSize = true,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = ThemeManager.Foreground,
                Tag = title
            };
        }
    }
}
