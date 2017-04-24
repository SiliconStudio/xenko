// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Globalization;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a string value to a boolean value, returning <c>true</c> if the string is null or empty (or whitespace, see remarks), <c>false</c> otherwise.
    /// </summary>
    /// <remarks>
    /// If the boolean value <c>true</c> is passed as converter parameter, a whitespace string (<see cref="string.IsNullOrWhiteSpace(string)"/>)
    /// is also considered empty.
    /// </remarks>
    public class EmptyStringToBool : OneWayValueConverter<EmptyStringToBool>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = ConverterHelper.ConvertToString(value, culture);
            var result = parameter is bool && (bool)parameter
                ? string.IsNullOrWhiteSpace(stringValue)
                : string.IsNullOrEmpty(stringValue);
            return result.Box();
        }
    }
}
