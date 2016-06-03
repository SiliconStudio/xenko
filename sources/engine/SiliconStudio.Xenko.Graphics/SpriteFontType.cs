// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


namespace SiliconStudio.Xenko.Graphics
{
    public enum SpriteFontType
    {
        /// <summary>
        /// Static font which has fixed font size and is pre-compiled
        /// </summary>
        Static,

        /// <summary>
        /// Font which can change its font size dynamically and is compiled at runtime
        /// </summary>
        Dynamic,

        /// <summary>
        /// Signed Distance Field font which is pre-compiled but can still be scaled at runtime
        /// </summary>
        SDF,
    }
}
