using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.DAL.Interfaces;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly AuditLogService _auditLogService = new AuditLogService();

        public EmployeeService() : this(new EmployeeRepository())
        {
        }

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Employee>> GetAllAsync(string searchKeyword = "") => _repository.GetAllAsync(searchKeyword);

        public async Task<bool> AddAsync(Employee employee)
        {
            ValidateBase(employee);
            ValidatePasswordForCreate(employee.Password);
            employee.Password = SecurityHelper.HashValue(employee.Password);
            bool success = await _repository.AddAsync(employee);
            if (success)
            {
                await _auditLogService.LogAsync("INSERT", "Employees", employee.Username, SessionManager.Username, "Employee added.");
            }
            return success;
        }

        public async Task<bool> UpdateAsync(Employee employee, string newPassword)
        {
            ValidateBase(employee);
            if (employee.EmployeeID <= 0)
            {
                throw new ApplicationException("Invalid employee selected.");
            }

            string passwordToSave;
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                passwordToSave = await _repository.GetPasswordHashByIdAsync(employee.EmployeeID);
                if (string.IsNullOrWhiteSpace(passwordToSave))
                {
                    throw new ApplicationException("Existing password not found for selected employee.");
                }
            }
            else
            {
                ValidatePasswordForCreate(newPassword);
                passwordToSave = SecurityHelper.HashValue(newPassword);
            }

            employee.Password = passwordToSave;
            bool success = await _repository.UpdateAsync(employee);
            if (success)
            {
                await _auditLogService.LogAsync("UPDATE", "Employees", employee.EmployeeID.ToString(), SessionManager.Username, "Employee updated.");
            }
            return success;
        }

        public async Task<bool> DeleteAsync(int employeeId)
        {
            if (employeeId <= 0)
            {
                throw new ApplicationException("Invalid employee selected.");
            }

            bool success = await _repository.DeleteAsync(employeeId);
            if (success)
            {
                await _auditLogService.LogAsync("DELETE", "Employees", employeeId.ToString(), SessionManager.Username, "Employee deleted.");
            }
            return success;
        }

        private static void ValidateBase(Employee employee)
        {
            if (employee == null || ValidationHelper.IsNullOrWhiteSpace(employee.FullName) || ValidationHelper.IsNullOrWhiteSpace(employee.Username))
            {
                throw new ApplicationException("Employee name and username are required.");
            }

            if (employee.Salary <= 0)
            {
                throw new ApplicationException("Salary must be greater than zero.");
            }

            if (ValidationHelper.IsNullOrWhiteSpace(employee.Role) || ValidationHelper.IsNullOrWhiteSpace(employee.Shift))
            {
                throw new ApplicationException("Role and shift are required.");
            }
        }

        private static void ValidatePasswordForCreate(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                throw new ApplicationException("Password must be at least 8 characters long.");
            }

            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char character in password)
            {
                if (char.IsUpper(character)) hasUpper = true;
                else if (char.IsLower(character)) hasLower = true;
                else if (char.IsDigit(character)) hasDigit = true;
                else hasSpecial = true;
            }

            if (!(hasUpper && hasLower && hasDigit && hasSpecial))
            {
                throw new ApplicationException("Password must include uppercase, lowercase, number, and special character.");
            }
        }
    }
}
