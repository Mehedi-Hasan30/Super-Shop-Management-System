using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL.Interfaces
{
    public interface ISupplierRepository
    {
        Task<List<Supplier>> GetAllAsync(string searchKeyword = "");
        Task<bool> AddAsync(Supplier supplier);
        Task<bool> UpdateAsync(Supplier supplier);
        Task<bool> DeleteAsync(int supplierId);
    }
}
