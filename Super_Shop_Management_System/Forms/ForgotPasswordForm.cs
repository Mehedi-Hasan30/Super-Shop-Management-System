using System;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class ForgotPasswordForm : Form
    {
        private readonly AuthService _authService = new AuthService();

        private TextBox _txtUsername;
        private TextBox _txtEmail;
        private Label _lblQuestion;
        private TextBox _txtAnswer;
        private TextBox _txtNewPassword;
        private Button _btnVerify;
        private Button _btnReset;

        private User _verifiedUser;

        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Forgot Password";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(500, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label lblUsername = new Label { Text = "Username", Location = new Point(40, 30), AutoSize = true };
            _txtUsername = new TextBox { Location = new Point(40, 50), Width = 400 };

            Label lblEmail = new Label { Text = "Email", Location = new Point(40, 85), AutoSize = true };
            _txtEmail = new TextBox { Location = new Point(40, 105), Width = 400 };

            _btnVerify = new Button
            {
                Text = "Verify User",
                Location = new Point(40, 145),
                Width = 120
            };
            _btnVerify.Click += BtnVerify_Click;

            _lblQuestion = new Label
            {
                Text = "Security Question: -",
                Location = new Point(40, 185),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            Label lblAnswer = new Label { Text = "Security Answer", Location = new Point(40, 215), AutoSize = true };
            _txtAnswer = new TextBox { Location = new Point(40, 235), Width = 190, Enabled = false };

            Label lblNewPassword = new Label { Text = "New Password", Location = new Point(250, 215), AutoSize = true };
            _txtNewPassword = new TextBox { Location = new Point(250, 235), Width = 190, PasswordChar = '*', Enabled = false };

            _btnReset = new Button
            {
                Text = "Reset Password",
                Location = new Point(170, 275),
                Width = 150,
                Enabled = false
            };
            _btnReset.Click += BtnReset_Click;

            Controls.Add(lblUsername);
            Controls.Add(_txtUsername);
            Controls.Add(lblEmail);
            Controls.Add(_txtEmail);
            Controls.Add(_btnVerify);
            Controls.Add(_lblQuestion);
            Controls.Add(lblAnswer);
            Controls.Add(_txtAnswer);
            Controls.Add(lblNewPassword);
            Controls.Add(_txtNewPassword);
            Controls.Add(_btnReset);
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            try
            {
                _verifiedUser = _authService.FindUserForPasswordReset(_txtUsername.Text, _txtEmail.Text);
                _lblQuestion.Text = $"Security Question: {_verifiedUser.SecurityQuestion}";
                _txtAnswer.Enabled = true;
                _txtNewPassword.Enabled = true;
                _btnReset.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            try
            {
                bool success = _authService.ResetPassword(
                    _verifiedUser.UserID,
                    _txtAnswer.Text,
                    _txtNewPassword.Text,
                    _verifiedUser.SecurityAnswer);

                if (!success)
                {
                    MessageBox.Show("Password reset failed.", "Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Password reset successful.", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
