using System;
using System.Globalization;
using System.Windows.Data;

namespace EmployerTimeManagement.Converters
{
    public class DayOfWeekConverter : IValueConverter
    {
        private static readonly string[] GreekDays =
        {
            "Κυριακή", "Δευτέρα", "Τρίτη", "Τετάρτη", "Πέμπτη", "Παρασκευή", "Σάββατο"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int day && day >= 0 && day <= 6)
                return GreekDays[day];
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dayIndex = Array.IndexOf(GreekDays, value?.ToString());
            return dayIndex >= 0 ? dayIndex : (object)0;
        }
    }
}
