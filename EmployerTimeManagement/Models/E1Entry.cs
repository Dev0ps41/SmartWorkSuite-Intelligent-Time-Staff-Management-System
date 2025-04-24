using System;

namespace EmployerTimeManagement.Models
{
    public class E1Entry
    {
        public int Id { get; set; }

        // Στοιχεία εργαζομένου
        public string AFM { get; set; }
        public string AMKA { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string FatherName { get; set; }
        public string Specialty { get; set; }

        // Στοιχεία εργασίας
        public decimal WorkHours { get; set; }
        // π.χ. "09:00 - 17:00"
        public string ContractType { get; set; }
        public string EmploymentType { get; set; }  // π.χ. Μερική / Πλήρης

        // Διεύθυνση κατοικίας
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        public DateTime HireDate { get; set; }

        // Υποβολή
        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }

        public string DOY { get; set; }
        public string WeeklyHours { get; set; }
        public string TableType { get; set; } // Ετήσιος / Τροποποιητικός

    }
}
