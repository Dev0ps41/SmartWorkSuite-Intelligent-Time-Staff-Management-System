using System;

namespace EmployerTimeManagement.Models
{
    public class E3Entry
    {
        public int Id { get; set; } //  Αυτό είναι το PRIMARY KEY
        public string AFM { get; set; }
        public string AMKA { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public DateTime HireDate { get; set; }
        public string Specialty { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string DOY { get; set; }
        public string ContractType { get; set; }
        public string WeeklyHours { get; set; }
        public string Notes { get; set; }

        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
    }
}
