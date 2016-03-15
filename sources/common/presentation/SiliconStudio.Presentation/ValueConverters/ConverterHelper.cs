using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SiliconStudio.Presentation.ValueConverters
{
    internal static class ConverterHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ConvertToDouble(object value, IFormatProvider culture)
        {
            return value != DependencyProperty.UnsetValue ? Convert.ToDouble(value, culture) : default(double);
        }
    }
}
