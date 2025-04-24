using System;

namespace EmployerTimeManagement.Models
{
    public class ScheduleAppointment
    {
        public string Subject { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Category { get; set; }         // optional
        public string EmployeeName { get; set; }     // optional for grouping
    }
}
