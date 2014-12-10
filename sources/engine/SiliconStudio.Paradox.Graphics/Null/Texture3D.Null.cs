// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class Texture3D
    {
        protected internal Texture3D(GraphicsDevice device, TextureDescription description3D, DataBox[] dataBoxes = null)
            : base(device, description3D)
        {
        }

        protected internal Texture3D(GraphicsDevice device, Texture3D texture)
            : base(device, texture.Description)
        {
        }

        public override Texture ToTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            throw new NotImplementedException();
        }
    }
}
 
#endif 
