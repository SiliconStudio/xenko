// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
        internal bool DiscardNextMap; // Used to internally force a WriteDiscard (to force a rename) with the GraphicsResourceAllocator

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
