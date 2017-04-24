// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.Windows;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a boolean to the object given in parameter if its true,
    /// and to <see cref="DependencyProperty.UnsetValue"/> if it's false.
    /// <see cref="ConvertBack"/> is supported and will return whether the given object is different from
    /// <see cref="DependencyProperty.UnsetValue"/>.
    /// </summary>
    public class BoolToParam : ValueConverterBase<BoolToParam>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = ConverterHelper.ConvertToBoolean(value, culture);
            return result ? parameter : DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value != DependencyProperty.UnsetValue;
            return result.Box();
        }
    }
}
