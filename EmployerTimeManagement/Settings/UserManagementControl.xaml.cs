using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement
{
    public partial class UserManagementControl : UserControl
    {
        private readonly AppDbContext _context;

        public UserManagementControl()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadUsers();
        }

        private void LoadUsers()
        {
            usersGrid.ItemsSource = _context.Users.ToList();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUsername.Text) && !string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                var user = new AppUser
                {
                    Username = txtUsername.Text,
                    Password = txtPassword.Password // Ideally hash this!
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                LoadUsers();
                txtUsername.Clear();
                txtPassword.Clear();
            }
            else
            {
                MessageBox.Show("Συμπλήρωσε όλα τα πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AppUser user)
            {
                if (MessageBox.Show($"Να διαγραφεί ο χρήστης {user.Username};", "Επιβεβαίωση", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    LoadUsers();
                }
            }
        }
    }
}
