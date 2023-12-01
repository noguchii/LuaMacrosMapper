using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LuaMacrosMapper.Behaviors
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                if (!Inverse)
                {
                    return isVisible ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return isVisible ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            return Inverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                switch (visibility)
                {
                    case Visibility.Collapsed:
                    case Visibility.Hidden:
                        return Inverse;
                    case Visibility.Visible:
                    default:
                        return !Inverse;
                }
            }

            throw new NotImplementedException();
        }
    }
}
