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
    /// This converter will sum a given <see cref="Thickness"/> with a <see cref="Thickness"/> passed as parameter. You can use
    /// the <see cref="MarkupExtensions.ThicknessExtension"/> markup extension to easily pass one, with the following syntax: {sskk:Thickness (arguments)}. 
    /// </summary>
    [ValueConversion(typeof(Thickness), typeof(Thickness))]
    public class SumThickness : ValueConverterBase<SumThickness>
    {
        /// <inheritdoc/>
        [NotNull]
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness))
            {
                throw new ArgumentException("The value of the ConvertBack method of this converter must be a an instance of the Thickness structure.");
            }
            if (!(parameter is Thickness))
            {
                throw new ArgumentException("The parameter of the ConvertBack method of this converter must be a an instance of the Thickness structure.");
            }

            var sizeValue = (Thickness)value;
            var sizeParameter = (Thickness)parameter;
            var result = new Thickness(sizeValue.Left + sizeParameter.Left, sizeValue.Top + sizeParameter.Top, sizeValue.Right + sizeParameter.Right, sizeValue.Bottom + sizeParameter.Bottom);
            return result;
        }

        /// <inheritdoc/>
        [NotNull]
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness))
            {
                throw new ArgumentException("The value of the ConvertBack method of this converter must be a an instance of the Thickness structure.");
            }
            if (!(parameter is Thickness))
            {
                throw new ArgumentException("The parameter of the ConvertBack method of this converter must be a an instance of the Thickness structure.");
            }
            var sizeValue = (Thickness)value;
            var sizeParameter = (Thickness)parameter;

            var result = new Thickness(sizeValue.Left - sizeParameter.Left, sizeValue.Top - sizeParameter.Top, sizeValue.Right - sizeParameter.Right, sizeValue.Bottom - sizeParameter.Bottom);
            return result;
        }
    }
}