using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ParserWpf.Wpf.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility VisibilityTrue { get; set; } = Visibility.Visible;

        public Visibility VisibilityFalse { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool flag ? flag ? VisibilityTrue : VisibilityFalse : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}