using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;
using Super_Shop_Management_System.Helpers;

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
            var title = new Label
            {
                Text = "Category Management",
                Font = ThemeManager.FontH2,
                ForeColor = ThemeManager.Foreground,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var panel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(500, 180),
                BackColor = ThemeManager.PanelBackground,
                Padding = new Padding(12)
            };

            _txtName = new TextBox { Location = new Point(20, 40), Width = 300 };
            _txtDescription = new TextBox { Location = new Point(20, 100), Width = 300 };

            Button btnAdd = new RoundedButton { Text = "Add", Location = new Point(20, 140), Width = 80, CornerRadius = 6 };
            btnAdd.Click += BtnAdd_Click;

            Button btnUpdate = new RoundedButton { Text = "Update", Location = new Point(120, 140), Width = 80, CornerRadius = 6 };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new RoundedButton { Text = "Delete", Location = new Point(220, 140), Width = 80, CornerRadius = 6 };
            btnDelete.Click += BtnDelete_Click;

            _txtSearch = new TextBox { Location = new Point(20, 170), Width = 300 };
            _txtSearch.TextChanged += (_, __) => LoadCategories();

            _grid = new DataGridView
            {
                Location = new Point(20, 220),
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

            Controls.Add(title);
            Controls.Add(panel);
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