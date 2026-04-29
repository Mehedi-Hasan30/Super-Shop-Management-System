using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL.Interfaces;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.DAL
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public async Task<List<Attendance>> GetAttendanceAsync(int? employeeId = null, DateTime? monthDate = null)
        {
            const string query = @"SELECT a.AttendanceID, a.EmployeeID, e.FullName AS EmployeeName, a.Date, a.CheckIn, a.CheckOut, a.Status
                                   FROM Attendance a
                                   INNER JOIN Employees e ON e.EmployeeID = a.EmployeeID
                                   WHERE (@EmployeeID IS NULL OR a.EmployeeID = @EmployeeID)
                                   AND (@MonthDate IS NULL OR (MONTH(a.Date) = MONTH(@MonthDate) AND YEAR(a.Date) = YEAR(@MonthDate)))
                                   ORDER BY a.Date DESC, e.FullName";

            DataTable table = await _dbHelper.ExecuteDataTableAsync(query,
                new SqlParameter("@EmployeeID", (object)employeeId ?? DBNull.Value),
                new SqlParameter("@MonthDate", (object)monthDate ?? DBNull.Value));

            List<Attendance> history = new List<Attendance>();
            foreach (DataRow row in table.Rows)
            {
                history.Add(new Attendance
                {
                    AttendanceID = Convert.ToInt32(row["AttendanceID"]),
                    EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                    EmployeeName = row["EmployeeName"].ToString(),
                    Date = Convert.ToDateTime(row["Date"]),
                    CheckIn = row["CheckIn"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CheckIn"]),
                    CheckOut = row["CheckOut"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CheckOut"]),
                    Status = row["Status"].ToString()
                });
            }

            return history;
        }

        public async Task<bool> SaveAttendanceAsync(Attendance attendance)
        {
            const string query = @"MERGE Attendance AS target
                                   USING (SELECT @EmployeeID AS EmployeeID, @Date AS [Date]) AS source
                                   ON target.EmployeeID = source.EmployeeID AND target.Date = source.Date
                                   WHEN MATCHED THEN
                                       UPDATE SET CheckIn = @CheckIn, CheckOut = @CheckOut, Status = @Status
                                   WHEN NOT MATCHED THEN
                                       INSERT (EmployeeID, Date, CheckIn, CheckOut, Status)
                                       VALUES (@EmployeeID, @Date, @CheckIn, @CheckOut, @Status);";

            int rows = await _dbHelper.ExecuteNonQueryAsync(query,
                new SqlParameter("@EmployeeID", attendance.EmployeeID),
                new SqlParameter("@Date", attendance.Date.Date),
                new SqlParameter("@CheckIn", (object)attendance.CheckIn ?? DBNull.Value),
                new SqlParameter("@CheckOut", (object)attendance.CheckOut ?? DBNull.Value),
                new SqlParameter("@Status", attendance.Status));

            return rows > 0;
        }

        public async Task<int> GetTodayPresentCountAsync()
        {
            const string query = "SELECT COUNT(1) FROM Attendance WHERE Date = CAST(GETDATE() AS DATE) AND Status = 'Present'";
            return Convert.ToInt32(await _dbHelper.ExecuteScalarAsync(query));
        }
    }
}
