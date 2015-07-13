using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class FormatString : OneWayValueConverter<FormatString>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var format = parameter as string;
            return string.Format(format ?? "{0}", value);
        }
    }
}