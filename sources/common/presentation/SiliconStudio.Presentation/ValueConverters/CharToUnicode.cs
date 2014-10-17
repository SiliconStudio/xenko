// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

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
            var unicodeValue = System.Convert.ToInt32(value);
            return unicodeValue;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var charValue = System.Convert.ToChar(System.Convert.ToInt32(value));
                return charValue;
            }
            catch (Exception)
            {
                return default(char);
            }
        }
    }
}
