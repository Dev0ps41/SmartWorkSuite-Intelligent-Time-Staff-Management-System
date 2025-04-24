using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E6
{
    public partial class E6Control : UserControl
    {
        public E6Control()
        {
            InitializeComponent();
            LoadE6Entries();
        }

        private void LoadE6Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E6Entries
                .OrderByDescending(e => e.Id)
                .ToList();

            e6Grid.ItemsSource = entries;
        }

        private void ClearForm()
        {
            txtAfm.Text = string.Empty;
            txtFullName.Text = string.Empty;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            txtSpecialty.Text = string.Empty;
        }

        private void AddE6_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                dpStartDate.SelectedDate == null ||
                dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Συμπλήρωσε όλα τα υποχρεωτικά πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entry = new E6Entry
            {
                AFM = txtAfm.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                StartDate = dpStartDate.SelectedDate.Value,
                EndDate = dpEndDate.SelectedDate.Value,
                Specialty = txtSpecialty.Text.Trim()
            };

            using var context = new AppDbContext();
            context.E6Entries.Add(entry);
            context.SaveChanges();

            LoadE6Entries();
            ClearForm();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (e6Grid.SelectedItem is not E6Entry selectedEntry)
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
                var entry = context.E6Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E6Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE6Entries();
                }
            }
        }

        private async void SubmitE6_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var toSubmit = context.E6Entries
                .Where(e => !e.IsSent)
                .ToList();

            if (!toSubmit.Any())
            {
                MessageBox.Show("Δεν υπάρχουν μη υποβληθείσες εγγραφές.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE6Async(toSubmit);

                if (success)
                {
                    MessageBox.Show("Η υποβολή στο ΕΡΓΑΝΗ ήταν επιτυχής.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE6Entries();
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
