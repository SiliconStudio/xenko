// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
