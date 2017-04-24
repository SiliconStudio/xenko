// Copyright (c) 2015-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
