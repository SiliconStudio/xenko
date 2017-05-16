// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;

namespace SiliconStudio.Xenko.Rendering.ProceduralModels
{
    /// <summary>
    /// The Torus Model.
    /// </summary>
    [DataContract("TorusProceduralModel")]
    [Display("Torus")]
    public class TorusProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TorusProceduralModel"/> class.
        /// </summary>
        public TorusProceduralModel()
        {
            Radius = 0.375f;
            Thickness = 0.125f;
            Tessellation = 32;
        }

        /// <summary>
        /// Gets or sets the size of this Torus.
        /// </summary>
        /// <value>The radius.</value>
        /// <userdoc>The major radius of the torus.</userdoc>
        [DataMember(10)]
        [DefaultValue(0.375f)]
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the thickness.
        /// </summary>
        /// <value>The minor radius of the torus. That is the radius of the ring.</value>
        [DataMember(20)]
        [DefaultValue(0.125f)]
        public float Thickness { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor (default: 3.0)
        /// </summary>
        /// <value>The tessellation.</value>
        /// <userdoc>The tessellation of the torus. That is the number of polygons composing it.</userdoc>
        [DataMember(30)]
        [DefaultValue(32)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Torus.New(Radius, Thickness, Tessellation, UvScale.X, UvScale.Y);
        }
    }
}
