using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using EmployerTimeManagement.LoginScreen; // πρόσθεσε αυτό αν το LoginWindow είναι στον φάκελο LoginWindow

namespace EmployerTimeManagement
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //  Ρύθμιση Ελληνικής γλώσσας και μορφής για την εφαρμογή
            CultureInfo greekCulture = new CultureInfo("el-GR");
            Thread.CurrentThread.CurrentCulture = greekCulture;
            Thread.CurrentThread.CurrentUICulture = greekCulture;
            CultureInfo.DefaultThreadCurrentCulture = greekCulture;
            CultureInfo.DefaultThreadCurrentUICulture = greekCulture; ;
            // Για να επηρεαστούν και τα DatePicker κ.ά. XAML στοιχεία

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(greekCulture.IetfLanguageTag)));


            // Εκκίνηση με LoginWindow
            // Δημιουργία MainWindow
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();

            // Εύρεση blur overlay
            var blur = mainWindow.FindName("BlurOverlay") as UIElement;
            if (blur != null)
                blur.Visibility = Visibility.Visible;

            // Εμφάνιση Login
            var login = new LoginWindow { Owner = mainWindow };
            bool? result = login.ShowDialog();

            if (result == true)
            {
                if (blur != null)
                    blur.Visibility = Visibility.Collapsed;
            }
            else
            {
                mainWindow.Close();
                Shutdown();
            }
        }
    }
    
}
