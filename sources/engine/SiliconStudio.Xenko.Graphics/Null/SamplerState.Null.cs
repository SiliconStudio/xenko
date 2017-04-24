// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class SamplerState
    {
        private SamplerState(GraphicsDevice graphicsDevice, SamplerStateDescription samplerStateDescription)
        {
            throw new NotImplementedException();
        }
    }
} 
#endif 
