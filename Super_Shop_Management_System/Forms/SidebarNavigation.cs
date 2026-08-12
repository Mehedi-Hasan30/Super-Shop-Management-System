using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Animations;

namespace Super_Shop_Management_System.Forms
{
    public class SidebarNavigation : UserControl
    {
        public event EventHandler<string> NavButtonClicked;

        private readonly string _userRole;
        private readonly FlowLayoutPanel _flow;
        private readonly RoundedButton _toggleButton;
        private bool _isCollapsed;
        private string _lastSelectedModule = "";
        private const int CollapsedWidth = 60;
        private const int ExpandedWidth = 240;

        public SidebarNavigation(string userRole)
        {
            _userRole = userRole;
            _isCollapsed = false;

            Dock = DockStyle.Left;
            MinimumSize = new Size(CollapsedWidth, 0);
            MaximumSize = new Size(ExpandedWidth, 0);
            BackColor = ThemeManager.Sidebar;
            Tag = ThemeManager.ThemeExemptTag;

            // Toggle button for collapse/expand
            _toggleButton = new RoundedButton
            {
                Text = "≡",
                Width = 36,
                Height = 36,
                Location = new Point(CollapsedWidth - 40, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                CornerRadius = 6
            };
            _toggleButton.FlatAppearance.BorderSize = 0;
            UIStyleKit.ApplyRoundedRegion(_toggleButton, 6);
            _toggleButton.MouseEnter += (s, e) => _toggleButton.BackColor = ThemeManager.SidebarButtonHover;
            _toggleButton.MouseLeave += (s, e) => _toggleButton.BackColor = Color.Transparent;
            _toggleButton.Click += ToggleButton_Click;

            // Main flow panel
            _flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(_isCollapsed ? 4 : 16, _isCollapsed ? 16 : 16, 4, 16)
            };
            Controls.Add(_toggleButton);
            Controls.Add(_flow);

            // Add navigation buttons
            AddButton("Dashboard", IconHelper.Glyphs.Dashboard, "Dashboard");
            AddButton("Products", IconHelper.Glyphs.Products, "Products");
            AddButton("Customers", IconHelper.Glyphs.Customers, "Customers");
            AddButton("Sales", IconHelper.Glyphs.Sales, "Sales");

            if (_userRole == "Admin" || _userRole == "Manager")
            {
                AddButton("Categories", IconHelper.Glyphs.Inventory, "Categories");
                AddButton("Suppliers", IconHelper.Glyphs.Suppliers, "Suppliers");
            }

            AddButton("Employees", IconHelper.Glyphs.Employees, "Employees");
            AddButton("Attendance", IconHelper.Glyphs.Attendance, "Attendance");
            AddButton("Reports", IconHelper.Glyphs.Reports, "Reports");
            AddButton("Settings", IconHelper.Glyphs.Settings, "Settings");

            // Initial width animation
            Transition.AnimateWidth(this, Width, _isCollapsed ? CollapsedWidth : ExpandedWidth, 200);
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            _isCollapsed = !_isCollapsed;
            _toggleButton.Text = _isCollapsed ? "≡" : "✕";
            _flow.Padding = new Padding(_isCollapsed ? 4 : 16, _isCollapsed ? 16 : 16, 4, 16);

            // Animate width change
            Transition.AnimateWidth(this, Width, _isCollapsed ? CollapsedWidth : ExpandedWidth, 250);

            // Adjust sidebar controls visibility when collapsed
            foreach (Control ctrl in _flow.Controls)
            {
                if (ctrl is RoundedButton btn)
                {
                    btn.Visible = !_isCollapsed;
                }
            }
        }

        private void AddButton(string text, string glyph, string moduleName)
        {
            var btn = new RoundedButton
            {
                Text = string.Empty,
                Width = _isCollapsed ? 32 : 208,
                Height = _isCollapsed ? 32 : 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Sidebar,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            // Add appropriate rounding based on collapse state
            int radius = _isCollapsed ? 6 : 6;
            UIStyleKit.ApplyRoundedRegion(btn, radius);

            Font iconFont = IconHelper.GlyphFont(12F);
            string iconGlyph = IconHelper.GlyphOrFallback(glyph, "-");

            btn.Paint += (s, e) =>
            {
                if (_isCollapsed)
                {
                    // Show only icon in collapsed mode
                    TextRenderer.DrawText(e.Graphics, iconGlyph, iconFont,
                        new Rectangle(8, 8, 24, btn.Height), btn.ForeColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                }
                else
                {
                    // Show icon + text in expanded mode
                    TextRenderer.DrawText(e.Graphics, iconGlyph, iconFont,
                        new Rectangle(14, 0, 24, btn.Height), btn.ForeColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(e.Graphics, text, ThemeManager.FontBody,
                        new Rectangle(44, 0, btn.Width - 50, btn.Height), btn.ForeColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
                }
            };

            Color normal = ThemeManager.Sidebar;
            Color hover = ThemeManager.SidebarButtonHover;
            Color selected = ThemeManager.Primary;  // Active selection color
            btn.MouseEnter += (s, e) =>
            {
                if (!_isCollapsed && btn.Enabled)
                {
                    btn.BackColor = hover;
                    btn.Invalidate();
                }
            };
            btn.MouseLeave += (s, e) =>
            {
                if (!_isCollapsed && btn.Enabled)
                {
                    // Restore normal or selected color
                    btn.BackColor = _lastSelectedModule == moduleName ? selected : normal;
                    btn.Invalidate();
                }
            };
            // Add click handler with selection tracking
            btn.Click += (s, e) =>
            {
                _lastSelectedModule = moduleName;
                NavButtonClicked?.Invoke(this, moduleName);
                // Visual feedback: briefly change to selected color then revert
                var originalColor = btn.BackColor;
                btn.BackColor = selected;
                btn.Invalidate();
                Task.Delay(150).ContinueWith(_ =>
                {
                    if (btn.InvokeRequired) btn.Invoke((MethodInvoker)(() => btn.BackColor = originalColor));
                    else btn.BackColor = originalColor;
                });
            };

            _flow.Controls.Add(btn);
        }

        public void ApplyTheme()
        {
            BackColor = ThemeManager.Sidebar;
            _toggleButton.BackColor = Color.Transparent;
            _toggleButton.ForeColor = Color.White;

            foreach (Control ctrl in _flow.Controls)
            {
                if (ctrl is RoundedButton btn)
                {
                    btn.BackColor = ThemeManager.Sidebar;
                    btn.ForeColor = Color.White;
                }
            }
        }
    }
}