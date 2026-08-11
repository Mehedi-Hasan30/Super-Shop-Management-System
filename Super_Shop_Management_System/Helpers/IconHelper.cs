using System;
using System.Drawing;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public static class IconHelper
    {
        private const string IconFontFamily = "Segoe MDL2 Assets";
        private static bool? _fontAvailable;

        public static bool IsIconFontAvailable
        {
            get
            {
                if (_fontAvailable.HasValue) return _fontAvailable.Value;
                try
                {
                    using (var testFont = new Font(IconFontFamily, 10F))
                    {
                        _fontAvailable = string.Equals(testFont.Name, IconFontFamily, StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch
                {
                    _fontAvailable = false;
                }
                return _fontAvailable.Value;
            }
        }

        public static Font GlyphFont(float sizeInPoints, FontStyle style = FontStyle.Regular)
        {
            return new Font(IsIconFontAvailable ? IconFontFamily : "Segoe UI", sizeInPoints, style);
        }

        public static string GlyphOrFallback(string glyph, string fallbackText)
        {
            return IsIconFontAvailable ? glyph : fallbackText;
        }

        public static Label CreateIconLabel(string glyph, float sizeInPoints, Color color)
        {
            return new Label
            {
                Text = glyph,
                Font = GlyphFont(sizeInPoints),
                ForeColor = color,
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        public static class Glyphs
        {
            public const string Dashboard = "\uE80F";
            public const string Products = "\uE7BF";
            public const string Inventory = "\uE7B8";
            public const string Sales = "\uE73E";
            public const string Customers = "\uE77B";
            public const string Employees = "\uE716";
            public const string Reports = "\uE9D2";
            public const string Suppliers = "\uE8D4";
            public const string Attendance = "\uE787";
            public const string AuditLog = "\uE81C";
            public const string Settings = "\uE713";
            public const string Logout = "\uE7E8";
            public const string Backup = "\uE896";

            public const string Add = "\uE710";
            public const string Edit = "\uE70F";
            public const string Delete = "\uE74D";
            public const string Search = "\uE721";
            public const string Export = "\uE898";
            public const string Print = "\uE749";
            public const string Refresh = "\uE72C";
            public const string Filter = "\uE71C";

            public const string Success = "\uE73E";
            public const string Warning = "\uE7BA";
            public const string Error = "\uE783";
            public const string Info = "\uE946";
            public const string Alert = "\uE7BA";

            public const string EyeShow = "\uE7B3";
            public const string EyeHide = "\uE7B2";
            public const string User = "\uE77B";
            public const string Lock = "\uE72E";
            public const string Email = "\uE715";
            public const string ChevronDown = "\uE70D";
            public const string ChevronRight = "\uE76C";
        }
    }
}
