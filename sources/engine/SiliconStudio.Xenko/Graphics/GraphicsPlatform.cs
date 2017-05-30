// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
        /// The Null Shader.
        /// </summary>
        Null,

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
