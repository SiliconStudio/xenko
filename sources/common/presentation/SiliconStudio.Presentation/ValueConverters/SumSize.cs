// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will sum a given <see cref="Size"/> with a <see cref="Size"/> passed as parameter. You can use the <see cref="MarkupExtensions.SizeExtension"/>
    /// markup extension to easily pass one, with the following syntax: {sskk:Size (arguments)}. 
    /// </summary>
    [ValueConversion(typeof(Size), typeof(Size))]
    public class SumSize : ValueConverterBase<SumSize>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Size))
            {
                throw new ArgumentException("The value of the ConvertBack method of this converter must be a an instance of the Size structure.");
            }
            if (!(parameter is Size))
            {
                throw new ArgumentException("The parameter of the ConvertBack method of this converter must be a an instance of the Size structure.");
            }

            var sizeValue = (Size)value;
            var sizeParameter = (Size)parameter;
            var result = new Size(sizeValue.Width + sizeParameter.Width, sizeValue.Height + sizeParameter.Height);
            return result;
        }

        /// <inheritdoc/>
        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Size))
            {
                throw new ArgumentException("The value of the ConvertBack method of this converter must be a an instance of the Size structure.");
            }
            if (!(parameter is Size))
            {
                throw new ArgumentException("The parameter of the ConvertBack method of this converter must be a an instance of the Size structure.");
            }
            var sizeValue = (Size)value;
            var sizeParameter = (Size)parameter;

            var result = new Size(sizeValue.Width - sizeParameter.Width, sizeValue.Height - sizeParameter.Height);
            return result;
        }
    }
}
