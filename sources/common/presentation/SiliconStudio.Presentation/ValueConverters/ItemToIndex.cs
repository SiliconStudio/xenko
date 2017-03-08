// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Core;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class ItemToIndex : ValueConverterBase<ItemToIndex>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = parameter as IList;
            if (collection != null)
            {
                // attempt generic reverse object lookup
                var res = collection.IndexOf(value);
                if (res != -1)
                    return res;
                // if we're here, it failed. Attempt #2 by using a normalizing (to doubles) conversion:
                var asDoubles = collection.ToListOfDoubles();
                if (asDoubles.IsNullOrEmpty())
                    return -1;   // there were no numeric types in this collection.
                Debug.Assert(asDoubles.SequenceEqual(asDoubles.OrderBy(d => d)));
                var search = asDoubles.BinarySearch((double)value);
                if (search < 0) // API : it returns a 1-complement of the index if an exact match is not found.
                    search = Math.Min(~search, collection.Count - 1);
                return search;
            }
            return -1;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = parameter as IList;
            if (collection == null)
                return null;

            var index = ConverterHelper.ConvertToInt32(value ?? -1, culture);
            if (index < 0 || index >= collection.Count)
                return null;

            return collection[index];
        }
    }
}
