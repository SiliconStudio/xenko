// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders
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
