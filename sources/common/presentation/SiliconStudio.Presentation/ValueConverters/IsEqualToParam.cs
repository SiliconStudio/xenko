// Copyright (c) 2014-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert an object to a boolean. If the given value is equal (or reference-equal for non-value type) to the parameter, it will
    /// return <c>true</c>. Otherwise, it will return <c>false</c>.
    /// </summary>
    public class IsEqualToParam : OneWayValueConverter<IsEqualToParam>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return parameter == null;

            var useEquals = value.GetType().IsValueType || value is string;
            var result = useEquals ? Equals(value, parameter) : ReferenceEquals(value, parameter);
            return result.Box();
        }
    }
}
