using System.Collections.Generic;
using System.Windows;
using EmployerTimeManagement.Models;

namespace EmployerTimeManagement.Ergani
{
    public partial class ValidationPreviewWindow : Window
    {
        public ValidationPreviewWindow(List<WorkLog> logs, string referenceDate, string eventDate, string eventTime, string reason, string companyName, string afm, string branchId)
        {
            InitializeComponent();
            PreviewGrid.ItemsSource = BuildPreviewRows(logs, referenceDate, eventDate, eventTime, reason, companyName, afm, branchId);
        }

        private List<PreviewRow> BuildPreviewRows(List<WorkLog> logs, string refDate, string evtDate, string evtTime, string reason, string compName, string afm, string branchId)
        {
            var list = new List<PreviewRow>();
            foreach (var log in logs)
            {
                list.Add(new PreviewRow
                {
                    EmployeeAFM = log.f_afm,
                    FirstName = log.f_onoma,
                    LastName = log.f_eponymo,
                    WorkDate = log.Date.ToString("dd/MM/yyyy"),
                    EntryTime = !string.IsNullOrWhiteSpace(log.EntryTime) ? log.EntryTime : "-",
                    ExitTime = !string.IsNullOrWhiteSpace(log.ExitTime) ? log.ExitTime : "-",
                    Type = log.f_type == 0 ? "Έναρξη" : "Λήξη",
                    ReferenceDate = refDate,
                    EventDate = $"{evtDate} {evtTime}",
                    Reason = string.IsNullOrWhiteSpace(reason) ? "-" : reason,
                    CompanyName = compName,
                    CompanyAFM = afm,
                    BranchId = branchId
                });
            }
            return list;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class PreviewRow
    {
        public string EmployeeAFM { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WorkDate { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }
        public string Type { get; set; }
        public string ReferenceDate { get; set; }
        public string EventDate { get; set; }
        public string Reason { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAFM { get; set; }
        public string BranchId { get; set; }
    }
}
