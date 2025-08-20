using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace mPlanet.Converters
{
    public class PhotoVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return Visibility.Collapsed;
            
            bool showPhotos = values[0] is bool sp && sp;
            string photoPath = values[1] as string;
            
            return showPhotos ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}