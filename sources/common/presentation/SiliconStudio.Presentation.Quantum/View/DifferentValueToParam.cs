using System;
using System.Collections.Generic;
using System.Globalization;
using SiliconStudio.Presentation.Quantum.ViewModels;
using SiliconStudio.Presentation.ValueConverters;

namespace SiliconStudio.Presentation.Quantum.View
{
    public class DifferentValueToParam : ValueConverterBase<DifferentValueToParam>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != NodeViewModel.DifferentValues ? value : parameter;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}