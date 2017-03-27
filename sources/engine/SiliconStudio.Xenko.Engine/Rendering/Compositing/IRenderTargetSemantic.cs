// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Type type itself represents a semantic for a render target
    /// </summary>
    /// <remarks>Please implement stateless, so that objects can be recycled.</remarks>
    public interface IRenderTargetSemantic
    {
        string ShaderClass { get; }
    }

    public class ColorTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = null;
    }

    public class NormalTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = null;
    }

    public class VelocityTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = null;
    }

    public class OctaNormalSpecColorTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = "GBufferOutputNormalSpec";
    }

    public class EnvlightRoughnessTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = "GBufferOutputIblRoughness";
    }
}