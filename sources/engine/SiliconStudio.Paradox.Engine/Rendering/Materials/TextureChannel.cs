// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// A RGBA channel selected when performing texture sampling.
    /// </summary>
    [DataContract("TextureChannel")]
    public enum TextureChannel
    {
        /// <summary>
        /// The sampled color is returned as a float4(R, R, R, R)
        /// </summary>
        R,

        /// <summary>
        /// The sampled color is returned as a float4(G, G, G, G)
        /// </summary>
        G,

        /// <summary>
        /// The sampled color is returned as a float4(B, B, B, B)
        /// </summary>
        B,

        /// <summary>
        /// The sampled color is returned as a float4(A, A, A, A)
        /// </summary>
        A,
    }
}