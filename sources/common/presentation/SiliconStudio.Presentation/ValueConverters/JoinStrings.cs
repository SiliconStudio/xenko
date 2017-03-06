// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This value converter will join an enumerable of strings with the separator given as parameter (or using a single space character as separator
    /// if the parameter is null).
    /// </summary>
    public class JoinStrings : ValueConverterBase<JoinStrings>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strings = (IEnumerable<string>)value;
            var separator = (string)parameter;
            return string.Join(separator ?? " ", strings);
        }

        /// <inheritdoc/>
        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)value;
            var separator = (string)parameter;
            return str.Split(new[] { separator ?? " "}, StringSplitOptions.None);
        }
    }
}
