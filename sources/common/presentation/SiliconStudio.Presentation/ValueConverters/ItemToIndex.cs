using System;
using System.Collections;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class ItemToIndex : ValueConverterBase<ItemToIndex>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IList)parameter;
            return collection != null ? collection.IndexOf(value) : -1;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IList)parameter;
            if (collection == null)
                return null;

            var index = (int)System.Convert.ChangeType(value ?? -1, typeof(int));
            if (index < 0 || index >= collection.Count)
                return null;

            return collection[index];
        }
    }
}