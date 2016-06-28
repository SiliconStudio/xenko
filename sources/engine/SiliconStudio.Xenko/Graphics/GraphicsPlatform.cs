// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// The graphics platform.
    /// </summary>
    [DataContract("GraphicsPlatform")]
    public enum GraphicsPlatform
    {
        /// <summary>
        /// HLSL Direct3D Shader.
        /// </summary>
        Direct3D11,

        /// <summary>
        /// HLSL Direct3D Shader.
        /// </summary>
        Direct3D12,

        /// <summary>
        /// GLSL OpenGL Shader.
        /// </summary>
        OpenGL,

        /// <summary>
        /// GLSL OpenGL ES Shader.
        /// </summary>
        OpenGLES,
		
        /// <summary>
        /// GLSL/SPIR-V Shader.
        /// </summary>
        Vulkan
    }
}