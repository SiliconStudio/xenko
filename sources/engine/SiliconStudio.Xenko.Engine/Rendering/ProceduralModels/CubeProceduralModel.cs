// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;

namespace SiliconStudio.Xenko.Rendering.ProceduralModels
{
    /// <summary>
    /// A cube procedural model
    /// </summary>
    [DataContract("CubeProceduralModel")]
    [Display("Cube")]
    public class CubeProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CubeProceduralModel"/> class.
        /// </summary>
        public CubeProceduralModel()
        {
            Size = Vector3.One;
        }

        /// <summary>
        /// Gets or sets the size of the cube.
        /// </summary>
        /// <value>The size.</value>
        /// <userdoc>The size of the cube along the Ox, Oy and Oz axis.</userdoc>
        [DataMember(10)]
        public Vector3 Size { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Cube.New(Size, UvScale.X, UvScale.Y);
        }
    }
}
