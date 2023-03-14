using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Streamline.Module.Admin.Converter
{
    [SuppressMessage("NDepend", "ND1700:PotentiallyDeadTypes", Justification = "This class is reffered from XAML")]
    internal sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;

            bool boolValue = false;
            
            if(bool.TryParse(value.ToString(), out boolValue) && boolValue)
            {
                return IsInverted ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return IsInverted ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
