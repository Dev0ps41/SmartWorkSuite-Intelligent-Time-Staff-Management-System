using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E4
{
    public partial class E4Control : UserControl

    {
        public E4Control()
        {
            InitializeComponent();
            LoadE4Entries();
        }

        private void LoadE4Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E4Entries
                .OrderByDescending(e => e.Id)
                .ToList();

            e4Grid.ItemsSource = entries;
        }

        private void AddE4_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                dpLeaveDate.SelectedDate == null)
            {
                MessageBox.Show("Συμπλήρωσε όλα τα υποχρεωτικά πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entry = new E4Entry
            {
                AFM = txtAfm.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                LeaveDate = dpLeaveDate.SelectedDate.Value,
                Reason = txtReason.Text.Trim()
            };

            using var context = new AppDbContext();
            context.E4Entries.Add(entry);
            context.SaveChanges();

            LoadE4Entries();
            ClearForm();
        }

        private void ClearForm()
        {
            txtAfm.Text = string.Empty;
            txtFullName.Text = string.Empty;
            dpLeaveDate.SelectedDate = null;
            txtReason.Text = string.Empty;
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (e4Grid.SelectedItem is not E4Entry selectedEntry)
            {
                MessageBox.Show("Επίλεξε μια αποχώρηση για διαγραφή.", "Προειδοποίηση", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Θέλεις σίγουρα να διαγράψεις την επιλεγμένη εγγραφή;",
                                         "Επιβεβαίωση",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var context = new AppDbContext();
                var entry = context.E4Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E4Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE4Entries();
                }
            }
        }

        private async void SubmitE4_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var toSubmit = context.E4Entries
                .Where(e => !e.IsSent)
                .ToList();

            if (!toSubmit.Any())
            {
                MessageBox.Show("Δεν υπάρχουν μη υποβληθείσες εγγραφές.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE4Async(toSubmit);

                if (success)
                {
                    MessageBox.Show("Η υποβολή στο ΕΡΓΑΝΗ ήταν επιτυχής.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE4Entries();
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
