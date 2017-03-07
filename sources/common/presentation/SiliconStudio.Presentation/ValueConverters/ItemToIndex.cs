// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Globalization;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class ItemToIndex : ValueConverterBase<ItemToIndex>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IList)parameter;
            return collection?.IndexOf(value) ?? -1;
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
