using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EmployerTimeManagement.Attendance;
using EmployerTimeManagement.Controls.E10;
using EmployerTimeManagement.Controls.E5;
using EmployerTimeManagement.Controls.E6;
using EmployerTimeManagement.Controls.E7;
using EmployerTimeManagement.Ergani;
using EmployerTimeManagement.Reports;
using EmployerTimeManagement.Settings;


namespace EmployerTimeManagement
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleSettingsMenu(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = SettingsPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void SetActiveSidebarButton(Button activeButton)
        {
            foreach (var child in SidebarPanel.Children)
            {
                if (child is Button btn)
                    btn.Tag = "Inactive";
            }

            activeButton.Tag = "Active";
        }

        private void OpenDashboard(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new DashboardControl());
        }

        private void OpenAddEmployeePage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new AddEmployeeControl());
        }

        private void OpenWTOPage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new EmployerTimeManagement.Controls.WTO.WTOControl());
        }





        private void OpenAttendancePage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new AttendanceControl());
        }

        private void OpenOvertimePage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new EmployerTimeManagement.Controls.Overtime.OvertimeControl());
        }

        private void OpenWorkingStatusChangePage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new Controls.WorkingStatusChange.WorkingStatusChangeControl());
        }

        private void OpenHolidayPage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new Controls.Holidays.HolidayControl());
        }


       

        private void OpenReportsPage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new ReportsControl());
        }

        private void OpenErganiPage(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new ErganiControl());
        }
        private void OpenE3Page(object sender, RoutedEventArgs e)
        {
            SetActiveSidebarButton((Button)sender);
            LoadPage(new EmployerTimeManagement.Controls.E3Control.E3Control());
        }

        private void OpenE1Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new Controls.E1Control.E1Control());
        }

        private void OpenE2Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new Controls.E2Control.E2Control());
        }

        private void OpenE4Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new Controls.E4.E4Control());

        }

        private void OpenE5Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new E5Control());
        }


        private void OpenE6Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new E6Control());
        }


        private void OpenE7Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new E7Control());
        }


        private void OpenE9Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new EmployerTimeManagement.Controls.E9.E9Control());
        }


        private void OpenE10Page(object sender, RoutedEventArgs e)
        {
            LoadPage(new E10Control());
        }



        private void ToggleErganiForms_Click(object sender, RoutedEventArgs e)
        {
            ErganiFormsPanel.Visibility = ErganiFormsPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }




        private void OpenCompanyInfo(object sender, RoutedEventArgs e)
        {
            LoadPage(new CompanyInfoControl());
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void OpenChangePassword(object sender, RoutedEventArgs e)
        {
            LoadPage(new ChangePasswordControl());
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void OpenUserManagement(object sender, RoutedEventArgs e)
        {
            LoadPage(new UserManagementControl());
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void OpenLanguageSettings(object sender, RoutedEventArgs e)
        {
            LoadPage(new LanguageSettingsControl());
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void OpenThemeSettings(object sender, RoutedEventArgs e)
        {
            LoadPage(new ThemeSettingsControl());
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void CreateBackup(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Collapsed;

            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "employer.db");
                if (!File.Exists(dbPath))
                {
                    MessageBox.Show($"⚠️ Η βάση δεδομένων δεν βρέθηκε:\n{dbPath}", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                File.Copy(dbPath, backupPath, true);
                MessageBox.Show("✅ Δημιουργήθηκε αντίγραφο ασφαλείας!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Σφάλμα κατά το backup: {ex.Message}", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreBackup(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Collapsed;

            var openFile = new Microsoft.Win32.OpenFileDialog { Filter = "SQLite DB (*.db)|*.db" };

            if (openFile.ShowDialog() == true)
            {
                try
                {
                    string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "employer.db");
                    File.Copy(openFile.FileName, targetPath, true);
                    MessageBox.Show("✅ Επαναφορά backup επιτυχής!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"⚠️ Σφάλμα κατά την επαναφορά: {ex.Message}", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadPage(UserControl page)
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(page);
        }
    }
}
