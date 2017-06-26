// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Represents a 2D grid of texels.
    /// </summary>
    public partial class Texture2D
    {
        protected internal Texture2D(GraphicsDevice device, TextureDescription description2D, DataBox[] dataBoxes = null, bool initialize = true) : base(device, description2D, TextureTarget.Texture2D, dataBoxes, initialize)
        {
        }

        protected internal Texture2D(GraphicsDevice device, Texture2D texture) : base(device, texture)
        {
        }

        public override Texture ToTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            // Exists since OpenGL 4.3
            if (viewType != ViewType.Full || arraySlice != 0 || mipMapSlice != 0)
                throw new NotImplementedException();

            return new Texture2D(GraphicsDevice, this);
        }

        public DepthStencilBuffer ToDepthStencilBuffer(bool isReadOnly)
        {
            return new DepthStencilBuffer(GraphicsDevice, this, isReadOnly);
        }

        public Texture2D CreateDepthTextureCompatible()
        {
            throw new NotImplementedException();
        }
    }
}
 
#endif 
