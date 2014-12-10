// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows.Media;

using SiliconStudio.Core.Mathematics;

using Color = SiliconStudio.Core.Mathematics.Color;

namespace SiliconStudio.Presentation.ValueConverters
{
    /// <summary>
    /// This converter will convert any known type of color value to the target type, if the conversion is possible. Otherwise, a <see cref="NotSupportedException"/> will be thrown.
    /// The currently input types supported are <see cref="SiliconStudio.Core.Mathematics.Color"/>, <see cref="Color3"/>, <see cref="Color4"/>.
    /// The currently output types supported are <see cref="System.Windows.Media.Color"/>, <see cref="SiliconStudio.Core.Mathematics.Color"/>, <see cref="Color3"/>, <see cref="Color4"/>, <see cref="string"/>, <see cref="System.Windows.Media.Brush"/>, <see cref="object"/>.
    /// </summary>
    public class ColorConverter : ValueConverterBase<ColorConverter>
    {
        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // TODO: string conversion is not correctly supported
            if (value is Color)
            {
                var color = (Color)value;
                if (targetType == typeof(System.Windows.Media.Color))
                    return ToMediaColor(color);
                if (targetType == typeof(Color))
                    return color;
                if (targetType == typeof(Color3))
                    return color.ToColor3();
                if (targetType == typeof(Color4))
                    return color.ToColor4();
                if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
                    return new SolidColorBrush(ToMediaColor(color));
                if (targetType == typeof(string))
                    return '#' + color.ToRgba().ToString("X8");
            }
            if (value is Color3)
            {
                var color = (Color3)value;
                if (targetType == typeof(System.Windows.Media.Color))
                    return ToMediaColor(color);
                if (targetType == typeof(Color))
                    return new Color(color.R, color.G, color.B);
                if (targetType == typeof(Color3))
                    return color;
                if (targetType == typeof(Color4))
                    return new Color4(color.R, color.G, color.B, 1.0f);
                if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
                    return new SolidColorBrush(ToMediaColor(color));
                if (targetType == typeof(string))
                    return '#' + color.ToRgb().ToString("X6");
            }
            if (value is Color4)
            {
                var color = (Color4)value;
                if (targetType == typeof(System.Windows.Media.Color))
                    return ToMediaColor(color);
                if (targetType == typeof(Color))
                    return new Color(color.R, color.G, color.B, color.A);
                if (targetType == typeof(Color3))
                    return new Color3(color.R, color.G, color.B);
                if (targetType == typeof(Color4))
                    return color;
                if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
                    return new SolidColorBrush(ToMediaColor(color));
                if (targetType == typeof(string))
                    return '#' + color.ToRgba().ToString("X8");
            }
            if (value is string)
            {
                var stringColor = value as string;
                int intValue;
                if (!stringColor.StartsWith("#") || !Int32.TryParse(stringColor.Substring(1), NumberStyles.HexNumber, null, out intValue))
                {
                    intValue = unchecked((int)0xFF000000);
                }
                if (targetType == typeof(Color))
                    return Color.FromRgba(intValue);
                if (targetType == typeof(Color3))
                    return new Color3(intValue);
                if (targetType == typeof(Color4))
                    return new Color4(intValue);
                if (targetType == typeof(System.Windows.Media.Color))
                {
                    return System.Windows.Media.Color.FromArgb(
                        (byte)((intValue >> 24) & 255),
                        (byte)(intValue & 255),
                        (byte)((intValue >> 8) & 255),
                        (byte)((intValue >> 16) & 255));
                }
                if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
                {
                    return new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                        (byte)((intValue >> 24) & 255),
                        (byte)(intValue & 255),
                        (byte)((intValue >> 8) & 255),
                        (byte)((intValue >> 16) & 255)));
                }
                if (targetType == typeof(string))
                    return stringColor;
            }
            throw new NotSupportedException("Requested conversion is not supported.");
        }

        /// <inheritdoc/>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(object))
                return value;

            var stringColor = value as string;
            if (stringColor != null)
            {
                int intValue;
                if (!stringColor.StartsWith("#") || !Int32.TryParse(stringColor.Substring(1), NumberStyles.HexNumber, null, out intValue))
                {
                    intValue = unchecked((int)0xFF000000);
                }
                if (targetType == typeof(Color))
                    return Color.FromRgba(intValue);
                if (targetType == typeof(Color3))
                    return new Color3(intValue);
                if (targetType == typeof(Color4))
                    return new Color4(intValue);
            }
            if (value is SolidColorBrush)
            {
                var brush = (SolidColorBrush)value;
                value = brush.Color;
            }
            if (value is System.Windows.Media.Color)
            {
                var wpfColor = (System.Windows.Media.Color)value;
                var color = new Color(wpfColor.R, wpfColor.G, wpfColor.B, wpfColor.A);
                if (targetType == typeof(Color))
                    return color;
                if (targetType == typeof(Color3))
                    return color.ToColor3();
                if (targetType == typeof(Color4))
                    return color.ToColor4();
            }
            if (value is Color)
            {
                var color = (Color)value;
                if (targetType == typeof(Color))
                    return color;
                if (targetType == typeof(Color3))
                    return color.ToColor3();
                if (targetType == typeof(Color4))
                    return color.ToColor4();
            }
            if (value is Color3)
            {
                var color = (Color3)value;
                if (targetType == typeof(Color))
                    return new Color(color.R, color.G, color.B);
                if (targetType == typeof(Color3))
                    return color;
                if (targetType == typeof(Color4))
                    return new Color4(1.0f, color.R, color.G, color.B);
            }
            if (value is Color4)
            {
                var color = (Color4)value;
                if (targetType == typeof(Color))
                    return new Color(color.R, color.G, color.B, color.A);
                if (targetType == typeof(Color3))
                    return new Color3(color.R, color.G, color.B);
                if (targetType == typeof(Color4))
                    return color;
            }
            throw new NotSupportedException("Requested conversion is not supported.");
        }

        private System.Windows.Media.Color ToMediaColor(Color4 color4)
        {
            var color = (Color)color4;
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private System.Windows.Media.Color ToMediaColor(Color3 color3)
        {
            var color = (Color)color3;
            return System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B);
        }
    }
}
