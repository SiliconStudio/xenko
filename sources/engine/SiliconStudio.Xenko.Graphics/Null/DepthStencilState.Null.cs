// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class DepthStencilState
    {
        public DepthStencilState(GraphicsDevice device, DepthStencilStateDescription description) : base(device)
        {
            throw new NotImplementedException();
        }
    }
} 
#endif 
