// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_UI_SDL
namespace SiliconStudio.Xenko.Graphics.SDL
{
    /// <summary>
    /// Set of border style to mimic the Windows forms one. We actually only show <see cref="FixedSingle"/> and <see cref="Sizable"/> as the
    /// other values don't make sense in a purely SDL context.
    /// </summary>
    public enum FormBorderStyle
    {
        None = 0,
        FixedSingle = 1,
        Sizable = 4,
    }
}
#endif
