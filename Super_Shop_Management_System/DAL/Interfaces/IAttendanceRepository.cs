using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<List<Attendance>> GetAttendanceAsync(int? employeeId = null, DateTime? monthDate = null);
        Task<bool> SaveAttendanceAsync(Attendance attendance);
        Task<int> GetTodayPresentCountAsync();
    }
}
