// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public interface IRenderTargetCompositionQueriable
    {
        string GetShaderClass(Type semanticType);
        Texture[] TexturesComposition { get; }
        ShaderSourceCollection MixinCollection { get; }
    }
}