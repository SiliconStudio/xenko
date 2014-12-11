// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class Texture2D
    {
        protected Texture2D(GraphicsDevice device, TextureDescription description2D, DataBox[] dataBoxes = null)
            : base(device, description2D, dataBoxes)
        {
        }

        protected Texture2D(GraphicsDevice device, Texture2D texture)
            : base(device, texture)
        {
        }

        public override Texture ToTexture(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            throw new NotImplementedException();
        }

        public Texture2D CreateDepthTextureCompatible()
        {
            throw new NotImplementedException();
        }

        public DepthStencilBuffer ToDepthStencilBuffer(bool isReadOnly)
        {
            throw new NotImplementedException();
        }
    }
}
 
#endif
