// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Identifies the set of supported devices for the demo based on device capabilities.
    /// </summary>
    [DataContract("GraphicsProfile")]
    public enum GraphicsProfile
    {
        /// <summary>
        /// DirectX9 support (HLSL 3.0)
        /// </summary>
        Level_9_1 = 0x9100,

        /// <summary>
        /// DirectX9 support (HLSL 3.0)
        /// </summary>
        Level_9_2 = 0x9200,

        /// <summary>
        /// DirectX9 support (HLSL 3.0)
        /// </summary>
        Level_9_3 = 0x9300,
        
        /// <summary>
        /// DirectX10 support (HLSL 4.0, Geometry Shader)
        /// </summary>
        Level_10_0 = 0xA000,

        /// <summary>
        /// DirectX10.1 support (HLSL 4.1, Geometry Shader)
        /// </summary>
        Level_10_1 = 0xA100,

        /// <summary>
        /// DirectX11 support (HLSL 5.0, Compute Shaders, Domain/Hull Shaders)
        /// </summary>
        Level_11_0 = 0xB000,

        /// <summary>
        /// DirectX11.1 support (HLSL 5.0, Compute Shaders, Domain/Hull Shaders)
        /// </summary>
        Level_11_1 = 0xB100,

        /// <summary>
        /// DirectX11.2 support (HLSL 5.0, Compute Shaders, Domain/Hull Shaders)
        /// </summary>
        Level_11_2 = 0xB200,
    }
}
