using System;

namespace Super_Shop_Management_System.Models
{
    public class AuditLog
    {
        public int AuditLogID { get; set; }
        public string ActionType { get; set; }
        public string TableName { get; set; }
        public string RecordID { get; set; }
        public string UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }
}
