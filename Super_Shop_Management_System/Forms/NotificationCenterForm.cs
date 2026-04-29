using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class NotificationCenterForm : Form
    {
        private readonly NotificationService _notificationService = new NotificationService();

        private DataGridView _grid;
        private Button _btnRefresh;
        private Button _btnClose;
        private Label _lblCount;

        private List<NotificationItem> _notifications = new List<NotificationItem>();

        public NotificationCenterForm()
        {
            InitializeComponent();
            ThemeManager.ApplyFormTheme(this);
            Load += async (_, __) => await LoadNotificationsAsync();
        }

        private void InitializeComponent()
        {
            Text = "Notification Center";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 650);

            Panel top = new Panel { Location = new Point(15, 15), Size = new Size(935, 70), BorderStyle = BorderStyle.FixedSingle };

            _btnRefresh = new Button { Text = "Refresh", Location = new Point(20, 18), Width = 120, Height = 35 };
            _btnClose = new Button { Text = "Close", Location = new Point(720, 18), Width = 120, Height = 35 };
            _lblCount = new Label { Text = "Notifications: 0", Location = new Point(160, 24), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            FormDesignHelper.ApplyCrudButtonStyle(_btnRefresh, ThemeManager.Primary);
            FormDesignHelper.ApplyCrudButtonStyle(_btnClose, Color.FromArgb(127, 140, 141));
            _btnRefresh.Click += async (_, __) => await LoadNotificationsAsync();
            _btnClose.Click += (_, __) => Close();

            top.Controls.Add(_btnRefresh);
            top.Controls.Add(_btnClose);
            top.Controls.Add(_lblCount);

            _grid = new DataGridView
            {
                Location = new Point(15, 95),
                Size = new Size(935, 505),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            BaseGridStyler.Apply(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Type", HeaderText = "Type", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Title", Width = 260 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Message", HeaderText = "Message", Width = 440 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Severity", HeaderText = "Severity", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreatedAt", HeaderText = "Created", Width = 160 });

            _grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (!(_grid.Rows[e.RowIndex].DataBoundItem is NotificationItem item)) return;
                if (e.ColumnIndex == 4)
                {
                    e.Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                }
            };

            Controls.Add(top);
            Controls.Add(_grid);
        }

        private async Task LoadNotificationsAsync()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                UseWaitCursor = true;
                _notifications = await _notificationService.GetDashboardNotificationsAsync(50);
                _grid.DataSource = null;
                _grid.DataSource = _notifications;
                _lblCount.Text = $"Notifications: {_notifications.Count}";
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("NotificationCenterForm.LoadNotificationsAsync", ex);
                MessageBox.Show(ex.Message, "Notifications", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
            }
        }
    }
}

