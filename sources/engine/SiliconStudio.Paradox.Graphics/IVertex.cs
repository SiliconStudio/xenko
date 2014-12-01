// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Graphics
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