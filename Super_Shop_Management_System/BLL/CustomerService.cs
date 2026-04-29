using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.DAL.Interfaces;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class CustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly AuditLogService _auditLogService = new AuditLogService();

        public CustomerService() : this(new CustomerRepository())
        {
        }

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Customer>> GetAllAsync(string searchKeyword = "") => _repository.GetAllAsync(searchKeyword);

        public async Task<bool> AddAsync(Customer customer)
        {
            Validate(customer);
            customer.CreatedDate = ValidationHelper.Now();
            bool success = await _repository.AddAsync(customer);
            if (success)
            {
                await _auditLogService.LogAsync("INSERT", "Customers", customer.Phone, SessionManager.Username, "Customer added.");
            }
            return success;
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            Validate(customer);
            if (customer.CustomerID <= 0)
            {
                throw new ApplicationException("Invalid customer selected.");
            }

            bool success = await _repository.UpdateAsync(customer);
            if (success)
            {
                await _auditLogService.LogAsync("UPDATE", "Customers", customer.CustomerID.ToString(), SessionManager.Username, "Customer updated.");
            }
            return success;
        }

        public async Task<bool> DeleteAsync(int customerId)
        {
            if (customerId <= 0)
            {
                throw new ApplicationException("Invalid customer selected.");
            }

            bool success = await _repository.DeleteAsync(customerId);
            if (success)
            {
                await _auditLogService.LogAsync("DELETE", "Customers", customerId.ToString(), SessionManager.Username, "Customer deleted.");
            }
            return success;
        }

        private static void Validate(Customer customer)
        {
            if (customer == null || ValidationHelper.IsNullOrWhiteSpace(customer.FullName) || ValidationHelper.IsNullOrWhiteSpace(customer.Phone))
            {
                throw new ApplicationException("Customer name and phone are required.");
            }
        }
    }
}
