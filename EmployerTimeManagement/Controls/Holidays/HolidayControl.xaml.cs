using EmployerTimeManagement.Models;
using EmployerTimeManagement.Ergani;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EmployerTimeManagement.Data;

namespace EmployerTimeManagement.Controls.Holidays
{
    public partial class HolidayControl : UserControl
    {
        public ObservableCollection<HolidayEntry> Entries { get; set; } = new();

        public HolidayControl()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadEmployees();
            holidayGrid.ItemsSource = Entries;
        }

        private void LoadEmployees()
        {
            using var context = new AppDbContext();
            var employees = context.Employees.OrderBy(e => e.LastName).ToList();
            cmbEmployee.ItemsSource = employees;
        }

        private void AddHoliday_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmployee.SelectedItem is not Employee selectedEmployee ||
                dpFrom.SelectedDate == null ||
                dpTo.SelectedDate == null ||
                cmbLeaveType.SelectedItem is not ComboBoxItem selectedLeave)
            {
                MessageBox.Show("Συμπληρώστε όλα τα απαιτούμενα πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Entries.Add(new HolidayEntry
            {
                EmployeeAFM = selectedEmployee.AFM,
                EmployeeName = selectedEmployee.FullName,
                FromDate = dpFrom.SelectedDate.Value,
                ToDate = dpTo.SelectedDate.Value,
                LeaveType = selectedLeave.Content.ToString(),
                RelProtocol = txtRelProtocol.Text.Trim(),
                IsSent = false
            });

            // Καθαρισμός πεδίων
            cmbEmployee.SelectedIndex = -1;
            dpFrom.SelectedDate = null;
            dpTo.SelectedDate = null;
            cmbLeaveType.SelectedIndex = -1;
            txtRelProtocol.Text = "";
        }

        private async void SubmitHolidays_Click(object sender, RoutedEventArgs e)
        {
            var toSubmit = Entries.Where(x => !x.IsSent).ToList();

            if (!toSubmit.Any())
            {
                MessageBox.Show("Δεν υπάρχουν εγγραφές προς αποστολή.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isCorrection = toSubmit.Any(x => !string.IsNullOrWhiteSpace(x.RelProtocol));
            bool success = isCorrection
                ? await ErganiApiService.SubmitHolidayCorrectionAsync(toSubmit)
                : await ErganiApiService.SubmitHolidaysAsync(toSubmit);

            if (success)
            {
                foreach (var entry in toSubmit)
                {
                    entry.IsSent = true;
                    entry.SentAt = DateTime.Now;
                }

                MessageBox.Show("Η αποστολή ολοκληρώθηκε!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                holidayGrid.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Η αποστολή απέτυχε.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
