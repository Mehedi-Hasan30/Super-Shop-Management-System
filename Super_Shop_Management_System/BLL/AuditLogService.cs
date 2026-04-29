using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Super_Shop_Management_System.DAL;
using Super_Shop_Management_System.Helpers;
using Super_Shop_Management_System.Models;

namespace Super_Shop_Management_System.BLL
{
    public class AuditLogService
    {
        private readonly AuditLogRepository _repository = new AuditLogRepository();

        public async Task LogAsync(string actionType, string tableName, string recordId, string userName, string description)
        {
            try
            {
                AuditLog log = Build(actionType, tableName, recordId, userName, description);
                await _repository.AddAsync(log);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("AuditLogService.LogAsync", ex);
            }
        }

        public void Log(string actionType, string tableName, string recordId, string userName, string description)
        {
            try
            {
                AuditLog log = Build(actionType, tableName, recordId, userName, description);
                _repository.Add(log);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log("AuditLogService.Log", ex);
            }
        }

        public Task<List<AuditLog>> GetRecentAsync(int top = 200)
        {
            return _repository.GetRecentAsync(top);
        }

        public Task<List<AuditLog>> GetFilteredAsync(string actionType, string tableName, string userName, DateTime? fromDate, DateTime? toDate, int top = 500)
        {
            return _repository.GetFilteredAsync(actionType, tableName, userName, fromDate, toDate, top);
        }

        public Task<List<string>> GetDistinctTableNamesAsync()
        {
            return _repository.GetDistinctTableNamesAsync();
        }

        public Task<List<string>> GetDistinctUsersAsync()
        {
            return _repository.GetDistinctUsersAsync();
        }

        private static AuditLog Build(string actionType, string tableName, string recordId, string userName, string description)
        {
            return new AuditLog
            {
                ActionType = actionType,
                TableName = tableName,
                RecordID = recordId,
                UserName = string.IsNullOrWhiteSpace(userName) ? "System" : userName,
                Timestamp = DateTime.Now,
                Description = description
            };
        }
    }
}
