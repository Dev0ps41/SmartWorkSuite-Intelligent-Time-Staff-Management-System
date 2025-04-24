// Models/E6Entry.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EmployerTimeManagement.Models
{
    public class E6Entry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(9)]
        public string AFM { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string Specialty { get; set; }

        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
    }
}
