// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
