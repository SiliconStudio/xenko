// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Linq;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class MultiplyMultiConverter : OneWayMultiValueConverter<MultiplyMultiConverter>
    {
        [NotNull]
        public override object Convert([NotNull] object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                throw new InvalidOperationException("This multi converter must be invoked with at least two elements");

            var result = 1.0;
            try
            {
                result = values.Select(x => ConverterHelper.ConvertToDouble(x, culture)).Aggregate(result, (current, next) => current * next);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("The values of this converter must be convertible to a double.", exception);
            }

            return result;
        }
    }
}
