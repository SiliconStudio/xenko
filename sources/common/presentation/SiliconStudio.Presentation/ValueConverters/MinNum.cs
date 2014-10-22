// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will return the minimal result between the converter value and the converter parameter.
    /// </summary>
    public class MinNum : OneWayValueConverter<MinNum>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValue = (double)System.Convert.ChangeType(value ?? 0, typeof(double));
            var doubleParameter = (double)System.Convert.ChangeType(parameter ?? 0, typeof(double));
            return System.Convert.ChangeType(Math.Min(doubleValue, doubleParameter), value != null ? value.GetType() : targetType);
        }
    }
}