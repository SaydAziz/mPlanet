using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace mPlanet.Converters
{
    public class ItemIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ListViewItem listViewItem)
            {
                var listView = FindParent<ListView>(listViewItem);
                if (listView != null)
                {
                    int index = listView.ItemContainerGenerator.IndexFromContainer(listViewItem);
                    return (index + 1).ToString();
                }
            }

            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static T FindParent<T>(System.Windows.DependencyObject child) where T : System.Windows.DependencyObject
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);

            if (parent == null)
                return null;

            if (parent is T parentT)
                return parentT;

            return FindParent<T>(parent);
        }
    }
}