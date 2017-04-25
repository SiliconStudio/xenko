// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will take an enumerable as input and return the number of items it contains.
    /// </summary>
    public class CountEnumerable : OneWayValueConverter<CountEnumerable>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;

            var enumerable = value as IEnumerable;
            if (enumerable == null)
                throw new ArgumentException(@"The given value must implement IEnumerable", nameof(value));

            return (value as ICollection)?.Count ?? enumerable.Cast<object>().Count();
        }
    }
}
