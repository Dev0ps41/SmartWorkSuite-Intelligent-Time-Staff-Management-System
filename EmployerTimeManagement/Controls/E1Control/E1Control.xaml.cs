using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E1Control
{
    public partial class E1Control : UserControl
    {
        public E1Control()
        {
            InitializeComponent();
            LoadE1Entries();
        }

        private void LoadE1Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E1Entries.OrderByDescending(e => e.Id).ToList();
            e1Grid.ItemsSource = entries;
        }

        private void ClearForm()
        {
            txtAfm.Text = string.Empty;
            txtLastName.Text = string.Empty;
            dpHireDate.SelectedDate = null;
            txtSpecialty.Text = string.Empty;
            txtWorkHours.Text = string.Empty;
        }

        private void AddE1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                dpHireDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtSpecialty.Text) ||
                string.IsNullOrWhiteSpace(txtWorkHours.Text))
            {
                MessageBox.Show("Συμπλήρωσε όλα τα πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtWorkHours.Text, out var hours))
            {
                MessageBox.Show("Οι ώρες πρέπει να είναι αριθμός.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entry = new E1Entry
            {
                AFM = txtAfm.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                HireDate = dpHireDate.SelectedDate.Value,
                Specialty = txtSpecialty.Text.Trim(),
                WorkHours = hours
            };

            using var context = new AppDbContext();
            context.E1Entries.Add(entry);
            context.SaveChanges();

            LoadE1Entries();
            ClearForm();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (e1Grid.SelectedItem is not E1Entry selectedEntry)
            {
                MessageBox.Show("Επίλεξε μια εγγραφή για διαγραφή.", "Προειδοποίηση", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Θέλεις σίγουρα να διαγράψεις την επιλεγμένη εγγραφή;",
                                         "Επιβεβαίωση",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var context = new AppDbContext();
                var entry = context.E1Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E1Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE1Entries();
                }
            }
        }

        private async void SubmitE1_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTableType.SelectedItem is not ComboBoxItem selectedItem)
            {
                MessageBox.Show("Παρακαλώ επέλεξε τύπο πίνακα.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tableType = selectedItem.Content.ToString();

            using var context = new AppDbContext();
            var entriesToSend = context.E1Entries.Where(e => !e.IsSent).ToList();

            if (!entriesToSend.Any())
            {
                MessageBox.Show("Δεν υπάρχουν μη υποβληθείσες εγγραφές.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE1Async(entriesToSend, tableType);

                if (success)
                {
                    MessageBox.Show("Ο πίνακας προσωπικού υποβλήθηκε επιτυχώς!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE1Entries();
                }
                else
                {
                    MessageBox.Show("Η υποβολή απέτυχε. Ελέγξτε τα δεδομένα.", "Αποτυχία", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα κατά την υποβολή: " + ex.Message, "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
