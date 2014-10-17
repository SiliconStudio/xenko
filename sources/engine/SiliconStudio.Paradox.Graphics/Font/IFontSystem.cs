// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics.Font
{
    /// <summary>
    /// The interface to create and manage fonts.
    /// </summary>
    public interface IFontSystem
    {
        /// <summary>
        /// Create a new instance of a static font.
        /// </summary>
        /// <param name="data">The static font data from which to create the font.</param>
        /// <returns>The newly created static font</returns>
        SpriteFont NewStatic(StaticSpriteFontData data);

        /// <summary>
        /// Create a new instance of a dynamic font.
        /// </summary>
        /// <param name="data">The dynamic font data from which to create the font.</param>
        /// <returns>The newly created dynamic font</returns>
        SpriteFont NewDynamic(DynamicSpriteFontData data);
    }
}