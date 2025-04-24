using System;

namespace EmployerTimeManagement.Models
{
    public class WorkingStatusChangeEntry
    {
        public int Id { get; set; }  // ✅ Primary Key για EF Core

        public string EmployeeAFM { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string ChangeType { get; set; }
        public string Comment { get; set; }

        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
    }
}
