using System;
using System.Globalization;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class LightenDarken : ValueConverterBase<LightenDarken>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var amount = System.Convert.ToInt32(parameter);

            if (value is Color)
            {
                var color = (Color)value;
                return DoLightenDarken(color, amount);
            }
            if (value is SolidColorBrush)
            {
                var brush = (SolidColorBrush)value;
                brush = brush.CloneCurrentValue();
                brush.Color = DoLightenDarken(brush.Color, amount);
                brush.Freeze();
                return brush;
            }
            throw new NotSupportedException("Requested conversion is not supported.");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var amount = System.Convert.ToInt32(parameter);
            return Convert(value, targetType, -amount, culture);
        }

        private static byte Clamp(int value)
        {
            if (value < byte.MinValue) return byte.MinValue;
            if (value > byte.MaxValue) return byte.MaxValue;
            return (byte)value;
        }

        private static Color DoLightenDarken(Color color, int amount)
        {
            var r = color.R + amount;
            var g = color.G + amount;
            var b = color.B + amount;
            color.R = Clamp(r);
            color.G = Clamp(g);
            color.B = Clamp(b);

            return color;
        }
    }
}
