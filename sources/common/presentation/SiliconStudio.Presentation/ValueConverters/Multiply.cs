// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will multiply a given numeric value by the numeric value given as parameter.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class Multiply : ValueConverterBase<Multiply>
    {
        /// <inheritdoc/>
        public override object Convert(object value, [NotNull] Type targetType, object parameter, CultureInfo culture)
        {
            double scalar;
            try
            {
                scalar = ConverterHelper.ConvertToDouble(value, culture);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("The value of this converter must be convertible to a double.", exception);
            }

            double param;
            try
            {
                param = ConverterHelper.ConvertToDouble(parameter, culture);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("The parameter of this converter must be convertible to a double.", exception);
            }

            return System.Convert.ChangeType(scalar * param, targetType);
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scalar;
            try
            {
                scalar = ConverterHelper.ConvertToDouble(value, culture);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("The value of this converter must be convertible to a double.", exception);
            }

            double param;
            try
            {
                param = ConverterHelper.ConvertToDouble(parameter, culture);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("The parameter of this converter must be convertible to a double.", exception);
            }

            if (Math.Abs(param) > double.Epsilon)
            {
                return System.Convert.ChangeType(scalar / param, targetType);
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
