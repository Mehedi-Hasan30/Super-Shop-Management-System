using System;

namespace Super_Shop_Management_System.Models
{
    public class NotificationItem
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

