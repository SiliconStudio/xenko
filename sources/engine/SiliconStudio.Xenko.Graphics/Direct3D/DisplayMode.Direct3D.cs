// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D

using SharpDX.DXGI;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class DisplayMode
    {
        internal ModeDescription ToDescription()
        {
            return new ModeDescription(Width, Height, RefreshRate.ToSharpDX(), format: (SharpDX.DXGI.Format)Format);
        }

        internal static DisplayMode FromDescription(ModeDescription description)
        {
            return new DisplayMode((PixelFormat)description.Format, description.Width, description.Height, new Rational(description.RefreshRate.Numerator, description.RefreshRate.Denominator));
        }
    }
}
#endif
