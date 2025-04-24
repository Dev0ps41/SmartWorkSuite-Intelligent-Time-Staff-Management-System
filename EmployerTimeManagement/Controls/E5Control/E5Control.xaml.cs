using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E5
{
    public partial class E5Control : UserControl
    {
        public E5Control()
        {
            InitializeComponent();
            LoadE5Entries();
        }

        private void LoadE5Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E5Entries
                .OrderByDescending(e => e.Id)
                .ToList();

            e5Grid.ItemsSource = entries;
        }

        private void ClearForm()
        {
            txtAfm.Text = string.Empty;
            txtFullName.Text = string.Empty;
            dpTerminationDate.SelectedDate = null;
            txtReason.Text = string.Empty;
        }

        private void AddE5_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                dpTerminationDate.SelectedDate == null)
            {
                MessageBox.Show("Συμπλήρωσε όλα τα υποχρεωτικά πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entry = new E5Entry
            {
                AFM = txtAfm.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                TerminationDate = dpTerminationDate.SelectedDate.Value,
                Reason = txtReason.Text.Trim()
            };

            using var context = new AppDbContext();
            context.E5Entries.Add(entry);
            context.SaveChanges();

            LoadE5Entries();
            ClearForm();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (e5Grid.SelectedItem is not E5Entry selectedEntry)
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
                var entry = context.E5Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E5Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE5Entries();
                }
            }
        }

        private async void SubmitE5_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var toSubmit = context.E5Entries
                .Where(e => !e.IsSent)
                .ToList();

            if (!toSubmit.Any())
            {
                MessageBox.Show("Δεν υπάρχουν μη υποβληθείσες εγγραφές.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE5Async(toSubmit);

                if (success)
                {
                    MessageBox.Show("Η υποβολή στο ΕΡΓΑΝΗ ήταν επιτυχής.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE5Entries();
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
