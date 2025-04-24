namespace EmployerTimeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string AFM { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        // 👉 Προσθήκη υπολογιζόμενης ιδιότητας
        public string FullName => $"{LastName} {FirstName}";

        // 👉 Αυτό εμφανίζεται σε ComboBox / DataGrid όταν δεν έχεις DisplayMemberPath
        public override string ToString()
        {
            return FullName;
        }
    }
}
