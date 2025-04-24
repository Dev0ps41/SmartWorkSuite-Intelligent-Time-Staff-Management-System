using System;
using System.Globalization;
using System.Windows.Data;

namespace EmployerTimeManagement.Ergani
{
    public class LiveErganiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = value is bool b && b;
            return isChecked ? "Ενεργή" : "Ανενεργή";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
