using System;
using System.ComponentModel.DataAnnotations;

namespace EmployerTimeManagement.Models
{
    public class E10Entry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(9)]
        public string AFM { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime ChangeDate { get; set; }

        public string PreviousData { get; set; }

        public string NewData { get; set; }

        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
