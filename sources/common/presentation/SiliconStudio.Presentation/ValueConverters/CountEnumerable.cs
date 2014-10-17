// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will take an enumerable as input and return the number of items it contains.
    /// </summary>
    public class CountEnumerable : OneWayValueConverter<CountEnumerable>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;

            if (!(value is IEnumerable))
                throw new ArgumentException(@"The given value must implement IEnumerable", "value");

            var enumerable = (IEnumerable)value;
            return enumerable.Cast<object>().Count();
        }
    }
}
