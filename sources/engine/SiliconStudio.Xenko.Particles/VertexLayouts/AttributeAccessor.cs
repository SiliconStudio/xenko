// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    /// <summary>
    /// <see cref="AttributeAccessor"/> is use to access and modify a particle vertex attribute.
    /// </summary>
    public struct AttributeAccessor
    {
        /// <summary>
        /// Offset of the attribute from the beginning of the vertex position
        /// </summary>
        public int Offset;

        /// <summary>
        /// Size of the attribute field
        /// </summary>
        public int Size;
    }
}
