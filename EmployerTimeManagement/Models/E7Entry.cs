using System;
using System.ComponentModel.DataAnnotations;

namespace EmployerTimeManagement.Models
{
    public class E7Entry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(9)]
        public string AFM { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double Hours { get; set; }

        public string Reason { get; set; }

        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
    }
}
