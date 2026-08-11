using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Super_Shop_Management_System.Helpers
{
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 10;
        public Color BorderColor { get; set; } = ThemeManager.BorderLight;
        public int AccentBarWidth { get; set; } = 0;
        public Color AccentBarColor { get; set; } = ThemeManager.Primary;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = ThemeManager.PanelBackground;
            Tag = ThemeManager.ThemeExemptTag;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = RoundedRect(bounds, CornerRadius))
            using (SolidBrush backBrush = new SolidBrush(BackColor))
            using (Pen borderPen = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(backBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            if (AccentBarWidth > 0)
            {
                Rectangle accentRect = new Rectangle(0, 0, AccentBarWidth, Height);
                using (GraphicsPath accentPath = RoundedRect(accentRect, CornerRadius, roundRight: false))
                using (SolidBrush accentBrush = new SolidBrush(AccentBarColor))
                {
                    e.Graphics.FillPath(accentBrush, accentPath);
                }
            }

            base.OnPaint(e);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius, bool roundRight = true)
        {
            int d = radius * 2;
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            if (roundRight)
            {
                path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
                path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            }
            else
            {
                path.AddLine(bounds.Right, bounds.Y, bounds.Right, bounds.Bottom);
            }
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class RoundedButton : Button
    {
        public int CornerRadius { get; set; } = 8;

        public RoundedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Tag = ThemeManager.ThemeExemptTag;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            int d = CornerRadius * 2;
            using (var path = new GraphicsPath())
            {
                if (CornerRadius > 0)
                {
                    path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
                    path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
                    path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
                    path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();
                }
                else
                {
                    path.AddRectangle(bounds);
                }

                using (var backBrush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillPath(backBrush, path);
                }

                TextRenderer.DrawText(e.Graphics, Text, Font, bounds, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    public static class UIStyleKit
    {
        public static RoundedButton CreatePrimaryButton(string text, int width = 140, int height = 40)
        {
            var button = new RoundedButton
            {
                Text = text,
                Width = width,
                Height = height,
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                Font = ThemeManager.FontBodyBold,
                Cursor = Cursors.Hand,
                CornerRadius = 8
            };

            Color normal = ThemeManager.Primary;
            Color hover = ThemeManager.PrimaryDark;
            button.MouseEnter += (s, e) => button.BackColor = hover;
            button.MouseLeave += (s, e) => button.BackColor = normal;

            return button;
        }

        public static RoundedButton CreateSecondaryButton(string text, int width = 140, int height = 40)
        {
            var button = new RoundedButton
            {
                Text = text,
                Width = width,
                Height = height,
                BackColor = ThemeManager.PanelBackground,
                ForeColor = ThemeManager.Primary,
                Font = ThemeManager.FontBodyBold,
                Cursor = Cursors.Hand,
                CornerRadius = 8
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = ThemeManager.Primary;

            Color normal = ThemeManager.PanelBackground;
            Color hover = ThemeManager.CurrentTheme == AppTheme.Dark
                ? Color.FromArgb(45, 45, 45)
                : Color.FromArgb(235, 242, 247);
            button.MouseEnter += (s, e) => button.BackColor = hover;
            button.MouseLeave += (s, e) => button.BackColor = normal;

            return button;
        }

        public static RoundedPanel CreateStatCard(string title, string value, string glyph, Color accentColor, int width = 220, int height = 110)
        {
            var card = new RoundedPanel
            {
                Width = width,
                Height = height,
                AccentBarWidth = 6,
                AccentBarColor = accentColor,
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(ThemeManager.Spacing.MD + 6, ThemeManager.Spacing.SM, ThemeManager.Spacing.SM, ThemeManager.Spacing.SM)
            };

            var iconLabel = IconHelper.CreateIconLabel(glyph, 22F, accentColor);
            iconLabel.Location = new Point(card.Padding.Left, ThemeManager.Spacing.SM);
            card.Controls.Add(iconLabel);

            var valueLabel = new Label
            {
                Text = value,
                Font = ThemeManager.FontH1,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(card.Padding.Left, iconLabel.Bottom + ThemeManager.Spacing.SM)
            };
            card.Controls.Add(valueLabel);

            var titleLabel = new Label
            {
                Text = title,
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText,
                AutoSize = true,
                Location = new Point(card.Padding.Left, valueLabel.Bottom + 2)
            };
            card.Controls.Add(titleLabel);

            return card;
        }

        public static void ApplyRoundedRegion(Control control, int radius)
        {
            var bounds = new Rectangle(0, 0, control.Width, control.Height);
            int d = radius * 2;
            using (var path = new GraphicsPath())
            {
                path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
                path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
                path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                control.Region = new Region(path);
            }
        }
    }
}


