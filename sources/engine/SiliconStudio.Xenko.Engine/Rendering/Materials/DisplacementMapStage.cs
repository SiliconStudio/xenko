// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Rendering.Materials
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