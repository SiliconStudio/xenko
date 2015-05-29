// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class DepthStencilBuffer
    {
        internal DepthStencilBuffer(GraphicsDevice device, Texture2D depthTexture, bool isReadOnly) : base(device)
        {
            throw new NotImplementedException();
        }

        public static bool IsDepthStencilReadOnlySupported(GraphicsDevice device)
        {
            throw new NotImplementedException();
        }
    }
} 
#endif
