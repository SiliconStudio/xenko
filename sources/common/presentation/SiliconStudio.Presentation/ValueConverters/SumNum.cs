// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will sum a given numeric value with a numeric value passed as parameter. You can use the <see cref="MarkupExtensions.DoubleExtension"/>
    /// markup extension to easily pass a double value as parameter, with the following syntax: {sskk:Double (argument)}. 
    /// </summary>
    public class SumNum : ValueConverterBase<SumNum>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValue = (double)System.Convert.ChangeType(value ?? 0, typeof(double));
            var doubleParameter = (double)System.Convert.ChangeType(parameter ?? 0, typeof(double));
            var result = doubleValue + doubleParameter;
            return System.Convert.ChangeType(result, value?.GetType() ?? targetType);
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValue = (double)System.Convert.ChangeType(value ?? 0, typeof(double));
            var doubleParameter = (double)System.Convert.ChangeType(parameter ?? 0, typeof(double));
            var result = doubleValue - doubleParameter;
            return System.Convert.ChangeType(result, targetType);
        }
    }
}