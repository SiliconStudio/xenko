// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
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
            Normal = NormalDirection.UpY;
            Size = new Vector2(1.0f);
            Tessellation = new Int2(1);
            UVScales = new Vector2(1);
        }

        /// <summary>
        /// Gets or sets the size of the plane.
        /// </summary>
        /// <value>The size x.</value>
        /// <userdoc>The size of plane along the X/Y axis</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        [Display("Size")]
        public Vector2 Size { get; set; }

        /// <summary>
        /// Gets or sets the tessellation of the plane.
        /// </summary>
        /// <value>The tessellation x.</value>
        /// <userdoc>The tessellation of the plane along the X/Y axis. That is the number rectangles the plane is made of.</userdoc>
        [DataMember(20)]
        [DefaultValue(1)]
        [Display("Tessellation")]
        public Int2 Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the UV scales.
        /// </summary>
        /// <value>The UV scales</value>
        /// <userdoc>The scales to apply to the UV coordinates of the plane.</userdoc>
        [DataMember(30)]
        [DefaultValue(1.0f)]
        [Display("UV Scales")]
        public Vector2 UVScales { get; set; }


        [DataMember(40)]
        [DefaultValue(NormalDirection.UpZ)]
        [Display("Normal")]
        public NormalDirection Normal { get; set; }


        /// <summary>
        /// Gets or sets value indicating if a back face should be added.
        /// </summary>
        /// <userdoc>Check the this combo box to generate a back face to the plane</userdoc>
        [DataMember(50)]
        [DefaultValue(1.0f)]
        [Display("Back Face")]
        public bool GenerateBackFace { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Plane.New(Size.X, Size.Y, Tessellation.X, Tessellation.Y, UVScales.X, UVScales.Y, GenerateBackFace, false, Normal);
        }
    }
}
