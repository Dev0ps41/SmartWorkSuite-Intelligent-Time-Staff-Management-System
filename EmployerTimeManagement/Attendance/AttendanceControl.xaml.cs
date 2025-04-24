using EmployerTimeManagement.Data;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Attendance
{
    public partial class AttendanceControl : UserControl
    {
        private readonly AppDbContext _context;
        private int? currentType = null; // 0=Έναρξη, 1=Λήξη

        public AttendanceControl()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadEmployees();
            LoadWorkLogPairs(); // εμφάνιση ζευγών
        }

        private void LoadEmployees()
        {
            cmbEmployee.ItemsSource = _context.Employees
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToList();
        }

        private void LoadWorkLogPairs()
        {
            using var db = new AppDbContext();

            var allLogs = db.WorkLogs
                .Include(w => w.Employee)
                .ToList(); // φορτώνει πρώτα στη μνήμη

            var workLogPairs = allLogs
                .GroupBy(w => new { w.EmployeeId, Date = w.f_date.Date })
                .Select((g, index) => new WorkLogPair
                {
                    Id = index + 1,
                    EmployeeId = g.Key.EmployeeId,
                    AFM = g.First().Employee.AFM,
                    FirstName = g.First().Employee.FirstName,
                    LastName = g.First().Employee.LastName,
                    Date = g.Key.Date,
                    EntryTime = g
                        .Where(w => w.f_type == 0)
                        .OrderBy(w => w.f_date)
                        .FirstOrDefault()
                        ?.f_date.TimeOfDay,
                    ExitTime = g
                        .Where(w => w.f_type == 1)
                        .OrderBy(w => w.f_date)
                        .FirstOrDefault()
                        ?.f_date.TimeOfDay
                })
                .ToList();

            MyDataGrid.ItemsSource = workLogPairs;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmployee.SelectedItem is Employee selectedEmployee &&
                datePicker.SelectedDate.HasValue &&
                (entryTimePicker.SelectedTime.HasValue || exitTimePicker.SelectedTime.HasValue))
            {
                string entryTime = entryTimePicker.SelectedTime?.ToString("HH:mm") ?? "";
                string exitTime = exitTimePicker.SelectedTime?.ToString("HH:mm") ?? "";

                int type = entryTime != "" ? 0 : 1;

                if (type == 1)
                {
                    bool hasEntry = _context.WorkLogs.Any(w =>
                        w.EmployeeId == selectedEmployee.Id &&
                        w.f_type == 0 &&
                        w.f_date.Date == datePicker.SelectedDate.Value.Date);

                    if (!hasEntry)
                    {
                        MessageBox.Show("Δεν μπορείτε να καταγράψετε έξοδο χωρίς να έχει προηγηθεί είσοδος.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var workLog = new WorkLog
                {
                    EmployeeId = selectedEmployee.Id,
                    Date = datePicker.SelectedDate.Value,
                    EntryTime = entryTime,
                    ExitTime = exitTime,
                    f_afm = selectedEmployee.AFM,
                    f_eponymo = selectedEmployee.LastName,
                    f_onoma = selectedEmployee.FirstName,
                    f_type = type,
                    f_reference_date = datePicker.SelectedDate.Value,
                    f_date = datePicker.SelectedDate.Value + TimeSpan.Parse(type == 0 ? entryTime : exitTime),
                    f_aitiologia = "Αυτόματη Καταγραφή"
                };

                _context.WorkLogs.Add(workLog);
                _context.SaveChanges();

                // ✅ LIVE ΑΠΟΣΤΟΛΗ στο ΕΡΓΑΝΗ αν είναι ενεργοποιημένη
                var companyInfo = _context.CompanyInfos.FirstOrDefault();
                if (companyInfo?.IsLiveErganiEnabled == true)
                {
                    await ErganiApiService.SubmitToErganiAsync(new List<WorkLog> { workLog });
                }

                MessageBox.Show("Η καταγραφή αποθηκεύτηκε!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);

                entryTimePicker.SelectedTime = null;
                exitTimePicker.SelectedTime = null;
                datePicker.SelectedDate = null;
                currentType = null;

                LoadWorkLogPairs();
            }
            else
            {
                MessageBox.Show("Συμπλήρωσε τουλάχιστον ώρα εισόδου ή εξόδου!", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is WorkLog selected)
            {
                if (MessageBox.Show("Να διαγραφεί η καταγραφή;", "Επιβεβαίωση", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _context.WorkLogs.Remove(selected);
                    _context.SaveChanges();
                    LoadWorkLogPairs();
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is WorkLog selected)
            {
                string newEntry = Interaction.InputBox("Ώρα εισόδου:", "Επεξεργασία", selected.EntryTime ?? "");
                string newExit = Interaction.InputBox("Ώρα εξόδου:", "Επεξεργασία", selected.ExitTime ?? "");

                if (!string.IsNullOrWhiteSpace(newEntry) && !string.IsNullOrWhiteSpace(newExit))
                {
                    selected.EntryTime = newEntry;
                    selected.ExitTime = newExit;
                    _context.SaveChanges();
                    LoadWorkLogPairs();
                }
            }
        }
    }
}
