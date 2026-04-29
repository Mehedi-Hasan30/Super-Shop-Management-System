using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Reports
{
    public class SmokeTestReportForm : Form
    {
        private readonly SmokeTestRunner _runner = new SmokeTestRunner();
        private DataGridView _grid;
        private Label _lblResult;

        public SmokeTestReportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "QA Smoke Test Report";
            Size = new Size(900, 560);
            StartPosition = FormStartPosition.CenterParent;
            ThemeManager.ApplyFormTheme(this);

            Button btnRun = new Button { Text = "Run Smoke Tests", Location = new Point(20, 20), Width = 160 };
            FormDesignHelper.ApplyCrudButtonStyle(btnRun, ThemeManager.Primary);
            btnRun.Click += async (_, __) => await RunTestsAsync();

            _lblResult = new Label { Text = "Status: Not Started", Location = new Point(200, 26), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            _grid = new DataGridView
            {
                Location = new Point(20, 60),
                Width = 840,
                Height = 440,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            BaseGridStyler.Apply(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TestName", HeaderText = "Test", Width = 220 });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "IsPassed", HeaderText = "Passed", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Details", HeaderText = "Details", Width = 520 });

            Controls.Add(btnRun);
            Controls.Add(_lblResult);
            Controls.Add(_grid);
        }

        private async Task RunTestsAsync()
        {
            List<SmokeTestResult> results = await _runner.RunAsync();
            _grid.DataSource = results;
            bool pass = SmokeTestRunner.IsOverallPass(results);
            _lblResult.Text = pass ? "Status: PASS" : "Status: FAIL";
            _lblResult.ForeColor = pass ? Color.DarkGreen : Color.DarkRed;
        }
    }
}
