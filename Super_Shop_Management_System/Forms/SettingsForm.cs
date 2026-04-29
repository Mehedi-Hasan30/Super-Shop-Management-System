using System;
using System.Windows.Forms;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public class SettingsForm : Form
    {
        private CheckBox _chkDarkMode;
        private Button _btnClose;

        public SettingsForm()
        {
            InitializeComponent();
            ThemeManager.ApplyFormTheme(this);
            Load += (_, __) => SyncUiFromTheme();
        }

        private void InitializeComponent()
        {
            Text = "Settings";
            StartPosition = FormStartPosition.CenterParent;
            Size = new System.Drawing.Size(480, 220);

            _chkDarkMode = new CheckBox
            {
                Text = "Enable Dark Mode",
                AutoSize = true,
                Location = new System.Drawing.Point(25, 25)
            };

            _chkDarkMode.CheckedChanged += (_, __) =>
            {
                ThemeManager.SetTheme(_chkDarkMode.Checked ? AppTheme.Dark : AppTheme.Light);
                foreach (Form open in Application.OpenForms)
                {
                    try { ThemeManager.ApplyFormTheme(open); }
                    catch { /* ignore */ }
                }
            };

            _btnClose = new Button
            {
                Text = "Close",
                Width = 120,
                Height = 34,
                Location = new System.Drawing.Point(320, 120)
            };
            FormDesignHelper.ApplyCrudButtonStyle(_btnClose, System.Drawing.Color.FromArgb(127, 140, 141));
            _btnClose.Click += (_, __) => Close();

            Controls.Add(_chkDarkMode);
            Controls.Add(_btnClose);
        }

        private void SyncUiFromTheme()
        {
            _chkDarkMode.Checked = ThemeManager.CurrentTheme == AppTheme.Dark;
        }
    }
}

