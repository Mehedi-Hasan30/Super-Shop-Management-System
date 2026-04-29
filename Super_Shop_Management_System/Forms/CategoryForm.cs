using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Super_Shop_Management_System.BLL;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.Forms
{
    public class CategoryForm : Form
    {
        private readonly CategoryService _categoryService = new CategoryService();

        private DataGridView _grid;
        private TextBox _txtName;
        private TextBox _txtDescription;
        private TextBox _txtSearch;

        private int _selectedCategoryId;

        public CategoryForm()
        {
            InitializeComponent();
            Load += (_, __) => LoadCategories();
        }

        private void InitializeComponent()
        {
            Text = "Category Management";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 600);

            Label lblName = new Label { Text = "Category Name", Location = new Point(20, 20), AutoSize = true };
            _txtName = new TextBox { Location = new Point(20, 40), Width = 250 };

            Label lblDescription = new Label { Text = "Description", Location = new Point(290, 20), AutoSize = true };
            _txtDescription = new TextBox { Location = new Point(290, 40), Width = 300 };

            Button btnAdd = new Button { Text = "Add", Location = new Point(610, 38), Width = 70 };
            btnAdd.Click += BtnAdd_Click;

            Button btnUpdate = new Button { Text = "Update", Location = new Point(690, 38), Width = 70 };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new Button { Text = "Delete", Location = new Point(770, 38), Width = 70 };
            btnDelete.Click += BtnDelete_Click;

            Label lblSearch = new Label { Text = "Search", Location = new Point(20, 85), AutoSize = true };
            _txtSearch = new TextBox { Location = new Point(20, 105), Width = 300 };
            _txtSearch.TextChanged += (_, __) => LoadCategories(_txtSearch.Text);

            _grid = new DataGridView
            {
                Location = new Point(20, 145),
                Width = 820,
                Height = 390,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryID", HeaderText = "ID", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryName", HeaderText = "Category Name", Width = 200 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Description", Width = 360 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreatedDate", HeaderText = "Created Date", Width = 170 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(lblName);
            Controls.Add(_txtName);
            Controls.Add(lblDescription);
            Controls.Add(_txtDescription);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(lblSearch);
            Controls.Add(_txtSearch);
            Controls.Add(_grid);
        }

        private void LoadCategories(string search = "")
        {
            try
            {
                List<Category> categories = _categoryService.GetAll(search);
                _grid.DataSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load categories: {ex.Message}", "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Category category = new Category
                {
                    CategoryName = _txtName.Text,
                    Description = _txtDescription.Text
                };

                bool success = _categoryService.Add(category);
                if (success)
                {
                    LoadCategories(_txtSearch.Text);
                    ClearInputs();
                }
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
                Category category = new Category
                {
                    CategoryID = _selectedCategoryId,
                    CategoryName = _txtName.Text,
                    Description = _txtDescription.Text
                };

                bool success = _categoryService.Update(category);
                if (success)
                {
                    LoadCategories(_txtSearch.Text);
                    ClearInputs();
                }
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
                if (_selectedCategoryId <= 0)
                {
                    MessageBox.Show("Select a category first.", "Category", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult result = MessageBox.Show("Delete selected category?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                bool success = _categoryService.Delete(_selectedCategoryId);
                if (success)
                {
                    LoadCategories(_txtSearch.Text);
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is Category category)
            {
                _selectedCategoryId = category.CategoryID;
                _txtName.Text = category.CategoryName;
                _txtDescription.Text = category.Description;
            }
        }

        private void ClearInputs()
        {
            _selectedCategoryId = 0;
            _txtName.Clear();
            _txtDescription.Clear();
        }
    }
}
