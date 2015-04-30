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
            Height = 1.0f;
            Radius = 1.0f;
            Tessellation = 16;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Height { get; set; }

        /// <summary>
        /// Gets or sets the diameter of the base of the Capsule.
        /// </summary>
        /// <value>The diameter.</value>
        [DataMember(20)]
        [DefaultValue(1.0f)]
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor.
        /// </summary>
        /// <value>The tessellation.</value>
        [DataMember(30)]
        [DefaultValue(16)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Capsule.New(Height, Radius, Tessellation);
        }
    }
}