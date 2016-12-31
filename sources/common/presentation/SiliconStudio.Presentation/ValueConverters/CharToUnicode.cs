// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a <see cref="char"/> value to the integer representation of its unicode value.
    /// <see cref="ConvertBack"/> is supported and will return the default char value if the given integer value can't be converted.
    /// </summary>
    public class CharToUnicode : ValueConverterBase<CharToUnicode>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var unicodeValue = ConverterHelper.ConvertToInt32(value, culture);
            return unicodeValue;
        }

        /// <inheritdoc/>
        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var charValue = ConverterHelper.ConvertToChar(ConverterHelper.ConvertToInt32(value, culture), culture);
                return charValue;
            }
            catch (Exception)
            {
                return default(char);
            }
        }
    }
}
