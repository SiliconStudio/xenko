// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Graphics.ProceduralModels
{
    /// <summary>
    /// The geometric descriptor for a plane.
    /// </summary>
    [DataContract("PlaneProceduralModel")]
    [Display("Plane")]
    public class PlaneProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of geometric descriptor for a plane.
        /// </summary>
        public PlaneProceduralModel()
        {
            SizeX = 100.0f;
            SizeY = 100.0f;
            TesselationX = 1;
            TesselationY = 1;
            ScaleU = 1;
            ScaleV = 1;
        }

        /// <summary>
        /// Gets or sets the size x.
        /// </summary>
        /// <value>The size x.</value>
        [DataMember(10)]
        [DefaultValue(100.0f)]
        public float SizeX { get; set; }

        /// <summary>
        /// Gets or sets the size y.
        /// </summary>
        /// <value>The size y.</value>
        [DataMember(20)]
        [DefaultValue(100.0f)]
        public float SizeY { get; set; }

        /// <summary>
        /// Gets or sets the tesselation x.
        /// </summary>
        /// <value>The tesselation x.</value>
        [DataMember(30)]
        [DefaultValue(1)]
        public int TesselationX { get; set; }

        /// <summary>
        /// Gets or sets the tesselation y.
        /// </summary>
        /// <value>The tesselation y.</value>
        [DataMember(40)]
        [DefaultValue(1)]
        public int TesselationY { get; set; }

        /// <summary>
        /// Gets or sets the scale u.
        /// </summary>
        /// <value>The scale u.</value>
        [DataMember(50)]
        [DefaultValue(1.0f)]
        public float ScaleU { get; set; }

        /// <summary>
        /// Gets or sets the scale v.
        /// </summary>
        /// <value>The scale v.</value>
        [DataMember(60)]
        [DefaultValue(1.0f)]
        public float ScaleV { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Plane.New(SizeX, SizeY, TesselationX, TesselationY, false, new Vector2(ScaleU, ScaleV));
        }
    }
}