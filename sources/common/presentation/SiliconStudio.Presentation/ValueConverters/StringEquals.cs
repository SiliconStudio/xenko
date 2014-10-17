// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

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
            return string.Equals((string)value, (string)parameter);
        }
    }
}
