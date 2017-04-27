// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;

using SiliconStudio.Presentation.Core;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will format a CamelCase string by inserting spaces between words.
    /// </summary>
    public class CamelCaseTextConverter : OneWayValueConverter<CamelCaseTextConverter>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var strVal = value.ToString();
            return Utils.SplitCamelCase(strVal);
        }
    }
}
