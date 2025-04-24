using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E10
{
    public partial class E10Control : UserControl
    {
        public E10Control()
        {
            InitializeComponent();
            LoadE10Entries();
        }

        private void LoadE10Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E10Entries
                .OrderByDescending(e => e.Id)
                .ToList();

            e10Grid.ItemsSource = entries;
        }

        private void ClearForm()
        {
            txtAfm.Text = string.Empty;
            txtFullName.Text = string.Empty;
            dpChangeDate.SelectedDate = null;
            txtPreviousData.Text = string.Empty;
            txtNewData.Text = string.Empty;
        }

        private void AddE10_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                dpChangeDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtNewData.Text))
            {
                MessageBox.Show("Συμπλήρωσε τουλάχιστον ΑΦΜ, Ονοματεπώνυμο, Ημερομηνία και Νέα Στοιχεία.",
                                "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entry = new E10Entry
            {
                AFM = txtAfm.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                ChangeDate = dpChangeDate.SelectedDate.Value,
                PreviousData = txtPreviousData.Text.Trim(),
                NewData = txtNewData.Text.Trim()
            };

            using var context = new AppDbContext();
            context.E10Entries.Add(entry);
            context.SaveChanges();

            LoadE10Entries();
            ClearForm();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (e10Grid.SelectedItem is not E10Entry selectedEntry)
            {
                MessageBox.Show("Επίλεξε μία εγγραφή για διαγραφή.", "Προειδοποίηση", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Θέλεις σίγουρα να διαγράψεις την επιλεγμένη εγγραφή;",
                                         "Επιβεβαίωση",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var context = new AppDbContext();
                var entry = context.E10Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E10Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE10Entries();
                }
            }
        }

        private async void SubmitE10_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var toSubmit = context.E10Entries
                .Where(e => !e.IsSent)
                .ToList();

            if (!toSubmit.Any())
            {
                MessageBox.Show("Δεν υπάρχουν εγγραφές προς υποβολή.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE10Async(toSubmit);

                if (success)
                {
                    MessageBox.Show("Η υποβολή στο ΕΡΓΑΝΗ ήταν επιτυχής.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE10Entries();
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
