using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public class RoundedControl : Control
    {
        public int CornerRadius { get; set; } = 10;
        public Color BorderColor { get; set; } = Color.FromArgb(225, 229, 233);
        public int BorderThickness { get; set; } = 1;
        public bool AnimateOnResize { get; set; } = false;

        public RoundedControl()
        {
            // Set default styles for modern appearance
            DoubleBuffered = true;
            Font = SystemFonts.MenuFont;
            ForeColor = Color.FromArgb(52, 73, 94);
            Margin = new Padding(4);
            Padding = new Padding(8, 6, 8, 6);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (AnimateOnResize)
            {
                SuspendLayout();
                Refresh();
                ResumeLayout();
            }
        }

        // Render rounded rectangle background
        protected void DrawRoundedBackground(PaintEventArgs e, Rectangle rect, Color backColor)
        {
            using (GraphicsPath path = GetRoundedPath(rect, CornerRadius))
            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }

        // Render rounded rectangle border
        protected void DrawRoundedBorder(PaintEventArgs e, Rectangle rect, Color borderColor, int thickness)
        {
            using (GraphicsPath path = GetRoundedPath(rect, CornerRadius))
            using (Pen pen = new Pen(borderColor, thickness))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        // Create GraphicsPath with rounded corners
        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // Top-left arc
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            // Top-right arc
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            // Bottom-right arc
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            // Bottom-left arc
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }

        // Apply rounded region to control
        public void ApplyRoundedRegion()
        {
            Region = new Region(GetRoundedPath(ClientRectangle, CornerRadius));
        }
    }
}