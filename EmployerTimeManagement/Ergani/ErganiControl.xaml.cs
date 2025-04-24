using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EmployerTimeManagement.Ergani
{
    public partial class ErganiControl : UserControl
    {
        private readonly AppDbContext _context;
        private CompanyInfo _companyInfo;
        private List<WorkLog> allLogs;
        public bool IsLiveSubmissionEnabled { get; set; }

        public ErganiControl()
        {
            InitializeComponent();
            _context = new AppDbContext();

            _companyInfo = _context.CompanyInfos.FirstOrDefault();
            IsLiveSubmissionEnabled = _companyInfo?.IsLiveErganiEnabled ?? false;
            toggleLiveSubmit.IsChecked = IsLiveSubmissionEnabled;
            UpdateLiveSubmitUI();

            LoadWorkLogs();
        }

        private void ToggleLiveSubmit_Checked(object sender, RoutedEventArgs e) => UpdateLiveErganiSetting(true);
        private void ToggleLiveSubmit_Unchecked(object sender, RoutedEventArgs e) => UpdateLiveErganiSetting(false);

        private void UpdateLiveErganiSetting(bool isEnabled)
        {
            IsLiveSubmissionEnabled = isEnabled;

            if (_companyInfo != null)
            {
                _companyInfo.IsLiveErganiEnabled = isEnabled;
                _context.SaveChanges();
            }

            UpdateLiveSubmitUI();
        }

        private void UpdateLiveSubmitUI()
        {
            if (toggleLiveSubmit != null)
                toggleLiveSubmit.Background = IsLiveSubmissionEnabled ? Brushes.Green : Brushes.Red;

            if (btnValidate != null)
            {
                btnValidate.IsEnabled = !IsLiveSubmissionEnabled;
                btnValidate.Background = IsLiveSubmissionEnabled ? Brushes.Gray : Brushes.DarkOrange;
            }
        }

        private void LoadWorkLogs()
        {
            allLogs = _context.WorkLogs.Include(w => w.Employee).ToList();
            var workLogPairs = allLogs.GroupBy(w => new { w.EmployeeId, Date = w.f_date.Date })
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
                }).ToList();

            erganiGrid.ItemsSource = workLogPairs;
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerCheckbox)
            {
                bool isChecked = headerCheckbox.IsChecked ?? false;
                foreach (var item in erganiGrid.ItemsSource as IEnumerable<WorkLogPair>)
                    item.IsSelected = isChecked;
                erganiGrid.Items.Refresh();
            }
        }

        private void ValidateData_Click(object sender, RoutedEventArgs e)
        {
            var selectedPairs = ((List<WorkLogPair>)erganiGrid.ItemsSource).Where(p => p.IsSelected).ToList();
            if (!selectedPairs.Any())
            {
                MessageBox.Show("Επιλέξτε τουλάχιστον μία εγγραφή.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpReferenceDate.SelectedDate == null || dpEventDate.SelectedDate == null || !timeEventPicker.SelectedTime.HasValue)
            {
                MessageBox.Show("Συμπληρώστε την ημερομηνία αναφοράς, ημερομηνία και ώρα γεγονότος.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedLogs = new List<WorkLog>();
            foreach (var pair in selectedPairs)
            {
                var entry = allLogs.FirstOrDefault(w => w.EmployeeId == pair.EmployeeId && w.f_type == 0 && w.f_date.Date == pair.Date);
                var exit = allLogs.FirstOrDefault(w => w.EmployeeId == pair.EmployeeId && w.f_type == 1 && w.f_date.Date == pair.Date);
                if (entry != null) selectedLogs.Add(entry);
                if (exit != null) selectedLogs.Add(exit);
            }

            var previewWindow = new ValidationPreviewWindow(
                selectedLogs,
                dpReferenceDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                dpEventDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                timeEventPicker.SelectedTime.Value.ToString(@"hh\:mm"),
                txtReason.Text,
                _companyInfo?.EmployerName,
                _companyInfo?.AFM,
                _companyInfo?.BranchId);

            if (previewWindow.ShowDialog() == true)
            {
                btnSubmitErgani.IsEnabled = true;
                btnSubmitErgani.Background = Brushes.Green;
            }
        }

        private async void SubmitToErgani_Click(object sender, RoutedEventArgs e)
        {
            var selectedPairs = ((List<WorkLogPair>)erganiGrid.ItemsSource).Where(p => p.IsSelected).ToList();
            if (!selectedPairs.Any())
            {
                MessageBox.Show("Επιλέξτε τουλάχιστον μία εγγραφή.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpReferenceDate.SelectedDate == null || dpEventDate.SelectedDate == null || !timeEventPicker.SelectedTime.HasValue)
            {
                MessageBox.Show("Συμπληρώστε την ημερομηνία αναφοράς, ημερομηνία και ώρα γεγονότος.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var time = timeEventPicker.SelectedTime.Value.TimeOfDay;
            var selectedLogs = new List<WorkLog>();

            foreach (var pair in selectedPairs)
            {
                var entryLog = allLogs.FirstOrDefault(w => w.EmployeeId == pair.EmployeeId && w.f_type == 0 && w.f_date.Date == pair.Date);
                var exitLog = allLogs.FirstOrDefault(w => w.EmployeeId == pair.EmployeeId && w.f_type == 1 && w.f_date.Date == pair.Date);

                if (entryLog != null)
                {
                    entryLog.f_reference_date = dpReferenceDate.SelectedDate.Value;
                    entryLog.f_date = dpEventDate.SelectedDate.Value.Date + time;
                    entryLog.f_aitiologia = string.IsNullOrWhiteSpace(txtReason.Text) ? null : txtReason.Text;
                    selectedLogs.Add(entryLog);
                }

                if (exitLog != null)
                {
                    exitLog.f_reference_date = dpReferenceDate.SelectedDate.Value;
                    exitLog.f_date = dpEventDate.SelectedDate.Value.Date + time;
                    exitLog.f_aitiologia = string.IsNullOrWhiteSpace(txtReason.Text) ? null : txtReason.Text;
                    selectedLogs.Add(exitLog);
                }
            }

            try
            {
                _context.SaveChanges();
                bool result = await ErganiApiService.SubmitToErganiAsync(selectedLogs);
                MessageBox.Show(result ? "Η αποστολή στο ΕΡΓΑΝΗ ολοκληρώθηκε με επιτυχία." : "❌ Απέτυχε η αποστολή στο ΕΡΓΑΝΗ.", result ? "✅ Επιτυχία" : "Σφάλμα", MessageBoxButton.OK, result ? MessageBoxImage.Information : MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα κατά την αποθήκευση ή αποστολή: " + ex.Message, "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            dpReferenceDate.SelectedDate = null;
            dpEventDate.SelectedDate = null;
            timeEventPicker.SelectedTime = null;
            txtReason.Text = string.Empty;
            btnSubmitErgani.IsEnabled = false;
            btnSubmitErgani.Background = Brushes.Gray;
            LoadWorkLogs();
        }

        private void PreviewJson_Click(object sender, RoutedEventArgs e)
        {
            var selectedPairs = ((List<WorkLogPair>)erganiGrid.ItemsSource).Where(p => p.IsSelected).ToList();
            if (!selectedPairs.Any())
            {
                MessageBox.Show("Επιλέξτε τουλάχιστον μία εγγραφή.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedLogs = new List<WorkLog>();
            foreach (var pair in selectedPairs)
            {
                var entry = allLogs.FirstOrDefault(w => w.EmployeeId == pair.EmployeeId && w.f_type == 0 && w.f_date.Date == pair.Date);
                var exit = allLogs.FirstOrDefault(w => w.EmployeeId == pair.EmployeeId && w.f_type == 1 && w.f_date.Date == pair.Date);
                if (entry != null) selectedLogs.Add(entry);
                if (exit != null) selectedLogs.Add(exit);
            }

            var jsonObjects = selectedLogs.Select(log => new
            {
                f_afm = log.f_afm,
                f_eponymo = log.f_eponymo,
                f_onoma = log.f_onoma,
                f_type = log.f_type,
                f_reference_date = log.f_reference_date.ToString("yyyy-MM-dd"),
                f_date = log.f_date.ToString("yyyy-MM-ddTHH:mm:ss"),
                f_aitiologia = log.f_aitiologia,
                employer_afm = _companyInfo?.AFM,
                employer_name = _companyInfo?.EmployerName,
                branch_id = _companyInfo?.BranchId
            });

            txtJsonPreview.Text = JsonSerializer.Serialize(jsonObjects, new JsonSerializerOptions { WriteIndented = true });
        }

        private void CopyJson_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtJsonPreview.Text))
            {
                Clipboard.SetText(txtJsonPreview.Text);
                MessageBox.Show("Το JSON αντιγράφηκε στο πρόχειρο!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowSubmissionHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new SubmissionHistoryWindow();
            historyWindow.Owner = Window.GetWindow(this);
            historyWindow.ShowDialog();
        }
    }
}
