using System;
using System.Globalization;
using System.Windows.Data;

namespace EmployerTimeManagement.Converters
{
    public class BooleanToSentStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? "Απεστάλη ✅" : "Δεν Απεστάλη ❌";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Contains("Απεστάλη") == true;
        }
    }
}
