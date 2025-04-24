using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E7
{
    public partial class E7Control : UserControl
    {
        public E7Control()
        {
            InitializeComponent();
            LoadE7Entries();
        }

        private void LoadE7Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E7Entries
                .OrderByDescending(e => e.Id)
                .ToList();

            e7Grid.ItemsSource = entries;
        }

        private void ClearForm()
        {
            txtAfm.Text = string.Empty;
            txtFullName.Text = string.Empty;
            dpDate.SelectedDate = null;
            txtHours.Text = string.Empty;
            txtReason.Text = string.Empty;
        }

        private void AddE7_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                dpDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtHours.Text) ||
                !double.TryParse(txtHours.Text, out double hours))
            {
                MessageBox.Show("Συμπλήρωσε σωστά όλα τα υποχρεωτικά πεδία (και τις ώρες).", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entry = new E7Entry
            {
                AFM = txtAfm.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Date = dpDate.SelectedDate.Value,
                Hours = hours,
                Reason = txtReason.Text.Trim()
            };

            using var context = new AppDbContext();
            context.E7Entries.Add(entry);
            context.SaveChanges();

            LoadE7Entries();
            ClearForm();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (e7Grid.SelectedItem is not E7Entry selectedEntry)
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
                var entry = context.E7Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E7Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE7Entries();
                }
            }
        }

        private async void SubmitE7_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var toSubmit = context.E7Entries
                .Where(e => !e.IsSent)
                .ToList();

            if (!toSubmit.Any())
            {
                MessageBox.Show("Δεν υπάρχουν μη υποβληθείσες εγγραφές.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE7Async(toSubmit);

                if (success)
                {
                    MessageBox.Show("Η υποβολή στο ΕΡΓΑΝΗ ήταν επιτυχής.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE7Entries();
                }
                else
                {
                    MessageBox.Show("Η υποβολή απέτυχε.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα κατά την υποβολή: " + ex.Message, "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
