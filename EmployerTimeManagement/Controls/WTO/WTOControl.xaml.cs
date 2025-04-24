using EmployerTimeManagement.Models;
using EmployerTimeManagement.Ergani;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ClosedXML.Excel;
using System.Windows.Media;
using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;



namespace EmployerTimeManagement.Controls.WTO
{
    public partial class WTOControl : UserControl
    {
        public ObservableCollection<WTOEntry> WTOEntries { get; set; } = new();
        public ObservableCollection<Employee> Employees { get; set; } = new();


        public WTOControl()
        {
            InitializeComponent();
            this.DataContext = this;
            WtoDataGrid.ItemsSource = WTOEntries;
            // Φόρτωση υπαλλήλων από βάση
            using var context = new AppDbContext();
            var allEmployees = context.Employees.ToList();

            Employees = new ObservableCollection<Employee>(allEmployees);
            EmployeeComboBox.ItemsSource = Employees;

        }


        private void LoadFromExcel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Επιλέξτε αρχείο Excel WTO"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            try
            {
                var workbook = new XLWorkbook(openFileDialog.FileName);
                var worksheet = workbook.Worksheets.First();

                WTOEntries.Clear();

                // Υποθέτουμε: headers στην 1η σειρά, δεδομένα από 2η
                for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
                {
                    var afm = worksheet.Cell(row, 1).GetString();
                    var lastName = worksheet.Cell(row, 2).GetString();
                    var firstName = worksheet.Cell(row, 3).GetString();
                    var dateOrDay = worksheet.Cell(row, 4).GetString();
                    var workType = worksheet.Cell(row, 5).GetString();
                    var fromTime = worksheet.Cell(row, 6).GetString();
                    var toTime = worksheet.Cell(row, 7).GetString();

                    var entry = new WTOEntry
                    {
                        EmployeeAFM = afm,
                        EmployeeLastName = lastName,
                        EmployeeFirstName = firstName,
                        WorkType = workType,
                        FromTime = fromTime,
                        ToTime = toTime
                    };

                    if (TypeComboBox.SelectedIndex == 0) // Daily
                    {
                        if (DateTime.TryParse(dateOrDay, out var parsedDate))
                            entry.Date = parsedDate;
                    }
                    else // Weekly
                    {
                        if (int.TryParse(dateOrDay, out var parsedDay))
                            entry.DayOfWeek = parsedDay;
                    }

                    WTOEntries.Add(entry);
                }

                MessageBox.Show($"Φορτώθηκαν {WTOEntries.Count} εγγραφές από το Excel.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα κατά την ανάγνωση του Excel:\n" + ex.Message, "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedRows_Click(object sender, RoutedEventArgs e)
        {
            var toDelete = WTOEntries.Where(e => e.IsSelected).ToList();

            if (toDelete.Count == 0)
            {
                MessageBox.Show("Δεν υπάρχουν επιλεγμένες εγγραφές για διαγραφή.", "Προσοχή", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Θέλετε να διαγράψετε {toDelete.Count} επιλεγμένες εγγραφές;", "Επιβεβαίωση", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var entry in toDelete)
                {
                    WTOEntries.Remove(entry);
                }
            }
        }

        private void ValidateEntries_Click(object sender, RoutedEventArgs e)
        {
            bool allValid = true;

            foreach (var entry in WTOEntries)
            {
                entry.HasError = false;

                // Έλεγχοι
                if (string.IsNullOrWhiteSpace(entry.EmployeeAFM) || entry.EmployeeAFM.Length != 9 || !entry.EmployeeAFM.All(char.IsDigit))
                    entry.HasError = true;

                if (string.IsNullOrWhiteSpace(entry.EmployeeLastName) || string.IsNullOrWhiteSpace(entry.EmployeeFirstName))
                    entry.HasError = true;

                if (string.IsNullOrWhiteSpace(entry.WorkType))
                    entry.HasError = true;

                if (string.IsNullOrWhiteSpace(entry.FromTime) || string.IsNullOrWhiteSpace(entry.ToTime))
                    entry.HasError = true;

                if (TypeComboBox.SelectedIndex == 0 && entry.Date == null)
                    entry.HasError = true;

                if (TypeComboBox.SelectedIndex == 1 && entry.DayOfWeek == null)
                    entry.HasError = true;

                if (entry.HasError)
                    allValid = false;
            }

            // Επαναφόρτωση DataGrid για να φανεί το styling
            WtoDataGrid.Items.Refresh();

            if (allValid)
            {
                MessageBox.Show("Όλες οι εγγραφές είναι έγκυρες!", "✅ Εντάξει", MessageBoxButton.OK, MessageBoxImage.Information);
                btnSubmit.IsEnabled = true;
                btnSubmit.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); // Πράσινο
            }
            else
            {
                MessageBox.Show("Υπάρχουν εγγραφές με σφάλματα. Διορθώστε τα και προσπαθήστε ξανά.", "❌ Σφάλματα", MessageBoxButton.OK, MessageBoxImage.Warning);
                btnSubmit.IsEnabled = false;
                btnSubmit.Background = new SolidColorBrush(Colors.Gray);
            }
        }
        private void chkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
            {
                bool isChecked = cb.IsChecked == true;

                foreach (var item in WTOEntries)
                {
                    item.IsSelected = isChecked;
                }

                WtoDataGrid.Items.Refresh();
            }
        }

        private void GenerateWeeklySchedule_Click(object sender, RoutedEventArgs e)
        {
            var window = new WeeklyScheduleWindow
            {
                Owner = Window.GetWindow(this)
            };

            //  Auto-fill αν υπάρχει επιλεγμένος υπάλληλος
            if (EmployeeComboBox.SelectedItem is Employee selectedEmployee)
            {
                window.PreFillFromEmployee(selectedEmployee);
            }

            if (window.ShowDialog() == true)
            {
                foreach (var entry in window.GeneratedEntries)
                {
                    WTOEntries.Add(entry);
                }

                MessageBox.Show($"{window.GeneratedEntries.Count} εγγραφές προστέθηκαν επιτυχώς.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (WTOEntries.Count == 0)
            {
                MessageBox.Show("Δεν υπάρχουν εγγραφές για υποβολή.", "Προσοχή", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
            string? mode = selectedItem?.Tag?.ToString();

            bool isWeekly = mode == "Weekly";

            bool success = await ErganiApiService.SubmitWTOAsync(WTOEntries.ToList(), isWeekly);

            if (success)
                MessageBox.Show("Η υποβολή στο ΕΡΓΑΝΗ ολοκληρώθηκε με επιτυχία.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Απέτυχε η υποβολή στο ΕΡΓΑΝΗ.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
