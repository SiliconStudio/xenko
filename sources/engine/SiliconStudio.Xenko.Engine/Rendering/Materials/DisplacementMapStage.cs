// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Enumerates the different shader stages in which a displacement map can be applied.
    /// </summary>
    public enum DisplacementMapStage
    {
        /// <summary>
        /// The vertex shader
        /// </summary>
        Vertex = MaterialShaderStage.Vertex,

        /// <summary>
        /// The domain shader
        /// </summary>
        Domain = MaterialShaderStage.Domain,
    }
}
