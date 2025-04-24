using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement
{
    public partial class AddEmployeeControl : UserControl
    {
        private readonly AppDbContext _context;

        public AddEmployeeControl()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees
                .OrderByDescending(e => e.CreatedAt)
                .ToList();

            employeeGrid.ItemsSource = employees;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAFM.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Όλα τα πεδία είναι υποχρεωτικά.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Έλεγχος αν υπάρχει ήδη υπάλληλος με ίδιο ΑΦΜ
            if (_context.Employees.Any(e => e.AFM == txtAFM.Text.Trim()))
            {
                MessageBox.Show("Υπάρχει ήδη υπάλληλος με αυτό το ΑΦΜ.", "Προειδοποίηση", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var employee = new Employee
            {
                AFM = txtAFM.Text.Trim(),
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();

            MessageBox.Show("✅ Ο υπάλληλος προστέθηκε επιτυχώς!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);

            txtAFM.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            LoadEmployees();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Employee selectedEmployee)
            {
                if (MessageBox.Show($"Θες σίγουρα να διαγράψεις τον υπάλληλο '{selectedEmployee.FirstName} {selectedEmployee.LastName}';",
                    "Επιβεβαίωση", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _context.Employees.Remove(selectedEmployee);
                    _context.SaveChanges();
                    LoadEmployees();
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Employee selectedEmployee)
            {
                string newFirstName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Εισάγετε νέο όνομα:", "Επεξεργασία Ονόματος", selectedEmployee.FirstName);
                string newLastName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Εισάγετε νέο επώνυμο:", "Επεξεργασία Επώνυμου", selectedEmployee.LastName);

                if (!string.IsNullOrWhiteSpace(newFirstName) && !string.IsNullOrWhiteSpace(newLastName))
                {
                    selectedEmployee.FirstName = newFirstName;
                    selectedEmployee.LastName = newLastName;
                    _context.SaveChanges();
                    LoadEmployees();
                }
            }
        }
    }
}
