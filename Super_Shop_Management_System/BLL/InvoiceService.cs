using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class InvoiceService
    {
        private readonly SalesRepository _salesRepository = new SalesRepository();

        public async Task<InvoiceData> GetInvoiceAsync(int saleId)
        {
            try
            {
                InvoiceData invoice = await _salesRepository.GetInvoiceDataAsync(saleId);
                invoice.ShopName = "Super Shop";
                if (string.IsNullOrWhiteSpace(invoice.PaymentMethod))
                {
                    invoice.PaymentMethod = "N/A";
                }
                if (string.IsNullOrWhiteSpace(invoice.PaymentStatus))
                {
                    invoice.PaymentStatus = "N/A";
                }
                return invoice;
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("InvoiceService.GetInvoiceAsync", ex);
                throw new ApplicationException("Unable to load invoice data.");
            }
        }

        public async Task<List<SalesHistoryItem>> GetCustomerSalesHistoryAsync(
            int? customerId,
            DateTime? fromDate,
            DateTime? toDate,
            string paymentStatusFilter)
        {
            // paymentStatusFilter: All | Paid | Pending
            if (string.IsNullOrWhiteSpace(paymentStatusFilter))
            {
                paymentStatusFilter = "All";
            }

            try
            {
                return await _salesRepository.GetCustomerSalesHistoryAsync(customerId, fromDate, toDate, paymentStatusFilter);
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("InvoiceService.GetCustomerSalesHistoryAsync", ex);
                throw new ApplicationException("Unable to load sales history.");
            }
        }
    }
}

