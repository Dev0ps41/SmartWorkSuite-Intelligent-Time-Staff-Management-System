using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Reports
{
    public partial class ReportsControl : UserControl
    {
        private readonly AppDbContext _context;

        public ReportsControl()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            cmbEmployee.ItemsSource = _context.Employees.ToList();
            cmbEmployee.DisplayMemberPath = "FullName";
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmployee.SelectedItem is Employee selectedEmployee &&
                dpFromDate.SelectedDate.HasValue &&
                dpToDate.SelectedDate.HasValue)
            {
                DateTime from = dpFromDate.SelectedDate.Value.Date;
                DateTime to = dpToDate.SelectedDate.Value.Date;

                var logs = _context.WorkLogs
                    .Include(w => w.Employee)
                    .Where(w => w.EmployeeId == selectedEmployee.Id &&
                                w.f_date.Date >= from &&
                                w.f_date.Date <= to)
                    .AsEnumerable()
                    .GroupBy(w => new { w.EmployeeId, Date = w.f_date.Date })
                    .Select((g, index) => new WorkLogPair
                    {
                        Id = index + 1,
                        EmployeeId = g.Key.EmployeeId,
                        AFM = g.First().Employee.AFM,
                        FirstName = g.First().Employee.FirstName,
                        LastName = g.First().Employee.LastName,
                        Date = g.Key.Date,
                        EntryTime = g.Where(w => w.f_type == 0).OrderBy(w => w.f_date).FirstOrDefault()?.f_date.TimeOfDay,
                        ExitTime = g.Where(w => w.f_type == 1).OrderBy(w => w.f_date).FirstOrDefault()?.f_date.TimeOfDay
                    })
                    .ToList();

                reportGrid.ItemsSource = logs;
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (reportGrid.ItemsSource is not IEnumerable<WorkLogPair> pairs) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Ημερομηνία,ΑΦΜ,Όνομα,Επώνυμο,Ώρα Εισόδου,Ώρα Εξόδου");

            foreach (var pair in pairs)
            {
                sb.AppendLine($"{pair.Date:dd/MM/yyyy},{pair.AFM},{pair.FirstName},{pair.LastName},{pair.EntryTime},{pair.ExitTime}");
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV Files|*.csv",
                FileName = "Αναφορά.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Η εξαγωγή ολοκληρώθηκε με επιτυχία.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Η εξαγωγή σε PDF δεν υποστηρίζεται ακόμη.", "Υπό κατασκευή", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
