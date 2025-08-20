using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace mPlanet.Converters
{
    public class PhotoSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string photoPath = value as string;
            
            if (string.IsNullOrEmpty(photoPath))
            {
                return null; // Will show placeholder
            }
            
            try
            {
                // For WPF resources, create pack URI
                if (photoPath.StartsWith("Assets/"))
                {
                    string packUri = $"pack://application:,,,/{photoPath}";
                    var uri = new Uri(packUri, UriKind.Absolute);
                    return new BitmapImage(uri);
                }
                
                // For absolute file paths, check if file exists
                if (File.Exists(photoPath))
                {
                    return new BitmapImage(new Uri(photoPath, UriKind.Absolute));
                }
                
                return null; // Will show placeholder
            }
            catch (Exception)
            {
                return null; // Will show placeholder on any error
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}