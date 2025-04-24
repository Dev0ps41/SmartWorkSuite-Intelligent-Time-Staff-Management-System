using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Controls.E9
{
    public partial class E9Control : UserControl
    {
        public E9Control()
        {
            InitializeComponent();
            LoadE9Entries();
        }

        private void LoadE9Entries()
        {
            using var context = new AppDbContext();
            var entries = context.E9Entries
                .OrderByDescending(e => e.Id)
                .ToList();

            e9Grid.ItemsSource = entries;
        }

        private void ClearForm()
        {
            txtAfm.Text = string.Empty;
            txtFullName.Text = string.Empty;
            dpDate.SelectedDate = null;
            txtProject.Text = string.Empty;
        }

        private void AddE9_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAfm.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                dpDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtProject.Text))
            {
                MessageBox.Show("Συμπλήρωσε όλα τα πεδία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entry = new E9Entry
            {
                AFM = txtAfm.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Date = dpDate.SelectedDate.Value,
                Project = txtProject.Text.Trim()
            };

            using var context = new AppDbContext();
            context.E9Entries.Add(entry);
            context.SaveChanges();

            LoadE9Entries();
            ClearForm();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (e9Grid.SelectedItem is not E9Entry selectedEntry)
            {
                MessageBox.Show("Επίλεξε μία εργολαβία για διαγραφή.", "Προειδοποίηση", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Θέλεις σίγουρα να διαγράψεις την επιλεγμένη εγγραφή;",
                                         "Επιβεβαίωση",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var context = new AppDbContext();
                var entry = context.E9Entries.FirstOrDefault(e => e.Id == selectedEntry.Id);

                if (entry != null)
                {
                    context.E9Entries.Remove(entry);
                    context.SaveChanges();
                    LoadE9Entries();
                }
            }
        }

        private async void SubmitE9_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var toSubmit = context.E9Entries
                .Where(e => !e.IsSent)
                .ToList();

            if (!toSubmit.Any())
            {
                MessageBox.Show("Δεν υπάρχουν εγγραφές προς υποβολή.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = await ErganiApiService.SubmitE9Async(toSubmit);

                if (success)
                {
                    MessageBox.Show("Η υποβολή ήταν επιτυχής.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadE9Entries();
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
