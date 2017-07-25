// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class Take : OneWayValueConverter<Take>
    {
        /// <inheritdoc />
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value;

            var count = ConverterHelper.TryConvertToInt32(parameter, culture);
            return count.HasValue ? value.ToEnumerable<object>().Take(count.Value) : value;
        }
    }
}
