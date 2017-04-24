// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
#else
using OpenTK.Graphics.OpenGL;
using PixelFormatGl = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Represents a 2D grid of texels.
    /// </summary>
    public partial class TextureCube
    {
        protected internal TextureCube(GraphicsDevice device, TextureDescription description2D, DataBox[] dataBoxes = null) : base(device, description2D, TextureTarget.TextureCubeMap)
        {
            throw new NotImplementedException();
        }

        protected internal TextureCube(GraphicsDevice device, TextureCube texture) : base(device, texture)
        {
            throw new NotImplementedException();
        }

        public override Texture ToTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            // Exists since OpenGL 4.3
            if (viewType != ViewType.Full || arraySlice != 0 || mipMapSlice != 0)
                throw new NotImplementedException();

            return new TextureCube(GraphicsDevice, this);
        }
    }
}
 
#endif 
