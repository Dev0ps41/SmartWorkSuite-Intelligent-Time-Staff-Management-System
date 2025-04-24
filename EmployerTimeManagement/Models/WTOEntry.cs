namespace EmployerTimeManagement.Models
{
    public class WTOEntry
    {
        public string EmployeeAFM { get; set; } = string.Empty;
        public string EmployeeLastName { get; set; } = string.Empty;
        public string EmployeeFirstName { get; set; } = string.Empty;

        // Για WTODaily (π.χ. 2024-04-12)
        public DateTime? Date { get; set; }

        // Για WTOWEEKLY (π.χ. 0 = Κυριακή, 1 = Δευτέρα...)
        public int? DayOfWeek { get; set; }

        public string WorkType { get; set; } = "ΕΡΓ"; // ΕΡΓ, ΤΗΛ, ΑΝ, ΑΔ.ΚΑΝ κλπ
        public string FromTime { get; set; } = "";    // π.χ. 09:00
        public string ToTime { get; set; } = "";
        public bool IsSelected { get; set; } = false;
        public bool HasError { get; set; } = false;

        public string FullName => $"{EmployeeLastName} {EmployeeFirstName}";

        public bool IsWeekly => DayOfWeek.HasValue;
    }
}
