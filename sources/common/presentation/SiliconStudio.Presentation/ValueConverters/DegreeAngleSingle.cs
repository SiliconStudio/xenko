// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class AngleSingleToDegrees : ValueConverterBase<AngleSingleToDegrees>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return targetType == typeof(double) ? ConverterHelper.ConvertToAngleSingle(value, culture).Degrees : ConverterHelper.TryConvertToAngleSingle(value, culture)?.Degrees;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValue = targetType == typeof(AngleSingle) ? ConverterHelper.ConvertToDouble(value, culture) : ConverterHelper.TryConvertToDouble(value, culture);
            return doubleValue != null ? (object)new AngleSingle((float)doubleValue.Value, AngleType.Degree) : null;
        }
    }
}
