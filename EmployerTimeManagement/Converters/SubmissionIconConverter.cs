using System;
using System.Globalization;
using System.Windows.Data;

namespace EmployerTimeManagement.Converters
{
    public class SubmissionIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string submissionType = value?.ToString();

            return submissionType == "Live" ? "⚡" : "✍️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
