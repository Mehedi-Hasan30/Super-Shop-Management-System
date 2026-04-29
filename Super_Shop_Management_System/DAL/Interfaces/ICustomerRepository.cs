using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync(string searchKeyword = "");
        Task<bool> AddAsync(Customer customer);
        Task<bool> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(int customerId);
    }
}
