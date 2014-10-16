// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.Effects.Data
{
    /*
    /// <summary>
    /// Draw data.
    /// It includes vertex buffers, index buffer and draw call information (primitive type, etc...).
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<MeshDrawData>))]
    [DataContract]
    public class MeshDrawData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshDrawData"/> class.
        /// </summary>
        public MeshDrawData()
        {
            VertexBuffers = new List<VertexBufferBindingData>();
        }

        /// <summary>
        /// Gets or sets the vertex buffers.
        /// </summary>
        /// <value>
        /// The vertex buffers.
        /// </value>
        public List<VertexBufferBindingData> VertexBuffers;

        /// <summary>
        /// Gets or sets the index buffer.
        /// </summary>
        /// <value>
        /// The index buffer.
        /// </value>
        public IndexBufferBindingData IndexBuffer;

        /// <summary>
        /// Gets or sets the primitive type.
        /// </summary>
        /// <value>
        /// The primitive type.
        /// </value>
        public PrimitiveType PrimitiveType;

        /// <summary>
        /// Gets or sets the number of items to draw (either vertex or indices count, depending if an index buffer is present).
        /// </summary>
        /// <value>
        /// The draw count.
        /// </value>
        public int DrawCount;

        public int StartLocation;

        public MeshDrawData Clone()
        {
            return (MeshDrawData)MemberwiseClone();
        }

        protected void ForceGenericInstantiation()
        {
            // AOT helper:

            // Force generic instantiation of ListDataConverter because VertexBufferBinding is a struct (used with VertexBuffers)
            typeof(ListDataConverter<List<VertexBufferBindingData>, VertexBufferBinding[], VertexBufferBindingData, VertexBufferBinding>).ToString();
        }
    }
     * */
}