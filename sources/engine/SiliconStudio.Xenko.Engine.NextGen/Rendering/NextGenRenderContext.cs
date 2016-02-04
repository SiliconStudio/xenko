// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public class NextGenRenderContext
    {
        public readonly GraphicsDevice GraphicsDevice;

        public NextGenRenderContext(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}