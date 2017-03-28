// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// The usage of a render target
    /// </summary>
    public interface IRenderTargetSemantic
    {
        /// <summary>
        /// The shader class deriving from ComputeColor that is used as a composition to output to the render target
        /// </summary>
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

    public class OctahedronNormalSpecularColorTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = "GBufferOutputNormalSpec";
    }

    public class EnvironmentLightRoughnessTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = "GBufferOutputIblRoughness";
    }
}