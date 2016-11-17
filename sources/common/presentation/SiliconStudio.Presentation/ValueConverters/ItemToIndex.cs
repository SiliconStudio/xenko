// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class ItemToIndex : ValueConverterBase<ItemToIndex>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var collection = new List<double>((IEnumerable<double>)parameter);
                var search = collection?.BinarySearch((double)value) ?? -1;
                if (search < 0)  // weird API : it returns a 1-complement of the index if an exact match is not found.
                    search = Math.Min(~search, collection.Count - 1);
                return search;
            }
            catch (Exception) // case that objects are not double.
            {
                var collection = (IList)parameter;
                return collection?.IndexOf(value) ?? -1;
            }
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IList)parameter;
            if (collection == null)
                return null;

            var index = ConverterHelper.ConvertToInt32(value ?? -1, culture);
            if (index < 0 || index >= collection.Count)
                return null;

            return collection[index];
        }
    }
}
