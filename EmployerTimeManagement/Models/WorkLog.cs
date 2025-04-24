using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployerTimeManagement.Models;

namespace EmployerTimeManagement.Models
{
    public class WorkLog
    {
        [Key]
        public int Id { get; set; }

        // Σχέση με υπάλληλο
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        // Ημερομηνία καταγραφής & ώρες
        public DateTime Date { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }

        // --- Πεδία για ΕΡΓΑΝΗ API ---

        [Required]
        [MaxLength(9)]
        public string f_afm { get; set; }

        [Required]
        public string f_eponymo { get; set; }

        [Required]
        public string f_onoma { get; set; }

        [Required]
        public int f_type { get; set; } // 0=Έναρξη, 1=Λήξη

        [Required]
        public DateTime f_reference_date { get; set; } // Ημερομηνία εργασίας

        [Required]
        public DateTime f_date { get; set; } // Ημερομηνία και ώρα γεγονότος

        public string? f_aitiologia { get; set; } // Αιτιολόγηση αν είναι εκπρόθεσμη (nullable)

        public bool IsSent { get; set; } = false;

        public DateTime? SentAt { get; set; }


    }
}
