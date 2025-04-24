using System;
using System.Windows;
using System.Windows.Controls;
using EmployerTimeManagement.Themes;

namespace EmployerTimeManagement
{
    public partial class ThemeSettingsControl : UserControl
    {
        public ThemeSettingsControl()
        {
            InitializeComponent();
        }

        private void ApplyTheme_Click(object sender, RoutedEventArgs e)
        {
            string selectedTheme = "ModernDark";

            try
            {
                var themeDict = new ResourceDictionary
                {
                    Source = new Uri($"/Themes/{selectedTheme}.xaml", UriKind.Relative)
                };

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(themeDict);

                ThemeConfig.Save(new ThemeSettings { SelectedTheme = selectedTheme });

                MessageBox.Show("Το θέμα \"Modern Dark\" εφαρμόστηκε!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα κατά την εφαρμογή θέματος: " + ex.Message);
            }
        }
    }
}
