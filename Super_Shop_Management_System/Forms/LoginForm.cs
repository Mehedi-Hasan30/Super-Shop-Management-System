using System;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _authService = new AuthService();

        private Panel _cardPanel;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private Button _btnLogin;
        private LinkLabel _linkForgotPassword;
        private Label _lblMessage;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Super Shop Management System - Login";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(460, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);

            _cardPanel = new Panel
            {
                Size = new Size(360, 250),
                Location = new Point(45, 35),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "Welcome Back",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = true,
                Location = new Point(105, 20)
            };

            Label lblUsername = new Label { Text = "Username", Location = new Point(35, 70), AutoSize = true };
            _txtUsername = new TextBox { Location = new Point(35, 90), Width = 290 };

            Label lblPassword = new Label { Text = "Password", Location = new Point(35, 125), AutoSize = true };
            _txtPassword = new TextBox { Location = new Point(35, 145), Width = 290, PasswordChar = '*' };

            _btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(35, 185),
                Width = 290,
                Height = 34,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += BtnLogin_Click;

            _linkForgotPassword = new LinkLabel
            {
                Text = "Forgot Password?",
                Location = new Point(230, 225),
                AutoSize = true
            };
            _linkForgotPassword.Click += LinkForgotPassword_Click;

            _lblMessage = new Label
            {
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Location = new Point(35, 228)
            };

            _cardPanel.Controls.Add(lblTitle);
            _cardPanel.Controls.Add(lblUsername);
            _cardPanel.Controls.Add(_txtUsername);
            _cardPanel.Controls.Add(lblPassword);
            _cardPanel.Controls.Add(_txtPassword);
            _cardPanel.Controls.Add(_btnLogin);
            _cardPanel.Controls.Add(_linkForgotPassword);
            _cardPanel.Controls.Add(_lblMessage);

            Controls.Add(_cardPanel);
            AcceptButton = _btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                _lblMessage.Text = string.Empty;
                User user = _authService.Login(_txtUsername.Text, _txtPassword.Text);
                DashboardForm dashboardForm = new DashboardForm(user.Role);
                dashboardForm.FormClosed += (_, __) => Show();
                Hide();
                dashboardForm.Show();
                _txtPassword.Clear();
            }
            catch (Exception ex)
            {
                _lblMessage.Text = ex.Message;
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
