// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsAdapterFactory
    {
        private static void InitializeInternal()
        {
            defaultAdapter = new GraphicsAdapter();
            adapters = new [] { defaultAdapter };
        }
    }
} 
#endif
