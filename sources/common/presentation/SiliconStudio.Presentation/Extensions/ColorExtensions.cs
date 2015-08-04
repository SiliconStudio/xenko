// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Extensions
{
    using SystemColor = System.Windows.Media.Color;
    using SystemColors = System.Windows.Media.Colors;

    public static class ColorExtensions
    {
        public static SystemColor ToSystemColor(this ColorHSV color)
        {
            return ToSystemColor(color.ToColor());
        }

        public static SystemColor ToSystemColor(this Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static SystemColor ToSystemColor(this Color4 color4)
        {
            var color = (Color)color4;
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static SystemColor ToSystemColor(this Color3 color3)
        {
            var color = (Color)color3;
            return System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B);
        }

        public static Color4 ToColor4(this Color3 color3)
        {
            return new Color4(color3.R, color3.G, color3.B, 1.0f);
        }

        public static Color3 ToColor3(this Color4 color4)
        {
            return new Color3(color4.R, color4.G, color4.B);
        }

        public static string RgbToString(int value)
        {
            var r = (value & 0x000000FF);
            var g = (value & 0x0000FF00) >> 8;
            var b = (value & 0x00FF0000) >> 16;
            return string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        public static string RgbaToString(int value)
        {
            var r = (value & 0x000000FF);
            var g = (value & 0x0000FF00) >> 8;
            var b = (value & 0x00FF0000) >> 16;
            var a = (value & 0xFF000000) >> 24;
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", a, r, g, b);
        }
    }
}
