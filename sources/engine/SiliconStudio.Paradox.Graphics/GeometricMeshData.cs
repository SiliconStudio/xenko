// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A geometric data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GeometricMeshData<T> : ComponentBase where T : struct, IVertexWindable
    {
        public GeometricMeshData(T[] vertices, int[] indices, bool isLeftHanded, VertexDeclaration layout)
        {
            Vertices = vertices;
            Indices = indices;
            IsLeftHanded = isLeftHanded;
            Layout = layout;
        }

        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        /// <value>The vertices.</value>
        public T[] Vertices { get; set; }

        /// <summary>
        /// Gets or sets the indices.
        /// </summary>
        /// <value>The indices.</value>
        public int[] Indices { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is left handed.
        /// </summary>
        /// <value><c>true</c> if this instance is left handed; otherwise, <c>false</c>.</value>
        public bool IsLeftHanded { get; set; }

        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>The layout.</value>
        public VertexDeclaration Layout { get; set; }
    }
}