// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
{
    /// <summary>
    /// A sphere procedural model.
    /// </summary>
    [DataContract("GeoSphereProceduralModel")]
    [Display("GeoSphere")]
    public class GeoSphereProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoSphereProceduralModel"/> class.
        /// </summary>
        public GeoSphereProceduralModel()
        {
            Radius = 0.5f;
            Tessellation = 3;
        }

        /// <summary>
        /// Gets or sets the radius of this sphere.
        /// </summary>
        /// <value>The radius.</value>
        /// <userdoc>The radius of the geosphere.</userdoc>
        [DataMember(10)]
        [DefaultValue(0.5f)]
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor (default: 3.0)
        /// </summary>
        /// <value>The tessellation.</value>
        /// <userdoc>The tessellation of the geophere. That is the number of polygons composing it.</userdoc>
        [DataMember(20)]
        [DefaultValue(3)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.GeoSphere.New(Radius, Tessellation, UvScale.X, UvScale.Y);
        }
    }
}