using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class SalesService
    {
        private readonly SalesRepository _salesRepository = new SalesRepository();

        public async Task<int> SaveSaleAsync(
            IReadOnlyList<CartItem> cartItems,
            PosTotals totals,
            PaymentMethod paymentMethod,
            decimal paymentAmount,
            SalePaymentStatus salePaymentStatus,
            int? customerId = null)
        {
            if (cartItems == null || cartItems.Count == 0)
            {
                throw new ApplicationException("Cannot checkout with an empty cart.");
            }

            if (totals == null)
            {
                throw new ApplicationException("Sales totals are required.");
            }

            if (totals.GrandTotal <= 0)
            {
                throw new ApplicationException("Grand total must be greater than zero.");
            }

            if (SessionManager.UserID <= 0)
            {
                throw new ApplicationException("Session is not valid. Please login again.");
            }

            if (cartItems.Any(ci => ci == null || ci.ProductID <= 0 || ci.Quantity <= 0 || ci.UnitPrice < 0))
            {
                throw new ApplicationException("Cart items contain invalid data.");
            }

            string paymentMethodDb = MapPaymentMethod(paymentMethod);
            string paymentStatusDb = salePaymentStatus == SalePaymentStatus.Paid ? "Paid" : "Pending";

            if (paymentMethod == PaymentMethod.Cash)
            {
                if (paymentAmount <= 0)
                {
                    throw new ApplicationException("Cash payment amount is required.");
                }

                if (salePaymentStatus == SalePaymentStatus.Paid && paymentAmount < totals.GrandTotal)
                {
                    throw new ApplicationException("Insufficient cash payment. Paid sales require full coverage of Grand Total.");
                }
            }
            else if (paymentMethod == PaymentMethod.Card || paymentMethod == PaymentMethod.MobileBanking)
            {
                if (paymentAmount <= 0)
                {
                    throw new ApplicationException("Card/Mobile payment amount is required.");
                }

                if (salePaymentStatus == SalePaymentStatus.Paid && paymentAmount < totals.GrandTotal)
                {
                    throw new ApplicationException("Paid sales require full coverage of Grand Total.");
                }
            }
            else
            {
                throw new ApplicationException("Invalid payment method selection.");
            }

            Sale sale = new Sale
            {
                CustomerID = customerId,
                EmployeeID = SessionManager.UserID,
                SaleDate = DateTime.Now,
                TotalAmount = totals.CartTotal,
                Discount = totals.DiscountAmount,
                VAT = totals.VatAmount,
                GrandTotal = totals.GrandTotal,
                PaymentMethod = paymentMethodDb,
                PaymentStatus = paymentStatusDb
            };

            List<SalesDetail> details = cartItems.Select(ci => new SalesDetail
            {
                ProductID = ci.ProductID,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                SubTotal = ci.SubTotal
            }).ToList();

            try
            {
                return await _salesRepository.SaveSaleAsync(sale, details);
            }
            catch (ApplicationException)
            {
                // Surface business validation failures directly to the UI.
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("SalesService.SaveSaleAsync", ex);
                throw new ApplicationException("Failed to complete the sale. Please try again.");
            }
        }

        private static string MapPaymentMethod(PaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case PaymentMethod.Cash:
                    return "Cash";
                case PaymentMethod.Card:
                    return "Card";
                case PaymentMethod.MobileBanking:
                    return "Mobile";
                default:
                    return "Cash";
            }
        }

        public async Task<bool> MarkSaleAsPaidAsync(int saleId)
        {
            return await _salesRepository.MarkSaleAsPaidAndDeductStockAsync(saleId);
        }

        public async Task<List<Sale>> GetSalesByPaymentStatusAsync(string paymentStatus)
        {
            return await _salesRepository.GetSalesByPaymentStatusAsync(paymentStatus);
        }
    }
}

