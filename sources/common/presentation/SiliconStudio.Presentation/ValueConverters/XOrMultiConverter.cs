// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Linq;

namespace SiliconStudio.Presentation.ValueConverters
{
    public class XOrMultiConverter : OneWayMultiValueConverter<XOrMultiConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                throw new InvalidOperationException("This multi converter must be invoked with at least two elements");

            return values.Skip(1).Aggregate((bool)values[0], (current, value) => current ^ (bool)value);
        }
    }
}