// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
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
            Diameter = 1.0f;
            Thickness = 3.3f;
            Tessellation = 32;
        }

        /// <summary>
        /// Gets or sets the size of this Torus.
        /// </summary>
        /// <value>The diameter.</value>
        /// <userdoc>The major diameter of the torus.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Diameter { get; set; }

        /// <summary>
        /// Gets or sets the thickness.
        /// </summary>
        /// <value>The minor diameter of the torus. That is the diameter of the ring.</value>
        [DataMember(20)]
        [DefaultValue(3.3f)]
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
            return GeometricPrimitive.Torus.New(Diameter, Thickness, Tessellation);
        }
    }
}