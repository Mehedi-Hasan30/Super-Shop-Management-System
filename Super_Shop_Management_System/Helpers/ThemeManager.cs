using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public enum AppTheme
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        private const string ThemeHookTag = "__ThemeManagerHooked__";

        public static readonly Color Primary = Color.FromArgb(41, 128, 185);
        public static readonly Color PrimaryDark = Color.FromArgb(31, 97, 141);
        public static readonly Color Accent = Color.FromArgb(52, 152, 219);
        public static readonly Color Sidebar = Color.FromArgb(33, 47, 61);
        public static readonly Color SidebarButton = Color.FromArgb(52, 73, 94);
        public static readonly Color SidebarButtonHover = Color.FromArgb(41, 58, 74);
        public static readonly Color SidebarButtonActive = Color.FromArgb(41, 128, 185);

        public static readonly Color Success = Color.FromArgb(39, 174, 96);
        public static readonly Color Warning = Color.FromArgb(243, 156, 18);
        public static readonly Color Danger = Color.FromArgb(231, 76, 60);
        public static readonly Color Info = Color.FromArgb(52, 152, 219);

        public static readonly Color BorderLight = Color.FromArgb(225, 229, 233);
        public static readonly Color BorderDark = Color.FromArgb(60, 60, 60);
        public static readonly Color MutedTextLight = Color.FromArgb(127, 140, 141);
        public static readonly Color MutedTextDark = Color.FromArgb(160, 160, 160);

        private const string FontFamily = "Segoe UI";
        public static readonly Font FontDisplay = new Font(FontFamily, 24F, FontStyle.Bold);
        public static readonly Font FontH1 = new Font(FontFamily, 18F, FontStyle.Bold);
        public static readonly Font FontH2 = new Font(FontFamily, 14F, FontStyle.Bold);
        public static readonly Font FontH3 = new Font(FontFamily, 11F, FontStyle.Bold);
        public static readonly Font FontBody = new Font(FontFamily, 9.5F, FontStyle.Regular);
        public static readonly Font FontBodyBold = new Font(FontFamily, 9.5F, FontStyle.Bold);
        public static readonly Font FontCaption = new Font(FontFamily, 8.5F, FontStyle.Regular);

        public static class Spacing
        {
            public const int XS = 4;
            public const int SM = 8;
            public const int MD = 16;
            public const int LG = 24;
            public const int XL = 32;
            public const int XXL = 48;
        }

        public static Color BorderColor => CurrentTheme == AppTheme.Dark ? BorderDark : BorderLight;
        public static Color MutedText => CurrentTheme == AppTheme.Dark ? MutedTextDark : MutedTextLight;

        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

        private static string ThemeFolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SuperShopManagementSystem");

        private static string ThemeFilePath => Path.Combine(ThemeFolderPath, "theme.txt");

        public static Color Background => CurrentTheme == AppTheme.Dark
            ? Color.FromArgb(22, 22, 22)
            : Color.FromArgb(244, 246, 248);

        public static Color Foreground => CurrentTheme == AppTheme.Dark
            ? Color.FromArgb(236, 236, 236)
            : Color.FromArgb(52, 73, 94);

        public static Color PanelBackground => CurrentTheme == AppTheme.Dark
            ? Color.FromArgb(30, 30, 30)
            : Color.White;

        public static Color ControlBackground => CurrentTheme == AppTheme.Dark
            ? Color.FromArgb(45, 45, 45)
            : Color.White;

        public static Color GridBackground => CurrentTheme == AppTheme.Dark
            ? Color.FromArgb(35, 35, 35)
            : Color.White;

        public static Color GridText => CurrentTheme == AppTheme.Dark
            ? Color.FromArgb(236, 236, 236)
            : Color.Black;

        public static Color GridSelectionBack => CurrentTheme == AppTheme.Dark
            ? Color.FromArgb(77, 127, 168)
            : Color.FromArgb(214, 234, 248);

        public static Color GridSelectionFore => CurrentTheme == AppTheme.Dark
            ? Color.White
            : Color.Black;

        public static void LoadThemePreference()
        {
            try
            {
                if (!File.Exists(ThemeFilePath))
                {
                    CurrentTheme = AppTheme.Light;
                    return;
                }

                string value = File.ReadAllText(ThemeFilePath).Trim();
                if (string.Equals(value, "Dark", StringComparison.OrdinalIgnoreCase))
                    CurrentTheme = AppTheme.Dark;
                else
                    CurrentTheme = AppTheme.Light;
            }
            catch
            {
                CurrentTheme = AppTheme.Light;
            }
        }

        public static void SetTheme(AppTheme theme)
        {
            CurrentTheme = theme;
            PersistThemePreference();
        }

        public static void ToggleTheme()
        {
            SetTheme(CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark);
        }

        private static void PersistThemePreference()
        {
            try
            {
                if (!Directory.Exists(ThemeFolderPath))
                {
                    Directory.CreateDirectory(ThemeFolderPath);
                }

                File.WriteAllText(ThemeFilePath, CurrentTheme == AppTheme.Dark ? "Dark" : "Light");
            }
            catch
            {
                // Ignore persistence errors.
            }
        }

        public static void ApplyFormTheme(Form form)
        {
            if (form == null) return;

            form.BackColor = Background;
            form.ForeColor = Foreground;
            form.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            ApplyThemeToControls(form.Controls);

            if (!(form.Tag is string) || !string.Equals(form.Tag?.ToString(), ThemeHookTag, StringComparison.Ordinal))
            {
                form.Tag = ThemeHookTag;
                form.ControlAdded += (_, e) =>
                {
                    if (e?.Control == null) return;
                    ApplyThemeToControl(e.Control);
                };
            }
        }

        public static void ApplyPrimaryButton(Button button)
        {
            button.BackColor = Primary;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        private static void ApplyThemeToControl(Control c)
        {
            if (c == null) return;

            if (c is DataGridView grid)
            {
                BaseGridStyler.Apply(grid);
                return;
            }

            if (c is Panel || c is GroupBox)
            {
                c.BackColor = PanelBackground;
                c.ForeColor = Foreground;
            }
            else if (c is TextBox)
            {
                TextBox tb = (TextBox)c;
                tb.BackColor = ControlBackground;
                tb.ForeColor = Foreground;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is ComboBox)
            {
                ComboBox cb = (ComboBox)c;
                cb.BackColor = ControlBackground;
                cb.ForeColor = Foreground;
            }
            else if (c is DateTimePicker)
            {
                DateTimePicker dt = (DateTimePicker)c;
                dt.CalendarMonthBackground = PanelBackground;
                dt.CalendarForeColor = Foreground;
                dt.BackColor = ControlBackground;
                dt.ForeColor = Foreground;
            }
            else if (c is Label)
            {
                c.ForeColor = Foreground;
            }
            else if (c is Button)
            {
                c.ForeColor = Foreground;
            }
            else
            {
                c.ForeColor = Foreground;
            }

            if (c.HasChildren)
            {
                ApplyThemeToControls(c.Controls);
            }
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                ApplyThemeToControl(c);
            }
        }
    }
}
