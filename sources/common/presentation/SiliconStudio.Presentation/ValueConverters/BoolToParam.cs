// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows;

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
            bool result = System.Convert.ToBoolean(value);
            return result ? parameter : DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != DependencyProperty.UnsetValue;
        }
    }
}
