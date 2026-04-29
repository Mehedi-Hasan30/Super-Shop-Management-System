using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Reports
{
    public class AuditLogViewerForm : Form
    {
        private readonly AuditLogService _auditLogService = new AuditLogService();

        private ComboBox _cmbActionType;
        private ComboBox _cmbTableName;
        private ComboBox _cmbUser;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private DataGridView _grid;
        private Label _lblCount;

        private List<AuditLog> _currentLogs = new List<AuditLog>();

        public AuditLogViewerForm()
        {
            InitializeComponent();
            Load += AuditLogViewerForm_Load;
        }

        private async void AuditLogViewerForm_Load(object sender, EventArgs e)
        {
            await LoadFilterSourcesAsync();
            await LoadLogsAsync();
        }

        private void InitializeComponent()
        {
            Text = "Audit Log Viewer";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1220, 700);
            ThemeManager.ApplyFormTheme(this);

            Panel filterPanel = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(1170, 112),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _cmbActionType = new ComboBox { Location = new Point(15, 32), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbTableName = new ComboBox { Location = new Point(205, 32), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbUser = new ComboBox { Location = new Point(395, 32), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _dtFrom = new DateTimePicker { Location = new Point(585, 32), Width = 130, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _dtTo = new DateTimePicker { Location = new Point(725, 32), Width = 130, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            Button btnSearch = new Button { Text = "Search", Location = new Point(870, 30), Width = 85 };
            Button btnRefresh = new Button { Text = "Refresh", Location = new Point(960, 30), Width = 85 };
            Button btnClear = new Button { Text = "Clear", Location = new Point(1050, 30), Width = 85 };
            Button btnExport = new Button { Text = "Export CSV", Location = new Point(870, 68), Width = 120 };

            FormDesignHelper.ApplyCrudButtonStyle(btnSearch, ThemeManager.Primary);
            FormDesignHelper.ApplyCrudButtonStyle(btnRefresh, Color.FromArgb(52, 73, 94));
            FormDesignHelper.ApplyCrudButtonStyle(btnClear, Color.FromArgb(192, 57, 43));
            FormDesignHelper.ApplyCrudButtonStyle(btnExport, Color.FromArgb(39, 174, 96));

            btnSearch.Click += async (_, __) => await LoadLogsAsync();
            btnRefresh.Click += async (_, __) => await LoadLogsAsync();
            btnClear.Click += async (_, __) => await ClearFiltersAsync();
            btnExport.Click += (_, __) => ExportToCsv();

            _cmbActionType.SelectedIndexChanged += async (_, __) => await LoadLogsAsync();
            _cmbTableName.SelectedIndexChanged += async (_, __) => await LoadLogsAsync();
            _cmbUser.SelectedIndexChanged += async (_, __) => await LoadLogsAsync();
            _dtFrom.ValueChanged += async (_, __) => await LoadLogsAsync();
            _dtTo.ValueChanged += async (_, __) => await LoadLogsAsync();

            filterPanel.Controls.Add(new Label { Text = "Action Type", Location = new Point(15, 10), AutoSize = true });
            filterPanel.Controls.Add(_cmbActionType);
            filterPanel.Controls.Add(new Label { Text = "Table Name", Location = new Point(205, 10), AutoSize = true });
            filterPanel.Controls.Add(_cmbTableName);
            filterPanel.Controls.Add(new Label { Text = "User", Location = new Point(395, 10), AutoSize = true });
            filterPanel.Controls.Add(_cmbUser);
            filterPanel.Controls.Add(new Label { Text = "From Date", Location = new Point(585, 10), AutoSize = true });
            filterPanel.Controls.Add(_dtFrom);
            filterPanel.Controls.Add(new Label { Text = "To Date", Location = new Point(725, 10), AutoSize = true });
            filterPanel.Controls.Add(_dtTo);
            filterPanel.Controls.Add(btnSearch);
            filterPanel.Controls.Add(btnRefresh);
            filterPanel.Controls.Add(btnClear);
            filterPanel.Controls.Add(btnExport);

            _lblCount = new Label { Location = new Point(15, 132), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            _grid = new DataGridView
            {
                Location = new Point(15, 160),
                Width = 1170,
                Height = 490,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            BaseGridStyler.Apply(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AuditLogID", HeaderText = "ID", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ActionType", HeaderText = "Action", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TableName", HeaderText = "Table", Width = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RecordID", HeaderText = "Record ID", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UserName", HeaderText = "User", Width = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Timestamp", HeaderText = "Timestamp", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Description", Width = 410 });
            _grid.CellFormatting += Grid_CellFormatting;

            Controls.Add(filterPanel);
            Controls.Add(_lblCount);
            Controls.Add(_grid);
        }

        private async Task LoadFilterSourcesAsync()
        {
            try
            {
                _cmbActionType.Items.Clear();
                _cmbActionType.Items.Add("All");
                _cmbActionType.Items.Add("INSERT");
                _cmbActionType.Items.Add("UPDATE");
                _cmbActionType.Items.Add("DELETE");
                _cmbActionType.Items.Add("LOGIN_SUCCESS");
                _cmbActionType.Items.Add("LOGIN_FAIL");
                _cmbActionType.Items.Add("LOGOUT");
                _cmbActionType.SelectedIndex = 0;

                List<string> tableNames = await _auditLogService.GetDistinctTableNamesAsync();
                _cmbTableName.Items.Clear();
                _cmbTableName.Items.Add("All");
                foreach (string table in tableNames)
                {
                    _cmbTableName.Items.Add(table);
                }
                _cmbTableName.SelectedIndex = 0;

                List<string> users = await _auditLogService.GetDistinctUsersAsync();
                _cmbUser.Items.Clear();
                _cmbUser.Items.Add("All");
                foreach (string user in users)
                {
                    _cmbUser.Items.Add(user);
                }
                _cmbUser.SelectedIndex = 0;

                _dtFrom.Checked = false;
                _dtTo.Checked = false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("AuditLogViewer.LoadFilterSourcesAsync", ex);
                MessageBox.Show($"Failed to load filter data: {ex.Message}", "Audit Viewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadLogsAsync()
        {
            try
            {
                string actionType = GetFilterValue(_cmbActionType);
                string tableName = GetFilterValue(_cmbTableName);
                string userName = GetFilterValue(_cmbUser);
                DateTime? fromDate = _dtFrom.Checked ? _dtFrom.Value.Date : (DateTime?)null;
                DateTime? toDate = _dtTo.Checked ? _dtTo.Value.Date : (DateTime?)null;

                _currentLogs = await _auditLogService.GetFilteredAsync(actionType, tableName, userName, fromDate, toDate, 1000);
                _grid.DataSource = null;
                _grid.DataSource = _currentLogs;
                _lblCount.Text = $"Total Logs: {_currentLogs.Count}";
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("AuditLogViewer.LoadLogsAsync", ex);
                MessageBox.Show($"Failed to load logs: {ex.Message}", "Audit Viewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task ClearFiltersAsync()
        {
            _cmbActionType.SelectedIndex = 0;
            _cmbTableName.SelectedIndex = 0;
            _cmbUser.SelectedIndex = 0;
            _dtFrom.Checked = false;
            _dtTo.Checked = false;
            await LoadLogsAsync();
        }

        private void ExportToCsv()
        {
            if (_currentLogs.Count == 0)
            {
                MessageBox.Show("No logs available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV Files (*.csv)|*.csv";
                dialog.FileName = $"AuditLogs_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("AuditLogID,ActionType,TableName,RecordID,User,Timestamp,Description");
                foreach (AuditLog log in _currentLogs)
                {
                    builder.AppendLine($"{log.AuditLogID},\"{Escape(log.ActionType)}\",\"{Escape(log.TableName)}\",\"{Escape(log.RecordID)}\",\"{Escape(log.UserName)}\",\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{Escape(log.Description)}\"");
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Audit logs exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!(_grid.Rows[e.RowIndex].DataBoundItem is AuditLog log))
            {
                return;
            }

            if (string.Equals(log.ActionType, "LOGIN_FAIL", StringComparison.OrdinalIgnoreCase))
            {
                _grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(252, 228, 236);
                _grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
            }
            else
            {
                _grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = ThemeManager.GridBackground;
                _grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = ThemeManager.GridText;
            }
        }

        private static string GetFilterValue(ComboBox comboBox)
        {
            if (comboBox.SelectedItem == null)
            {
                return string.Empty;
            }

            string value = comboBox.SelectedItem.ToString();
            return string.Equals(value, "All", StringComparison.OrdinalIgnoreCase) ? string.Empty : value;
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\"", "\"\"");
        }
    }
}
