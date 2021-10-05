using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace VRCEventUtil.Converters
{
    class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Trueの時のVisibilityの値
        /// </summary>
        public Visibility TrueVisibility { get; set; } = Visibility.Visible;

        /// <summary>
        /// Falseの時のVisibilityの値
        /// </summary>
        public Visibility FalseVisibility { get; set; } = Visibility.Hidden;

        /// <summary>
        /// bool値を反転してから変換するか
        /// </summary>
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bVal)
            {
                return (bVal ^ Invert) ? TrueVisibility : FalseVisibility;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
