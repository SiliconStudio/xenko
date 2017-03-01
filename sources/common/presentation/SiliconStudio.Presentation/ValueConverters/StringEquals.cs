// Copyright (c) 2014-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter compares the given string with the string passed as parameter, and returns <c>true</c> if they are equal, <c>false</c> otherwise.
    /// </summary>
    public class StringEquals : OneWayValueConverter<StringEquals>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = string.Equals((string)value, (string)parameter);
            return result.Box();
        }
    }
}
