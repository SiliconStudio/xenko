// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A RGBA channel selected when performing texture sampling.
    /// </summary>
    [DataContract("ColorChannel")]
    public enum ColorChannel
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
