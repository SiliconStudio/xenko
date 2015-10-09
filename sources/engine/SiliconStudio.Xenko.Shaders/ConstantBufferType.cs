// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Describes the type of constant buffer.
    /// </summary>
    [DataContract]
    public enum ConstantBufferType
    {
        /// <summary>
        /// An unknown buffer.
        /// </summary>
        Unknown,

        /// <summary>
        /// A standard constant buffer
        /// </summary>
        ConstantBuffer,

        /// <summary>
        /// A texture buffer
        /// </summary>
        TextureBuffer,
    }
}