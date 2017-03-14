using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Presentation.ValueConverters
{
    [ValueConversion(typeof(object), typeof(IEnumerable<object>))]
    public class Yield : OneWayValueConverter<Yield>
    {
        /// <inheritdoc />
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == DependencyProperty.UnsetValue ? DependencyProperty.UnsetValue : value.Yield();
        }
    }
}
