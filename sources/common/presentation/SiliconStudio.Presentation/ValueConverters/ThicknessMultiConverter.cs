using System;
using System.Globalization;
using System.Windows;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class ThicknessMultiConverter : OneWayMultiValueConverter<ThicknessMultiConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var left = values.Length > 0 ? ConverterHelper.ConvertToDouble(values[0], culture) : 0.0;
            var top = values.Length > 1 ? ConverterHelper.ConvertToDouble(values[1], culture) : 0.0;
            var right = values.Length > 2 ? ConverterHelper.ConvertToDouble(values[2], culture) : 0.0;
            var bottom = values.Length > 3 ? ConverterHelper.ConvertToDouble(values[3], culture) : 0.0;

            return new Thickness(left, top, right, bottom);
        }
    }
}
