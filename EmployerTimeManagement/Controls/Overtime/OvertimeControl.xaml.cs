using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.Overtime
{
    public partial class OvertimeControl : UserControl
    {
        public ObservableCollection<OvertimeEntry> OvertimeEntries { get; set; } = new();
        public ObservableCollection<Employee> Employees { get; set; } = new();

        public OvertimeControl()
        {
            InitializeComponent();
            this.DataContext = this;

            LoadEmployees();
            overtimeGrid.ItemsSource = OvertimeEntries;
        }

        private void LoadEmployees()
        {
            using var context = new AppDbContext();
            var all = context.Employees.ToList();
            Employees = new ObservableCollection<Employee>(all);
            cmbEmployee.ItemsSource = Employees;
        }

        private async void SubmitToErgani_Click(object sender, RoutedEventArgs e)
        {
            var toSubmit = OvertimeEntries.Where(o => !o.IsSent).ToList();

            if (toSubmit.Count == 0)
            {
                MessageBox.Show("Δεν υπάρχουν εγγραφές προς υποβολή.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmed = MessageBox.Show($"Θα υποβληθούν {toSubmit.Count} εγγραφές στο ΕΡΓΑΝΗ.\nΣυνέχεια;",
                                            "Επιβεβαίωση", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmed != MessageBoxResult.Yes)
                return;

            bool success = await ErganiApiService.SubmitOvertimeAsync(toSubmit);

            if (success)
            {
                MessageBox.Show("Οι εγγραφές υποβλήθηκαν επιτυχώς!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                overtimeGrid.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Η υποβολή απέτυχε. Δείτε το αρχείο καταγραφής.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void AddOvertime_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmployee.SelectedItem is not Employee employee)
            {
                MessageBox.Show("Επιλέξτε εργαζόμενο.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDate.SelectedDate is not DateTime date)
            {
                MessageBox.Show("Επιλέξτε ημερομηνία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtHours.Text, out double hours) || hours <= 0)
            {
                MessageBox.Show("Εισάγετε έγκυρο αριθμό ωρών.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reason = txtReason.Text.Trim();

            OvertimeEntries.Add(new OvertimeEntry
            {
                EmployeeAFM = employee.AFM,
                EmployeeName = $"{employee.LastName} {employee.FirstName}",
                Date = date,
                Hours = hours,
                Reason = reason
            });

            // Καθαρισμός πεδίων
            txtHours.Clear();
            txtReason.Clear();
            dpDate.SelectedDate = null;
        }
    }
}
