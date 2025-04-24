using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using System;
using System.Windows;
using System.Windows.Controls;


namespace EmployerTimeManagement.Controls.E3Control
{
    public partial class E3Control : UserControl
    {
        public E3Control()
        {
            InitializeComponent();
            LoadE3History();
        }

        private void LoadE3History()
        {
            using var context = new AppDbContext();
            var history = context.E3Entries.OrderByDescending(e => e.HireDate).ToList();
            e3HistoryGrid.ItemsSource = history;
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtAFM.Text) || txtAFM.Text.Length != 9)
            {
                MessageBox.Show("Το ΑΦΜ πρέπει να έχει 9 ψηφία.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAMKA.Text))
            {
                MessageBox.Show("Το πεδίο ΑΜΚΑ είναι υποχρεωτικό.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Το όνομα και το επώνυμο είναι υποχρεωτικά.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!dpHireDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Επιλέξτε ημερομηνία πρόσληψης.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbContractType.SelectedItem is null)
            {
                MessageBox.Show("Επιλέξτε τύπο σύμβασης.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWeeklyHours.Text) || !decimal.TryParse(txtWeeklyHours.Text, out _))
            {
                MessageBox.Show("Συμπληρώστε έγκυρες ώρες ανά εβδομάδα (π.χ. 40).", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }


        private async void SubmitE3_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
                return;

            var entry = new E3Entry
            {
                AFM = txtAFM.Text.Trim(),
                AMKA = txtAMKA.Text.Trim(),
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                FatherName = txtFatherName.Text.Trim(),
                HireDate = dpHireDate.SelectedDate.Value,
                Specialty = txtSpecialty.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                City = txtCity.Text.Trim(),
                PostalCode = txtPostalCode.Text.Trim(),
                DOY = txtDOY.Text.Trim(),
                ContractType = ((ComboBoxItem)cmbContractType.SelectedItem)?.Content.ToString(),
                WeeklyHours = txtWeeklyHours.Text.Trim(),
                Notes = txtNotes.Text.Trim()
            };

            bool success = await ErganiApiService.SubmitE3Async(entry);

            if (success)
                MessageBox.Show("Η αναγγελία πρόσληψης υποβλήθηκε επιτυχώς!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Η υποβολή απέτυχε. Δοκιμάστε ξανά.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}

