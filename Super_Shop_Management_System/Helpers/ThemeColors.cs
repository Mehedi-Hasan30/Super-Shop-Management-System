using System.Drawing;

namespace Super_Shop_Management_System.Helpers
{
    public static class ThemeColors
    {
        // Primary color palette - Segoe UI inspired enterprise blue
        public static readonly Color Primary = Color.FromArgb(41, 128, 185);
        public static readonly Color PrimaryDark = Color.FromArgb(31, 97, 141);
        public static readonly Color PrimaryLight = Color.FromArgb(220, 242, 247);
        public static readonly Color PrimaryHover = Color.FromArgb(48, 153, 212);

        // Accent colors
        public static readonly Color Success = Color.FromArgb(39, 174, 96);
        public static readonly Color SuccessLight = Color.FromArgb(232, 245, 233);
        public static readonly Color Warning = Color.FromArgb(243, 156, 18);
        public static readonly Color WarningLight = Color.FromArgb(255, 248, 235);
        public static readonly Color Danger = Color.FromArgb(231, 76, 60);
        public static readonly Color DangerLight = Color.FromArgb(252, 236, 235);
        public static readonly Color Info = Color.FromArgb(52, 152, 219);
        public static readonly Color InfoLight = Color.FromArgb(231, 237, 245);

        // Neutral colors - Light theme
        public static readonly Color Background = Color.FromArgb(244, 246, 248);
        public static readonly Color Surface = Color.FromArgb(255, 255, 255);
        public static readonly Color Card = Color.FromArgb(255, 255, 255);
        public static readonly Color Sidebar = Color.FromArgb(33, 47, 61);
        public static readonly Color SidebarInactive = Color.FromArgb(52, 73, 94);
        public static readonly Color SidebarHover = Color.FromArgb(41, 58, 74);

        // Border and divider colors
        public static readonly Color BorderLight = Color.FromArgb(225, 229, 233);
        public static readonly Color BorderDark = Color.FromArgb(60, 60, 60);
        public static readonly Color Divider = Color.FromArgb(200, 200, 200);

        // Text colors - Light theme
        public static readonly Color TextPrimary = Color.FromArgb(52, 73, 94);
        public static readonly Color TextSecondary = Color.FromArgb(127, 140, 141);
        public static readonly Color TextMuted = Color.FromArgb(155, 160, 165);
        public static readonly Color TextInverse = Color.FromArgb(255, 255, 255);

        // Status colors
        public static readonly Color StatusSuccess = Color.FromArgb(39, 174, 96);
        public static readonly Color StatusWarning = Color.FromArgb(243, 156, 18);
        public static readonly Color StatusDanger = Color.FromArgb(231, 76, 60);
        public static readonly Color StatusInfo = Color.FromArgb(52, 152, 219);
    }
}