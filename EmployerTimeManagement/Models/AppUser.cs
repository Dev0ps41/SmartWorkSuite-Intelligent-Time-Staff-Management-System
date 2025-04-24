namespace EmployerTimeManagement.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;
    }
}
