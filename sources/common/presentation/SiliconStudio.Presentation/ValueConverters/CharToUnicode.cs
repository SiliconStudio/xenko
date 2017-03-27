// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a <see cref="char"/> value to the integer representation of its unicode value.
    /// <see cref="ConvertBack"/> is supported.
    /// </summary>
    public class CharToUnicode : ValueConverterBase<CharToUnicode>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return targetType == typeof(int) ? ConverterHelper.ConvertToInt32(value, culture) : ConverterHelper.TryConvertToInt32(value, culture);
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return targetType == typeof(char) ? ConverterHelper.ConvertToChar(value, culture) : ConverterHelper.TryConvertToChar(value, culture);
        }
    }
}
