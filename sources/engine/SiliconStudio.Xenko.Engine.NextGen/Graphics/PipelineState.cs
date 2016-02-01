// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public class PipelineStateDescription
    {
        // TODO: Root Signature

        // Effect/Shader
        public EffectBytecode EffectBytecode;

        // Rendering States
        public BlendStateDescription BlendState;
        public uint SampleMask;
        public RasterizerState RasterizerState;
        public DepthStencilState DepthStencilState;

        public PrimitiveType PrimitiveType;
        public PixelFormat[] RenderTargetFormats;
        public PixelFormat DepthStencilFormat;
    }

    public class PipelineState
    {
    }
}