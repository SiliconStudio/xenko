// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
{
    /// <summary>
    /// A teapot procedural model.
    /// </summary>
    [DataContract("TeapotProceduralModel")]
    [Display("Teapot")]
    public class TeapotProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeapotProceduralModel"/> class.
        /// </summary>
        public TeapotProceduralModel()
        {
            Size = 1.0f;
            Tessellation = 8;
        }

        /// <summary>
        /// Gets or sets the size of this teapot.
        /// </summary>
        /// <value>The diameter.</value>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Size { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor (default: 3.0)
        /// </summary>
        /// <value>The tessellation.</value>
        [DataMember(20)]
        [DefaultValue(8)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Teapot.New(Size, Tessellation);
        }
    }
}