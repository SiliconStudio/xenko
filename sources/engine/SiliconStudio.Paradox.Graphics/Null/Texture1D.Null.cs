// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
namespace SiliconStudio.Paradox.Graphics
{
    public partial class Texture1D
    {
        protected internal Texture1D(GraphicsDevice device, TextureDescription description1D, DataBox[] dataBox = null)
            : base(device, description1D)
        {
        }

        protected internal Texture1D(GraphicsDevice device, Texture1D texture)
            : base(device, texture.Description)
        {
        }

        public override Texture ToTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            throw new System.NotImplementedException();
        }
    }
}
 
#endif 
