// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;

namespace SiliconStudio.Xenko.Rendering.ProceduralModels
{
    /// <summary>
    /// A Cylinder descriptor
    /// </summary>
    [DataContract("CylinderProceduralModel")]
    [Display("Cylinder")]
    public class CylinderProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the Cylinder descriptor class.
        /// </summary>
        public CylinderProceduralModel()
        {
            Height = 1.0f;
            Radius = 0.5f;
            Tessellation = 32;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        /// <userdoc>The height of the cylinder.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Height { get; set; }

        /// <summary>
        /// Gets or sets the radius of the base of the cylinder.
        /// </summary>
        /// <value>The radius.</value>
        /// <userdoc>The radius of the cylinder.</userdoc>
        [DataMember(20)]
        [DefaultValue(0.5f)]
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor.
        /// </summary>
        /// <value>The tessellation.</value>
        /// <userdoc>The tessellation of the cylinder. That is the number of polygons composing it.</userdoc>
        [DataMember(30)]
        [DefaultValue(32)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Cylinder.New(Height, Radius, Tessellation, UvScale.X, UvScale.Y);
        }
    }
}
