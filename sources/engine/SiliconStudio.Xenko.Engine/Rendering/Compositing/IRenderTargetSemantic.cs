// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Shaders;

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
        ShaderSource ShaderClass { get; }
    }

    public class ColorTargetSemantic : IRenderTargetSemantic
    {
        public ShaderSource ShaderClass { get; } = null;
    }

    public class NormalTargetSemantic : IRenderTargetSemantic
    {
        public ShaderSource ShaderClass { get; } = new ShaderClassSource("GBufferOutputNormals");
    }

    public class SpecularColorRoughnessTargetSemantic : IRenderTargetSemantic
    {
        public ShaderSource ShaderClass { get; } = new ShaderClassSource("GBufferOutputSpecularColorRoughness");
    }

    public class VelocityTargetSemantic : IRenderTargetSemantic
    {
        public ShaderSource ShaderClass { get; } = null;
    }

    public class OctahedronNormalSpecularColorTargetSemantic : IRenderTargetSemantic
    {
        public ShaderSource ShaderClass { get; } = new ShaderClassSource("GBufferOutputNormalSpec");
    }

    public class EnvironmentLightRoughnessTargetSemantic : IRenderTargetSemantic
    {
        public ShaderSource ShaderClass { get; } = new ShaderClassSource("GBufferOutputIblRoughness");
    }
}