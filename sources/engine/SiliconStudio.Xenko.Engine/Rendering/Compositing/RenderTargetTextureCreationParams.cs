// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public struct RenderTargetTextureCreationParams
    {
        public PixelFormat PixelFormat { get; set; }
        public TextureFlags TextureFlags { get; set; }
        public MSAALevel MSAALevel { get; set; }
    }
}