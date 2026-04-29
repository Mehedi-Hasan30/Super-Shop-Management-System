using System.Collections.Generic;

namespace Super_Shop_Management_System.Models
{
    public class HealthCheckResult
    {
        public bool IsHealthy { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}

