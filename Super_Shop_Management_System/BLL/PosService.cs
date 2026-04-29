using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class PosService
    {
        private readonly PosRepository _posRepository = new PosRepository();
        private readonly List<CartItem> _cartItems = new List<CartItem>();

        public Task<List<Product>> GetProductsAsync(string searchKeyword = "")
        {
            return _posRepository.GetProductsAsync(searchKeyword);
        }

        public Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            return _posRepository.GetProductByBarcodeAsync(barcode);
        }

        public IReadOnlyList<CartItem> GetCartItems()
        {
            return _cartItems.OrderBy(x => x.ProductName).ToList();
        }

        public void AddToCart(Product product, int quantity)
        {
            if (product == null)
            {
                throw new ApplicationException("Select a product.");
            }

            if (quantity <= 0)
            {
                throw new ApplicationException("Quantity must be greater than zero.");
            }

            CartItem existing = _cartItems.FirstOrDefault(x => x.ProductID == product.ProductID);
            if (existing != null)
            {
                existing.Quantity += quantity;
                return;
            }

            _cartItems.Add(new CartItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Quantity = quantity,
                UnitPrice = product.SellingPrice
            });
        }

        public void RemoveFromCart(int productId)
        {
            CartItem existing = _cartItems.FirstOrDefault(x => x.ProductID == productId);
            if (existing != null)
            {
                _cartItems.Remove(existing);
            }
        }

        public void ClearCart()
        {
            _cartItems.Clear();
        }

        public PosTotals CalculateTotals(decimal discountAmount, decimal vatPercentage)
        {
            decimal cartTotal = _cartItems.Sum(x => x.SubTotal);
            decimal normalizedDiscount = discountAmount < 0 ? 0 : discountAmount;
            if (normalizedDiscount > cartTotal)
            {
                normalizedDiscount = cartTotal;
            }

            decimal normalizedVat = vatPercentage < 0 ? 0 : vatPercentage;
            decimal taxableAmount = cartTotal - normalizedDiscount;
            decimal vatAmount = taxableAmount * normalizedVat / 100m;
            decimal grandTotal = taxableAmount + vatAmount;

            return new PosTotals
            {
                CartTotal = cartTotal,
                DiscountAmount = normalizedDiscount,
                VatPercentage = normalizedVat,
                VatAmount = vatAmount,
                GrandTotal = grandTotal
            };
        }
    }
}
