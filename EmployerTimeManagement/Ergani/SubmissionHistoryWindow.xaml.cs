using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;

namespace EmployerTimeManagement.Ergani
{
    public partial class SubmissionHistoryWindow : Window
    {
        private readonly bool _showOnlyFailed;

        public SubmissionHistoryWindow(bool showOnlyFailed = false)
        {
            InitializeComponent();
            _showOnlyFailed = showOnlyFailed;
            LoadHistory();
        }

        private void LoadHistory()
        {
            using var context = new AppDbContext();

            var query = context.WorkLogs.AsQueryable();

            if (_showOnlyFailed)
                query = query.Where(w => !w.IsSent);

            var history = query
                .OrderByDescending(w => w.SentAt)
                .Select(w => new
                {
                    w.Id,
                    w.f_afm,
                    w.f_onoma,
                    w.f_eponymo,
                    w.Date,
                    f_type = w.f_type == 0 ? "Έναρξη" : "Λήξη",
                    w.SentAt,
                    StatusLabel = w.IsSent ? "Απεστάλη" : "❌ Απέτυχε",
                    SubmissionType = w.f_aitiologia == "Αυτόματη Καταγραφή" ? "Live" : "Χειροκίνητη"
                })
                .ToList();

            historyGrid.ItemsSource = history;
        }

        private async void AutoResend_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();

            var failed = context.WorkLogs
                .Where(w => !w.IsSent && w.f_aitiologia == "Αυτόματη Καταγραφή")
                .ToList();

            if (failed.Count == 0)
            {
                MessageBox.Show("Δεν υπάρχουν αποτυχημένες αυτόματες αποστολές.", "Πληροφορία", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await ErganiApiService.SubmitToErganiAsync(failed); // ✅ σωστή static κλήση

            MessageBox.Show("Η αυτόματη αποστολή ολοκληρώθηκε.", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadHistory(); // ανανέωση πίνακα
        }

        private void ManualResend_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Η χειροκίνητη αποστολή δεν έχει υλοποιηθεί ακόμα.", "Υπό Κατασκευή", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
