using System.Drawing;

namespace Super_Shop_Management_System.Helpers
{
    public static class ThemeFonts
    {
        // Font family - Segoe UI for modern Windows appearance
        private static readonly FontFamily FontFamily = new FontFamily("Segoe UI");

        // Font sizes - consistent typography scale
        public const float FontSizeXS = 8F;
        public const float FontSizeSM = 9F;
        public const float FontSizeMD = 10F;
        public const float FontSizeBase = 11F;
        public const float FontSizeLG = 12F;
        public const float FontSizeXL = 14F;
        public const float FontSize2XL = 16F;
        public const float FontSize3XL = 18F;
        public const float FontSize4XL = 24F;
        public const float FontSize5XL = 32F;

        // Font styles - .NET Framework FontStyle has: Regular, Bold, Italic, Underline, Strikeout
        public const FontStyle FontStyleRegular = FontStyle.Regular;
        public const FontStyle FontStyleBold = FontStyle.Bold;

        // Pre-defined font objects - reusable instances
        public static readonly Font DisplaySmall = new Font(FontFamily, FontSizeBase, FontStyleRegular);
        public static readonly Font DisplayMedium = new Font(FontFamily, FontSizeLG, FontStyleRegular);
        public static readonly Font DisplayLarge = new Font(FontFamily, FontSizeXL, FontStyleRegular);
        public static readonly Font HeadlineSmall = new Font(FontFamily, FontSize2XL, FontStyleBold);
        public static readonly Font HeadlineMedium = new Font(FontFamily, FontSize3XL, FontStyleBold);
        public static readonly Font HeadlineLarge = new Font(FontFamily, FontSize4XL, FontStyleBold);
        public static readonly Font TitleLarge = new Font(FontFamily, FontSizeBase, FontStyleBold);
        public static readonly Font BodyMedium = new Font(FontFamily, FontSizeBase, FontStyleRegular);
        public static readonly Font BodySmall = new Font(FontFamily, FontSizeSM, FontStyleRegular);
        public static readonly Font Caption = new Font(FontFamily, FontSizeXS, FontStyleRegular);
        public static readonly Font Overline = new Font(FontFamily, FontSizeXS, FontStyle.Regular);

        // Line height constants
        public const float LineHeightTight = 1.2F;
        public const float LineHeightNormal = 1.5F;
        public const float LineHeightRelaxed = 1.8F;
    }
}