using System;
using System.Collections.Generic;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository = new CategoryRepository();

        public List<Category> GetAll(string searchKeyword = "")
        {
            return _categoryRepository.GetAll(searchKeyword);
        }

        public bool Add(Category category)
        {
            Validate(category);
            category.CreatedDate = ValidationHelper.Now();
            return _categoryRepository.Add(category);
        }

        public bool Update(Category category)
        {
            Validate(category);
            return _categoryRepository.Update(category);
        }

        public bool Delete(int categoryId)
        {
            if (categoryId <= 0)
            {
                throw new ApplicationException("Invalid category selected.");
            }

            return _categoryRepository.Delete(categoryId);
        }

        private static void Validate(Category category)
        {
            if (category == null)
            {
                throw new ApplicationException("Category data is required.");
            }

            if (ValidationHelper.IsNullOrWhiteSpace(category.CategoryName))
            {
                throw new ApplicationException("Category name is required.");
            }
        }
    }
}
