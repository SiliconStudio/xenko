// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will trim the string representation of an object to the given number of characters, adding "..." at the end of the resulting string.
    /// The number of character must be passed via the converter parameter.
    /// </summary>
    /// <remarks>If the parameter is a negative number, its absolute value will be used and no trailing "..." will be added.</remarks>
    public class TrimString : OneWayValueConverter<TrimString>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // We get the length first to always throw an exception if the parameter in incorrect
            int length;
            try
            {
                length = ConverterHelper.ConvertToInt32(parameter, culture);
                if (length == 0) throw new Exception();
            }
            catch (Exception)
            {
                throw new FormatException("The parameter must be convertible to a non-null integer.");
            }

            if (value == null)
                return null;

            var addEllipsis = length >= 0;
            length = Math.Abs(length);
            var str = value.ToString();

            return str.Length > length ? str.Substring(0, length) + (addEllipsis ? "..." : "") : str;
        }
    }
}
