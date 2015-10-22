// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
{
    /// <summary>
    /// A Capsule descriptor
    /// </summary>
    [DataContract("CapsuleProceduralModel")]
    [Display("Capsule")]
    public class CapsuleProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the Capsule descriptor class.
        /// </summary>
        public CapsuleProceduralModel()
        {
            Length = 0.5f;
            Radius = 0.25f;
            Tessellation = 8;
        }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>The length.</value>
        /// <userdoc>The length of the capsule. That is the distance between the center of two extremity spheres.</userdoc>
        [DataMember(10)]
        [DefaultValue(0.5f)]
        public float Length { get; set; }

        /// <summary>
        /// Gets or sets the radius of the base of the Capsule.
        /// </summary>
        /// <value>The radius.</value>
        /// <userdoc>The radius of the capsule.</userdoc>
        [DataMember(20)]
        [DefaultValue(0.25f)]
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor.
        /// </summary>
        /// <userdoc>The tessellation of the capsule. That is the number of polygons composing it.</userdoc>
        [DataMember(30)]
        [DefaultValue(8)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Capsule.New(Length, Radius, Tessellation, UvScale.X, UvScale.Y);
        }
    }
}