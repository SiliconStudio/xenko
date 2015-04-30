// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
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
            Diameter = 1.0f;
            Tessellation = 32;
            ScaleUV = 1;
        }

        //float height = 1.0f, float diameter = 1.0f, int tessellation = 32, float textureTiling = 1,

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Height { get; set; }

        /// <summary>
        /// Gets or sets the diameter of the base of the cylinder.
        /// </summary>
        /// <value>The diameter.</value>
        [DataMember(20)]
        [DefaultValue(1.0f)]
        public float Diameter { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor.
        /// </summary>
        /// <value>The tessellation.</value>
        [DataMember(30)]
        [DefaultValue(32)]
        public int Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the scale to apply on texcoord uv.
        /// </summary>
        /// <value>The scale uv.</value>
        [DataMember(40)]
        [DefaultValue(1.0f)]
        public float ScaleUV { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Cylinder.New(Height, Diameter, Tessellation, ScaleUV);
        }
    }
}