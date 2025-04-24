namespace EmployerTimeManagement.Models
{
    public class E2Entry
    {
        public int Id { get; set; }
        public string AFM { get; set; }
        public string FullName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Specialty { get; set; }
        public string School { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
