using System;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.WTO

{
    public partial class WeeklyScheduleWindow : Window
    {
        public List<WTOEntry> GeneratedEntries { get; private set; } = new();

        public WeeklyScheduleWindow()
        {
            InitializeComponent();

        }

        public void PreFillFromEmployee(Employee emp)
        {
            txtAFM.Text = emp.AFM;
            txtFirstName.Text = emp.FirstName;
            txtLastName.Text = emp.LastName;
        }


        





        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Βασικός έλεγχος
            if (string.IsNullOrWhiteSpace(txtAFM.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtFromTime.Text) ||
                string.IsNullOrWhiteSpace(txtToTime.Text) ||
                cmbWorkType.SelectedItem == null)
            {
                MessageBox.Show("Παρακαλώ συμπληρώστε όλα τα πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedDays = FindVisualChildren<CheckBox>(this)
                .Where(cb => cb.Tag != null && cb.IsChecked == true)
                .Select(cb => int.Parse(cb.Tag.ToString()))
                .ToList();

            if (selectedDays.Count == 0)
            {
                MessageBox.Show("Επιλέξτε τουλάχιστον μία ημέρα.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Δημιουργία εγγραφών
            foreach (int day in selectedDays)
            {
                GeneratedEntries.Add(new WTOEntry
                {
                    EmployeeAFM = txtAFM.Text.Trim(),
                    EmployeeLastName = txtLastName.Text.Trim(),
                    EmployeeFirstName = txtFirstName.Text.Trim(),
                    FromTime = txtFromTime.Text.Trim(),
                    ToTime = txtToTime.Text.Trim(),
                    WorkType = ((ComboBoxItem)cmbWorkType.SelectedItem).Content.ToString(),
                    DayOfWeek = day
                });
            }

            this.DialogResult = true;
            this.Close();
        }

        // Utility για εύρεση όλων των CheckBox στο Window
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                        yield return t;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }
}
