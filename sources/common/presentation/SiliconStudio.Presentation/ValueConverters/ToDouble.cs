// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This value converter will convert any numeric value to double. <see cref="ConvertBack"/> is supported and
    /// will convert the value to the target if it is numeric, otherwise it returns the value as-is.
    /// </summary>
    public class ToDouble : ValueConverterBase<ToDouble>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return typeof(double).CastToNumericType(value);
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return targetType.IsNumeric() ? targetType.CastToNumericType(value) : value;
        }
    }
}
