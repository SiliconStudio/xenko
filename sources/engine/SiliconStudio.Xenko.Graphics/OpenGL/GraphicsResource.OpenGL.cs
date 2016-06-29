// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.ES30.TextureComponentCount;
#else
using OpenTK.Graphics.OpenGL;
using PixelFormatGl = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// GraphicsResource class
    /// </summary>
    public partial class GraphicsResource
    {
        // Shaader resource view (Texture or Texture Buffer)
        internal int TextureId;
        internal TextureTarget TextureTarget;
        internal PixelInternalFormat TextureInternalFormat;
        internal PixelFormatGl TextureFormat;
        internal PixelType TextureType;
        internal int TexturePixelSize;
    }
}
 
#endif
