using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class CategoryUserControl : UserControl
    {
        private readonly CategoryService _categoryService = new CategoryService();

        private DataGridView _grid;
        private TextBox _txtName;
        private TextBox _txtDescription;
        private TextBox _txtSearch;

        public CategoryUserControl()
        {
            Dock = DockStyle.Fill;
            BackColor = ThemeManager.Background;
            Tag = ThemeManager.ThemeExemptTag;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Premium header panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 80),
                BackColor = ThemeManager.Sidebar,
                Padding = new Padding(0)
            };

            var iconLabel = IconHelper.CreateIconLabel(IconHelper.Glyphs.Inventory, 32F, ThemeManager.Primary);
            iconLabel.Location = new Point(20, 20);
            headerPanel.Controls.Add(iconLabel);

            var title = new Label
            {
                Text = "Category Management",
                Font = ThemeManager.FontH2,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(60, 25)
            };
            headerPanel.Controls.Add(title);

            var separator = new Panel
            {
                Location = new Point(60, 55),
                Size = new Size(150, 1),
                BackColor = ThemeManager.BorderColor
            };
            headerPanel.Controls.Add(separator);

            // Input panel card
            var inputCard = new RoundedPanel
            {
                Location = new Point(20, 90),
                Size = new Size(520, 130),
                BorderColor = ThemeManager.BorderColor,
                Padding = new Padding(16)
            };

            _txtName = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, inputCard.Padding.Top),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            _txtDescription = new TextBox
            {
                Location = new Point(inputCard.Padding.Left, _txtName.Bottom + 12),
                Size = new Size(260, 30),
                BorderStyle = BorderStyle.None,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.MutedText
            };

            var btnAdd = UIStyleKit.CreateButton(ButtonStyle.Primary, "Add", 80, 36);
            btnAdd.Location = new Point(inputCard.Padding.Left + 260 + 16, 20);
            btnAdd.Click += BtnAdd_Click;
            inputCard.Controls.Add(_txtName);
            inputCard.Controls.Add(_txtDescription);
            inputCard.Controls.Add(btnAdd);

            // Stats cards row
            var statsPanel = new Panel
            {
                Location = new Point(20, 230),
                Size = new Size(520, 50),
                BackColor = Color.Transparent
            };

            var categoryCard = UIStyleKit.CreateStatCard("Categories", "0", IconHelper.Glyphs.Inventory, ThemeManager.Primary, 180, 40);
            categoryCard.Location = new Point(20, 10);
            statsPanel.Controls.Add(categoryCard);

            // Grid area
            _grid = new DataGridView
            {
                Location = new Point(20, 290),
                Size = new Size(500, 300),
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            DataGridStyler.ApplyModernStyle(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryID", HeaderText = "ID", Width = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryName", HeaderText = "Category Name", Width = 200 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Description", Width = 200 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(headerPanel);
            Controls.Add(inputCard);
            Controls.Add(statsPanel);
            Controls.Add(_grid);
        }

        private void CategoryUserControl_Load(object sender, EventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                List<Category> categories = _categoryService.GetAll(_txtSearch.Text);
                _grid.DataSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load categories: {ex.Message}", "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (!(_grid.CurrentRow?.DataBoundItem is Category category))
            {
                return;
            }

            _txtName.Text = category.CategoryName;
            _txtDescription.Text = category.Description ?? "";
        }

        private void ClearInputs()
        {
            _txtName.Clear();
            _txtDescription.Clear();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_txtName.Text))
                {
                    MessageBox.Show("Category name is required.", "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Category category = new Category
                {
                    CategoryName = _txtName.Text,
                    Description = _txtDescription.Text
                };

                _categoryService.Add(category);
                LoadCategories();
                ClearInputs();
                ToastNotification.Show("Category added successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_grid.CurrentRow?.DataBoundItem is Category category) || category.CategoryID <= 0)
                {
                    MessageBox.Show("Select a category first.", "Category", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                category.CategoryName = _txtName.Text;
                category.Description = _txtDescription.Text;

                _categoryService.Update(category);
                LoadCategories();
                ClearInputs();
                ToastNotification.Show("Category updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(_grid.CurrentRow?.DataBoundItem is Category category) || category.CategoryID <= 0)
                {
                    MessageBox.Show("Select a category first.", "Category", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("Delete selected category?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                _categoryService.Delete(category.CategoryID);
                LoadCategories();
                ClearInputs();
                ToastNotification.Show("Category deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}