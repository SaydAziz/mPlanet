using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace mPlanet.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToUpper())
                {
                    case "FOUND":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
                    case "MISSING":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
                    case "EXTRA":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12"));
                    case "ОЖИДАЕТСЯ":
                    default:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                }
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}