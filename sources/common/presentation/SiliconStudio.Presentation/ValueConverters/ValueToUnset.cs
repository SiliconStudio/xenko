// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.Windows;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a specific value to a <see cref="DependencyProperty.UnsetValue"/>.
    /// </summary>
    public class ValueToUnset : OneWayValueConverter<ValueToUnset>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter) ? DependencyProperty.UnsetValue : value;
        }
    }
}
