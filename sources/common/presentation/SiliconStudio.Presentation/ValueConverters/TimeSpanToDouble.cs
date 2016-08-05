// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This value converter will convert a TimeSpan to double by using the <see cref="TimeSpan.TotalSeconds"/> property.
    /// </summary>
    public class TimeSpanToDouble : ValueConverterBase<TimeSpanToDouble>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;
            return timeSpan.TotalSeconds;
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var seconds = ConverterHelper.ConvertToDouble(value, culture);
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
