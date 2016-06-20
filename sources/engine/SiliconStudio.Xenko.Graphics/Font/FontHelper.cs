// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Graphics.Font
{
    public static class FontHelper
    {
        /// <summary>
        /// Converts a font size from points to pixels. Can't just let GDI+ do this for us, because we want identical results on every machine regardless of system DPI settings.
        /// </summary>
        /// <param name="points">The size in number of points</param>
        /// <returns>The size in number of pixels</returns>
        public static float PointsToPixels(float points)
        {
            return points * (96.0f / 72.0f);
        }

        /// <summary>
        /// Converts a font size from pixels to points. Can't just let GDI+ do this for us, because we want identical results on every machine regardless of system DPI settings.
        /// </summary>
        /// <param name="pixels">The size in number of pixels</param>
        /// <returns>The size in number of points</returns>
        public static float PixelsToPoints(float pixels)
        {
            return pixels * (72.0f / 96.0f);
        }

        /// <summary>
        /// Build the path of a font in the database given the name of the font family and the font style.
        /// </summary>
        /// <param name="fontName">Family name of the font</param>
        /// <param name="style">The style of the font</param>
        /// <remarks>This function does not indicate it the font exists or not in the database.</remarks>
        /// <returns>The absolute path of the font in the database</returns>
        public static string GetFontPath(string fontName, FontStyle style)
        {
            var styleName = "";
            if ((style & FontStyle.Bold) == FontStyle.Bold)
                styleName += " Bold";
            if ((style & FontStyle.Italic) == FontStyle.Italic)
                styleName += " Italic";

            return "fonts/" + fontName + styleName + ".ttf";
        }
    }
}
