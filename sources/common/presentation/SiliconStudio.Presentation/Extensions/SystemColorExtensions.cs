// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Extensions
{
    using SystemColor = System.Windows.Media.Color;

    public static class SystemColorExtensions
    {
        public static SystemColor ToSystemColor(this ColorHSV color)
        {
            return ToSystemColor(color.ToColor());
        }

        public static SystemColor ToSystemColor(this Color color)
        {
            return SystemColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static SystemColor ToSystemColor(this Color4 color4)
        {
            var color = (Color)color4;
            return SystemColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static SystemColor ToSystemColor(this Color3 color3)
        {
            var color = (Color)color3;
            return SystemColor.FromArgb(255, color.R, color.G, color.B);
        }
    }
}
