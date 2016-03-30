using System;
using System.Globalization;
using System.Windows.Data;
using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.Presentation.ValueConverters
{
    [ValueConversion(typeof(VectorEditingMode), typeof(bool?))]
    public class VectorEditingModeToBool : ValueConverterBase<VectorEditingModeToBool>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var toolMode = (VectorEditingMode)System.Convert.ChangeType(value, typeof(VectorEditingMode));
            switch (toolMode)
            {
                case VectorEditingMode.Normal:
                    return false;

                case VectorEditingMode.AllComponents:
                    return true;

                case VectorEditingMode.Length:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return VectorEditingMode.Length;
            
            return System.Convert.ToBoolean(value) ? VectorEditingMode.AllComponents : VectorEditingMode.Normal;
        }
    }
}
