using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync(string searchKeyword = "");
        Task<bool> AddAsync(Employee employee);
        Task<bool> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(int employeeId);
        Task<string> GetPasswordHashByIdAsync(int employeeId);
    }
}
