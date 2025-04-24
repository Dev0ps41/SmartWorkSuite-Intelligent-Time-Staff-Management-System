using System;

namespace EmployerTimeManagement.Models
{
    public class HolidayEntry
    {
        public int Id { get; set; } // Primary Key για EF

        public string EmployeeAFM { get; set; }
        public string EmployeeName { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string LeaveType { get; set; }
        public string RelProtocol { get; set; } // Μόνο για διορθώσεις

        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
    }
}
