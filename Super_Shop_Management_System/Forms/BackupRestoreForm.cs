using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;

namespace Super_Shop_Management_System.Forms
{
    public class BackupRestoreForm : Form
    {
        private readonly BackupService _backupService = new BackupService();

        private TextBox _txtBackupPath;
        private Button _btnBrowseBackup;
        private Button _btnBackupNow;

        private TextBox _txtRestorePath;
        private Button _btnBrowseRestore;
        private Button _btnRestoreNow;

        private Label _lblHint;

        public BackupRestoreForm()
        {
            InitializeComponent();
            ThemeManager.ApplyFormTheme(this);

            bool isAdmin = string.Equals(SessionManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                MessageBox.Show("Admin access required to perform backup/restore.", "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            Load += BackupRestoreForm_Load;
        }

        private void BackupRestoreForm_Load(object sender, EventArgs e)
        {
            _txtBackupPath.Text = _backupService.BuildDefaultBackupFilePath();
        }

        private void InitializeComponent()
        {
            Text = "Backup & Restore (Admin)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(720, 430);

            Panel backupPanel = new Panel { Location = new Point(15, 15), Size = new Size(690, 160), BorderStyle = BorderStyle.FixedSingle };
            Panel restorePanel = new Panel { Location = new Point(15, 190), Size = new Size(690, 150), BorderStyle = BorderStyle.FixedSingle };

            _lblHint = new Label
            {
                Location = new Point(20, 10),
                AutoSize = true,
                Text = "Backup creates a .bak file. Restore overwrites the current database.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            // Backup controls
            backupPanel.Controls.Add(new Label { Text = "Backup File (.bak)", Location = new Point(15, 20), AutoSize = true });
            _txtBackupPath = new TextBox { Location = new Point(15, 45), Width = 450 };
            _btnBrowseBackup = new Button { Text = "Browse", Location = new Point(475, 43), Width = 80, Height = 28 };
            _btnBackupNow = new Button { Text = "Backup Now", Location = new Point(565, 43), Width = 110, Height = 28 };

            FormDesignHelper.ApplyCrudButtonStyle(_btnBrowseBackup, Color.FromArgb(52, 73, 94));
            FormDesignHelper.ApplyCrudButtonStyle(_btnBackupNow, ThemeManager.Primary);
            _btnBrowseBackup.Click += BtnBrowseBackup_Click;
            _btnBackupNow.Click += async (_, __) => await BackupNowAsync();

            backupPanel.Controls.Add(_txtBackupPath);
            backupPanel.Controls.Add(_btnBrowseBackup);
            backupPanel.Controls.Add(_btnBackupNow);

            // Restore controls
            restorePanel.Controls.Add(new Label { Text = "Restore From (.bak)", Location = new Point(15, 20), AutoSize = true });
            _txtRestorePath = new TextBox { Location = new Point(15, 45), Width = 450 };
            _btnBrowseRestore = new Button { Text = "Browse", Location = new Point(475, 43), Width = 80, Height = 28 };
            _btnRestoreNow = new Button { Text = "Restore Now", Location = new Point(565, 43), Width = 110, Height = 28 };

            FormDesignHelper.ApplyCrudButtonStyle(_btnBrowseRestore, Color.FromArgb(52, 73, 94));
            FormDesignHelper.ApplyCrudButtonStyle(_btnRestoreNow, Color.FromArgb(192, 57, 43));
            _btnBrowseRestore.Click += BtnBrowseRestore_Click;
            _btnRestoreNow.Click += async (_, __) => await RestoreNowAsync();

            restorePanel.Controls.Add(_txtRestorePath);
            restorePanel.Controls.Add(_btnBrowseRestore);
            restorePanel.Controls.Add(_btnRestoreNow);

            Controls.Add(_lblHint);
            Controls.Add(backupPanel);
            Controls.Add(restorePanel);
        }

        private void BtnBrowseBackup_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "SQL Backup (.bak)|*.bak";
                dialog.FileName = $"SuperShopDB_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _txtBackupPath.Text = dialog.FileName;
                }
            }
        }

        private void BtnBrowseRestore_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "SQL Backup (.bak)|*.bak";
                dialog.Multiselect = false;
                dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _txtRestorePath.Text = dialog.FileName;
                }
            }
        }

        private async Task BackupNowAsync()
        {
            string path = _txtBackupPath.Text?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Select a backup file location first.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Create full SQL database backup now?", "Confirm Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                UseWaitCursor = true;
                DisableButtons(true);
                string created = await _backupService.CreateFullBackupAsync(path);
                MessageBox.Show($"Backup completed successfully.\n{created}", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Backup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                DisableButtons(false);
                Cursor.Current = Cursors.Default;
            }
        }

        private async Task RestoreNowAsync()
        {
            string path = _txtRestorePath.Text?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Select a .bak file to restore from.", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Restore will overwrite the current database. Continue?", "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                UseWaitCursor = true;
                DisableButtons(true);
                await _backupService.RestoreBackupAsync(path);
                MessageBox.Show("Restore completed successfully. Restart may be required for some open screens.", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Restore Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                DisableButtons(false);
                Cursor.Current = Cursors.Default;
            }
        }

        private void DisableButtons(bool isDisabled)
        {
            _btnBrowseBackup.Enabled = !isDisabled;
            _btnBackupNow.Enabled = !isDisabled;
            _btnBrowseRestore.Enabled = !isDisabled;
            _btnRestoreNow.Enabled = !isDisabled;
            _txtBackupPath.Enabled = !isDisabled;
            _txtRestorePath.Enabled = !isDisabled;
        }
    }
}

