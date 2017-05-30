// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;

namespace SiliconStudio.Xenko.Rendering.ProceduralModels
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
        /// <value>The size.</value>
        /// <userdoc>The size of the teapot.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Size { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor (default: 3.0)
        /// </summary>
        /// <value>The tessellation of the teapot. That is the number of polygons composing it.</value>
        [DataMember(20)]
        [DefaultValue(8)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Teapot.New(Size, Tessellation, UvScale.X, UvScale.Y);
        }
    }
}
