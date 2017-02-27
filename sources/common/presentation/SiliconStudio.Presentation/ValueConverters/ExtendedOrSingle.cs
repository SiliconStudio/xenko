// Copyright (c) 2014-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows.Controls;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert a boolean value to a <see cref="SelectionMode"/> value, where <c>false</c> translates to <see cref="SelectionMode.Single"/>
    /// and <c>true></c> translates to <see cref="SelectionMode.Extended"/>.
    /// <see cref="ConvertBack"/> is supported.
    /// </summary>
    /// <remarks>If the boolean value <c>false</c> is passed as converter parameter, the visibility is inverted.</remarks>
    public class ExtendedOrSingle : ValueConverterBase<ExtendedOrSingle>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = ConverterHelper.ConvertToBoolean(value, culture);
            if (parameter as bool? == false)
            {
                result = !result;
            }
            return result ? SelectionMode.Extended : SelectionMode.Single;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectionMode = (SelectionMode)value;
            var result = selectionMode == SelectionMode.Extended;
            if (parameter as bool? == false)
            {
                result = !result;
            }
            return result.Box();
        }
    }

}
