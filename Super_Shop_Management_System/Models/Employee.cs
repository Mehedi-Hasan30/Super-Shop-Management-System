using System;

namespace Super_Shop_Management_System.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public decimal Salary { get; set; }
        public string Shift { get; set; }
        public DateTime JoinDate { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public decimal LatestSalaryPaid { get; set; }
    }
}
