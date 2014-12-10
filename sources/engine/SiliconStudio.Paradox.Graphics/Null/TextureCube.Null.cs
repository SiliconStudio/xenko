// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
namespace SiliconStudio.Paradox.Graphics
{
    public partial class TextureCube
    {
        internal TextureCube(GraphicsDevice device, TextureDescription description2D, params DataBox[] dataBoxes) : base(device, description2D, dataBoxes)
        {
        }

        internal TextureCube(GraphicsDevice device, TextureDescription description2D) : base(device, description2D, null)
        {
        }

        internal TextureCube(GraphicsDevice device, TextureCube texture)
            : base(device, texture)
        {
        }

        public override Texture ToTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            throw new System.NotImplementedException();
        }
    }
} 
#endif
