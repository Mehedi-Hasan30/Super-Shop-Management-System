using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _authService = new AuthService();

        private RoundedPanel _cardPanel;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private Button _btnTogglePassword;
        private CheckBox _chkRememberMe;
        private RoundedButton _btnLogin;
        private LinkLabel _linkForgotPassword;
        private RoundedPanel _errorBanner;
        private Label _lblErrorText;
        private bool _passwordVisible;

        private static string RememberedUserFilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SuperShopManagementSystem", "lastuser.txt");

        public LoginForm()
        {
            InitializeComponent();
            LoadRememberedUsername();
        }

        private void InitializeComponent()
        {
            Text = "Super Shop Management System - Login";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(480, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ThemeManager.ApplyFormTheme(this);

            _cardPanel = new RoundedPanel
            {
                Size = new Size(380, 500),
                Location = new Point(40, 50),
                CornerRadius = 14,
                BorderColor = ThemeManager.BorderColor
            };

            Label brandIcon = IconHelper.CreateIconLabel(IconHelper.Glyphs.Products, 28F, ThemeManager.Primary);
            brandIcon.Location = new Point(170, 28);
            Label brandText = new Label
            {
                Text = "Super Shop",
                Font = ThemeManager.FontH3,
                ForeColor = ThemeManager.Primary,
                AutoSize = true,
                Tag = ThemeManager.ThemeExemptTag
            };
            brandText.Location = new Point((_cardPanel.Width - TextRenderer.MeasureText(brandText.Text, brandText.Font).Width) / 2, 66);

            Label lblTitle = new Label
            {
                Text = "Welcome Back",
                Font = ThemeManager.FontDisplay,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Tag = ThemeManager.ThemeExemptTag
            };
            lblTitle.Location = new Point((_cardPanel.Width - TextRenderer.MeasureText(lblTitle.Text, lblTitle.Font).Width) / 2, 96);

            Label lblSubtitle = new Label
            {
                Text = "Sign in to continue to your dashboard",
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText,
                AutoSize = true,
                Tag = ThemeManager.ThemeExemptTag
            };
            lblSubtitle.Location = new Point((_cardPanel.Width - TextRenderer.MeasureText(lblSubtitle.Text, lblSubtitle.Font).Width) / 2, 132);

            _errorBanner = new RoundedPanel
            {
                Location = new Point(30, 164),
                Size = new Size(320, 40),
                CornerRadius = 8,
                AccentBarWidth = 4,
                AccentBarColor = ThemeManager.Danger,
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark
                    ? Color.FromArgb(60, 35, 33)
                    : Color.FromArgb(253, 237, 236),
                Visible = false
            };
            _lblErrorText = new Label
            {
                Text = string.Empty,
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.Danger,
                AutoSize = false,
                Size = new Size(290, 30),
                Location = new Point(16, 6),
                Tag = ThemeManager.ThemeExemptTag
            };
            _errorBanner.Controls.Add(_lblErrorText);

            Label lblUsername = CreateFieldLabel("Username", 218);
            _txtUsername = CreateTextBox(240, false);

            Label lblPassword = CreateFieldLabel("Password", 288);
            _txtPassword = CreateTextBox(310, true);

            _btnTogglePassword = new Button
            {
                Text = string.Empty,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(28, 28),
                Location = new Point(_txtPassword.Right - 30, _txtPassword.Top),
                BackColor = ThemeManager.ControlBackground,
                ForeColor = ThemeManager.MutedText,
                Cursor = Cursors.Hand,
                Tag = ThemeManager.ThemeExemptTag
            };
            _btnTogglePassword.FlatAppearance.BorderSize = 0;
            Font eyeFont = IconHelper.GlyphFont(11F);
            string eyeGlyphShow = IconHelper.GlyphOrFallback(IconHelper.Glyphs.EyeShow, "show");
            string eyeGlyphHide = IconHelper.GlyphOrFallback(IconHelper.Glyphs.EyeHide, "hide");
            _btnTogglePassword.Paint += (s, e) =>
            {
                TextRenderer.DrawText(e.Graphics, _passwordVisible ? eyeGlyphHide : eyeGlyphShow, eyeFont,
                    new Rectangle(0, 0, _btnTogglePassword.Width, _btnTogglePassword.Height), _btnTogglePassword.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            _btnTogglePassword.Click += (s, e) =>
            {
                _passwordVisible = !_passwordVisible;
                _txtPassword.UseSystemPasswordChar = !_passwordVisible;
                _btnTogglePassword.Invalidate();
            };

            _chkRememberMe = new CheckBox
            {
                Text = "Remember me",
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText,
                AutoSize = true,
                Location = new Point(30, 354),
                Tag = ThemeManager.ThemeExemptTag
            };

            _linkForgotPassword = new LinkLabel
            {
                Text = "Forgot Password?",
                Font = ThemeManager.FontCaption,
                AutoSize = true,
                Tag = ThemeManager.ThemeExemptTag
            };
            _linkForgotPassword.Location = new Point(_cardPanel.Width - 30 - TextRenderer.MeasureText(_linkForgotPassword.Text, _linkForgotPassword.Font).Width, 354);
            _linkForgotPassword.Click += LinkForgotPassword_Click;

            _btnLogin = UIStyleKit.CreatePrimaryButton("Login", 320, 42);
            _btnLogin.Location = new Point(30, 396);
            _btnLogin.Click += BtnLogin_Click;

            Label lblFooter = new Label
            {
                Text = "Authorized personnel only",
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText,
                AutoSize = true,
                Tag = ThemeManager.ThemeExemptTag
            };
            lblFooter.Location = new Point((_cardPanel.Width - TextRenderer.MeasureText(lblFooter.Text, lblFooter.Font).Width) / 2, 456);

            _cardPanel.Controls.Add(brandIcon);
            _cardPanel.Controls.Add(brandText);
            _cardPanel.Controls.Add(lblTitle);
            _cardPanel.Controls.Add(lblSubtitle);
            _cardPanel.Controls.Add(_errorBanner);
            _cardPanel.Controls.Add(lblUsername);
            _cardPanel.Controls.Add(_txtUsername);
            _cardPanel.Controls.Add(lblPassword);
            _cardPanel.Controls.Add(_txtPassword);
            _cardPanel.Controls.Add(_btnTogglePassword);
            _cardPanel.Controls.Add(_chkRememberMe);
            _cardPanel.Controls.Add(_linkForgotPassword);
            _cardPanel.Controls.Add(_btnLogin);
            _cardPanel.Controls.Add(lblFooter);

            Controls.Add(_cardPanel);
            AcceptButton = _btnLogin;
        }

        private Label CreateFieldLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.MutedText,
                AutoSize = true,
                Location = new Point(30, top),
                Tag = ThemeManager.ThemeExemptTag
            };
        }

        private TextBox CreateTextBox(int top, bool isPassword)
        {
            TextBox textBox = new TextBox
            {
                Location = new Point(30, top),
                Width = 320,
                Height = 28,
                Font = ThemeManager.FontBody,
                BackColor = ThemeManager.ControlBackground,
                ForeColor = ThemeManager.Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = isPassword,
                Tag = ThemeManager.ThemeExemptTag
            };
            return textBox;
        }

        private void LoadRememberedUsername()
        {
            try
            {
                if (File.Exists(RememberedUserFilePath))
                {
                    string savedUsername = File.ReadAllText(RememberedUserFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(savedUsername))
                    {
                        _txtUsername.Text = savedUsername;
                        _chkRememberMe.Checked = true;
                        _txtPassword.Focus();
                    }
                }
            }
            catch
            {
            }
        }

        private void SaveRememberedUsername()
        {
            try
            {
                string folder = Path.GetDirectoryName(RememberedUserFilePath);
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (_chkRememberMe.Checked)
                {
                    File.WriteAllText(RememberedUserFilePath, _txtUsername.Text.Trim());
                }
                else if (File.Exists(RememberedUserFilePath))
                {
                    File.Delete(RememberedUserFilePath);
                }
            }
            catch
            {
            }
        }

        private void ShowError(string message)
        {
            _lblErrorText.Text = message;
            _errorBanner.Visible = true;
        }

        private void HideError()
        {
            _errorBanner.Visible = false;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            HideError();

            string username = _txtUsername.Text;
            string password = _txtPassword.Text;

            _btnLogin.Enabled = false;
            _btnLogin.Text = "Signing in...";
            UseWaitCursor = true;

            try
            {
                User user = await Task.Run(() => _authService.Login(username, password));

                SaveRememberedUsername();

DashboardShell dashboardShell = new DashboardShell(user.Role);
                dashboardShell.FormClosed += (_, __) => Show();
                Hide();
                dashboardShell.Show();
                _txtPassword.Clear();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                _btnLogin.Enabled = true;
                _btnLogin.Text = "Login";
                UseWaitCursor = false;
            }
        }

        private void LinkForgotPassword_Click(object sender, EventArgs e)
        {
            using (ForgotPasswordForm forgotPasswordForm = new ForgotPasswordForm())
            {
                forgotPasswordForm.ShowDialog();
            }
        }
    }
}
