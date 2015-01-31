// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.ProceduralModels
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
            Size = 1.0f;
        }

        /// <summary>
        /// Gets or sets the size of the cube.
        /// </summary>
        /// <value>The size.</value>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Size { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Cube.New(Size);
        }
    }
}