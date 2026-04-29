using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.DAL.Interfaces;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class AttendanceService
    {
        private readonly IAttendanceRepository _repository;

        public AttendanceService() : this(new AttendanceRepository())
        {
        }

        public AttendanceService(IAttendanceRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Attendance>> GetAttendanceAsync(int? employeeId = null, DateTime? monthDate = null)
        {
            return _repository.GetAttendanceAsync(employeeId, monthDate);
        }

        public Task<bool> SaveAttendanceAsync(Attendance attendance)
        {
            if (attendance.EmployeeID <= 0)
            {
                throw new ApplicationException("Select a valid employee.");
            }

            if (string.IsNullOrWhiteSpace(attendance.Status))
            {
                throw new ApplicationException("Attendance status is required.");
            }

            return _repository.SaveAttendanceAsync(attendance);
        }
    }
}
