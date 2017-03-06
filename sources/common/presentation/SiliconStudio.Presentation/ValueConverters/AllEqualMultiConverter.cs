// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class AllEqualMultiConverter : OneWayMultiValueConverter<AllEqualMultiConverter>
    {
        [NotNull]
        public override object Convert([NotNull] object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                throw new InvalidOperationException("This multi converter must be invoked with at least two elements");

            var fallbackValue = parameter is bool && (bool)parameter;
            var first = values[0];
            var result = values.All(x => x == DependencyProperty.UnsetValue ? fallbackValue : Equals(x, first));
            return result.Box();
        }
    }
}
