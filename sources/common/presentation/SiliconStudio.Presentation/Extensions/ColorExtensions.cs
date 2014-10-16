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

        public static SystemColor ToSystemColor(this Color4 color)
        {
            var byteColor = new Color(color);
            return SystemColor.FromArgb(byteColor.A, byteColor.R, byteColor.G, byteColor.B);
        }
    }
}
