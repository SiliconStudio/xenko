// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D 
// Copyright (c) 2011-2012 Silicon Studio

using System;

namespace SiliconStudio.Xenko.Graphics
{
    public partial struct Rational
    {
        /// <summary>
        /// Converts from SharpDX representation.
        /// </summary>
        /// <param name="rational">The rational.</param>
        /// <returns>Rational.</returns>
        internal static Rational FromSharpDX(SharpDX.DXGI.Rational rational)
        {
            return new Rational(rational.Numerator, rational.Denominator);
        }

        /// <summary>
        /// Converts to SharpDX representation.
        /// </summary>
        /// <returns>SharpDX.DXGI.Rational.</returns>
        internal SharpDX.DXGI.Rational ToSharpDX()
        {
            return new SharpDX.DXGI.Rational(Numerator, Denominator);
        }
    }
}
#endif
