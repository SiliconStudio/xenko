// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a boolean value to a <see cref="Visibility"/> value, where false translates to <see cref="Visibility.Collapsed"/>.
    /// <see cref="ConvertBack"/> is supported.
    /// </summary>
    /// <remarks>If the boolean value <c>false</c> is passed as converter parameter, the visibility is inverted.</remarks>
    /// <seealso cref="VisibleOrHidden"/>
    public class VisibleOrCollapsed : ValueConverterBase<VisibleOrCollapsed>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = ConverterHelper.ConvertToBoolean(value, culture);
            if (parameter is bool && (bool)parameter == false)
            {
                result = !result;
            }
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc/>
        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            if (parameter is bool && (bool)parameter == false)
            {
                return visibility != Visibility.Visible;
            }
            return visibility == Visibility.Visible;
        }
    }
}
