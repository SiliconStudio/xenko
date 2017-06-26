// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// The base interface for all the vertex data structure.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Gets the layout of the vertex.
        /// </summary>
        /// <returns></returns>
        VertexDeclaration GetLayout();

        /// <summary>
        /// Flip the vertex winding.
        /// </summary>
        void FlipWinding();
    }
}
