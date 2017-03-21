// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a <see cref="char"/> value to the integer representation of its unicode value.
    /// <see cref="ConvertBack"/> is supported and will return the default char value if the given integer value can't be converted.
    /// </summary>
    public class CharToUnicode : ValueConverterBase<CharToUnicode>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.IsNullable())
                return value is char ? (object)ConverterHelper.ConvertToInt32(value, culture) : null;

            return ConverterHelper.ConvertToInt32(value, culture);
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (targetType.IsNullable())
                    return value is int ? (object)ConverterHelper.ConvertToChar(ConverterHelper.ConvertToInt32(value, culture), culture) : null;

                return ConverterHelper.ConvertToChar(ConverterHelper.ConvertToInt32(value, culture), culture);
            }
            catch (Exception)
            {
                return targetType.Default();
            }
        }
    }
}
