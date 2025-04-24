using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E2Control
{
    public partial class E2Control : UserControl
    {
        public E2Control()
        {
            InitializeComponent();
            LoadE2Entries();
        }

        private void LoadE2Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E2Entries.OrderByDescending(e => e.Id).ToList();
            e2Grid.ItemsSource = entries;
        }

        private void AddContract_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Συμπλήρωσε τουλάχιστον ΑΦΜ και Ονοματεπώνυμο.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newEntry = new E2Entry
            {
                AFM = txtAfm.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                StartDate = dpStartDate.SelectedDate ?? DateTime.Today,
                EndDate = dpEndDate.SelectedDate ?? DateTime.Today.AddMonths(6),
                Specialty = txtSpecialty.Text.Trim(),
                School = txtSchool.Text.Trim(),
                IsSent = false,
                SentAt = null
            };

            using var context = new AppDbContext();
            context.E2Entries.Add(newEntry);
            context.SaveChanges();

            LoadE2Entries();

            txtAfm.Clear();
            txtFullName.Clear();
            txtSpecialty.Clear();
            txtSchool.Clear();
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;

            MessageBox.Show("Η σύμβαση προστέθηκε.", "Ενημέρωση", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteSelectedEntry_Click(object sender, RoutedEventArgs e)
        {
            if (e2Grid.SelectedItem is not E2Entry selectedEntry)
            {
                MessageBox.Show("Επέλεξε πρώτα μια σύμβαση προς διαγραφή.", "Προειδοποίηση", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Είσαι σίγουρος ότι θέλεις να διαγράψεις αυτή τη σύμβαση;",
                                         "Επιβεβαίωση",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var context = new AppDbContext();
                var entry = context.E2Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E2Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE2Entries();
                    MessageBox.Show("Η σύμβαση διαγράφηκε.", "Ενημέρωση", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        private async void SubmitE2_Click(object sender, RoutedEventArgs e)
        {
            var entriesToSend = e2Grid.ItemsSource as List<E2Entry>;

            if (entriesToSend == null || !entriesToSend.Any())
            {
                MessageBox.Show("Δεν υπάρχουν εγγραφές προς υποβολή.", "Προειδοποίηση", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE2Async(entriesToSend);

                if (success)
                {
                    MessageBox.Show("Οι συμβάσεις μαθητείας υποβλήθηκαν επιτυχώς!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE2Entries();
                }
                else
                {
                    MessageBox.Show("Η υποβολή απέτυχε. Ελέγξτε τα δεδομένα.", "Αποτυχία", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα: " + ex.Message, "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
