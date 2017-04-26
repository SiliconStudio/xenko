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
    public partial class Texture3D
    {
        protected internal Texture3D(GraphicsDevice device, TextureDescription description3D, DataBox[] dataBoxes = null) : base(device, description3D, ViewType.Full, 0, 0)
        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            Target = TextureTarget.Texture3D;
#endif
        }

        protected internal Texture3D(GraphicsDevice device, Texture3D texture) : base(device, texture, ViewType.Full, 0, 0)
        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            Target = TextureTarget.Texture3D;
#endif
        }

        public override Texture ToTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            // Exists since OpenGL 4.3
            if (viewType != ViewType.Full || arraySlice != 0 || mipMapSlice != 0)
                throw new NotImplementedException();

            return new Texture3D(GraphicsDevice, this);
        }
    }
}
 
#endif 
