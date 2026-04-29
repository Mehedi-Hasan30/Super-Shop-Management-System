using System;
using System.Collections.Generic;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository = new ProductRepository();
        private readonly AuditLogService _auditLogService = new AuditLogService();

        public List<Product> GetAll(string searchKeyword = "")
        {
            return _productRepository.GetAll(searchKeyword);
        }

        public bool Add(Product product)
        {
            Validate(product);
            product.CreatedDate = ValidationHelper.Now();
            bool success = _productRepository.Add(product);
            if (success)
            {
                _auditLogService.Log("INSERT", "Products", product.Barcode, SessionManager.Username, "Product added.");
            }
            return success;
        }

        public bool Update(Product product)
        {
            Validate(product);
            bool success = _productRepository.Update(product);
            if (success)
            {
                _auditLogService.Log("UPDATE", "Products", product.ProductID.ToString(), SessionManager.Username, "Product updated.");
            }
            return success;
        }

        public bool Delete(int productId)
        {
            if (productId <= 0)
            {
                throw new ApplicationException("Invalid product selected.");
            }

            bool success = _productRepository.Delete(productId);
            if (success)
            {
                _auditLogService.Log("DELETE", "Products", productId.ToString(), SessionManager.Username, "Product deleted.");
            }
            return success;
        }

        private static void Validate(Product product)
        {
            if (product == null)
            {
                throw new ApplicationException("Product data is required.");
            }

            if (ValidationHelper.IsNullOrWhiteSpace(product.ProductName))
            {
                throw new ApplicationException("Product name is required.");
            }

            if (ValidationHelper.IsNullOrWhiteSpace(product.Barcode))
            {
                throw new ApplicationException("Barcode is required.");
            }

            if (product.CategoryID <= 0)
            {
                throw new ApplicationException("Category is required.");
            }

            if (!ValidationHelper.IsPositiveDecimal(product.PurchasePrice) || !ValidationHelper.IsPositiveDecimal(product.SellingPrice))
            {
                throw new ApplicationException("Prices must be greater than zero.");
            }

            if (!ValidationHelper.HasProfitMargin(product.PurchasePrice, product.SellingPrice))
            {
                throw new ApplicationException("Selling price cannot be less than purchase price.");
            }

            if (!ValidationHelper.IsNonNegativeInt(product.StockQuantity) || !ValidationHelper.IsNonNegativeInt(product.ReorderLevel))
            {
                throw new ApplicationException("Stock and reorder levels must be zero or more.");
            }
        }
    }
}
