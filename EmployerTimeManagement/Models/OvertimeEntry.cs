using System;

namespace EmployerTimeManagement.Models
{
    public class OvertimeEntry
    {
        public string EmployeeAFM { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public string Reason { get; set; }

        // Προαιρετικά για metadata
        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
    }
}
