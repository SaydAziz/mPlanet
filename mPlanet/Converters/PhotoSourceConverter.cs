using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace mPlanet.Converters
{
    public class PhotoSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string photoPath = value as string;
            
            if (string.IsNullOrEmpty(photoPath) || !File.Exists(photoPath))
            {
                return null; // Will show placeholder
            }
            
            return photoPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}