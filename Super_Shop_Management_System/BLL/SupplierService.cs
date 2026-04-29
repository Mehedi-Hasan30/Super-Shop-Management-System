using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.DAL.Interfaces;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class SupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService() : this(new SupplierRepository())
        {
        }

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Supplier>> GetAllAsync(string searchKeyword = "") => _repository.GetAllAsync(searchKeyword);

        public async Task<bool> AddAsync(Supplier supplier)
        {
            Validate(supplier);
            supplier.CreatedDate = ValidationHelper.Now();
            return await _repository.AddAsync(supplier);
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            Validate(supplier);
            if (supplier.SupplierID <= 0)
            {
                throw new ApplicationException("Invalid supplier selected.");
            }

            return await _repository.UpdateAsync(supplier);
        }

        public Task<bool> DeleteAsync(int supplierId)
        {
            if (supplierId <= 0)
            {
                throw new ApplicationException("Invalid supplier selected.");
            }

            return _repository.DeleteAsync(supplierId);
        }

        private static void Validate(Supplier supplier)
        {
            if (supplier == null || ValidationHelper.IsNullOrWhiteSpace(supplier.SupplierName) || ValidationHelper.IsNullOrWhiteSpace(supplier.Phone))
            {
                throw new ApplicationException("Supplier name and phone are required.");
            }
        }
    }
}
