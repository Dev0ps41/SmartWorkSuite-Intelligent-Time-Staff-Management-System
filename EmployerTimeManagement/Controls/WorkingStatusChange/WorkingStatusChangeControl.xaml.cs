using EmployerTimeManagement.Models;
using EmployerTimeManagement.Ergani;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EmployerTimeManagement.Data;

namespace EmployerTimeManagement.Controls.WorkingStatusChange
{
    public partial class WorkingStatusChangeControl : UserControl
    {
        public ObservableCollection<WorkingStatusChangeEntry> Entries { get; set; } = new();

        public WorkingStatusChangeControl()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadEmployees();
            statusChangeGrid.ItemsSource = Entries;
        }

        private void LoadEmployees()
        {
            using var context = new AppDbContext();
            var employees = context.Employees.OrderBy(e => e.LastName).ToList();
            cmbEmployee.ItemsSource = employees;
        }

        private void AddChange_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmployee.SelectedItem is not Employee selectedEmployee ||
                dpChangeDate.SelectedDate == null ||
                cmbChangeType.SelectedItem is not ComboBoxItem selectedType)
            {
                MessageBox.Show("Συμπληρώστε όλα τα πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Entries.Add(new WorkingStatusChangeEntry
            {
                EmployeeAFM = selectedEmployee.AFM,
                EmployeeName = selectedEmployee.FullName,
                Date = dpChangeDate.SelectedDate.Value,
                ChangeType = selectedType.Content.ToString(),
                Comment = "",  // Μπορείς να προσθέσεις πεδίο σχολίου στο XAML
                IsSent = false
            });

            cmbEmployee.SelectedIndex = -1;
            dpChangeDate.SelectedDate = null;
            cmbChangeType.SelectedIndex = -1;
        }

        private async void SubmitToErgani_Click(object sender, RoutedEventArgs e)
        {
            var unsent = Entries.Where(e => !e.IsSent).ToList();

            if (!unsent.Any())
            {
                MessageBox.Show("Δεν υπάρχουν εγγραφές προς υποβολή.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool success = await ErganiApiService.SubmitWorkingStatusChangesAsync(unsent);

            if (success)
            {
                foreach (var entry in unsent)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                MessageBox.Show("Η αποστολή ολοκληρώθηκε με επιτυχία!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                statusChangeGrid.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Η αποστολή απέτυχε.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
