using System.ComponentModel.DataAnnotations;

namespace EmployerTimeManagement.Models
{
    public class CompanyInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployerName { get; set; }

        [Required]
        [MaxLength(9)]
        public string AFM { get; set; }
        public bool IsLiveErganiEnabled { get; set; } = false;

        public string BranchId { get; set; }     // ✅ Νέο: Α/Α Παραρτήματος
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }

        // Νέα πεδία για το ERGANI API
        public string ErganiUsername { get; set; }
        public string ErganiPassword { get; set; }
        public string ErganiAA { get; set; }
        public bool UseProductionApi { get; set; }
        // Νέο πεδίο για API Key
        public string ErganiApiKey { get; set; }
    }
}
