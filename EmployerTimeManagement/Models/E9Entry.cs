using System;
using System.ComponentModel.DataAnnotations;

namespace EmployerTimeManagement.Models
{
    public class E9Entry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(9)]
        public string AFM { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime Date { get; set; }  //  Προσθέτουμε αυτό

        public string Project { get; set; }

        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
