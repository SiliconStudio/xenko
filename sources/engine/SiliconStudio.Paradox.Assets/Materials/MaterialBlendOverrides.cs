// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Material overrides used in a <see cref="MaterialBlendLayer"/>
    /// </summary>
    [DataContract("MaterialBlendOverrides")]
    [Display("Material Overrides")]
    public class MaterialBlendOverrides
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendOverrides"/> class.
        /// </summary>
        public MaterialBlendOverrides()
        {
            SurfaceContribution = 1.0f;
            MicroSurfaceContribution = 1.0f;
            DiffuseContribution = 1.0f;
            SpecularContribution = 1.0f;
            OcclusionContribution = 1.0f;
            OffsetU = 0.0f;
            OffsetV = 0.0f;
            ScaleU = 1.0f;
            ScaleV = 1.0f;
        }

        /// <summary>
        /// Gets or sets the surface contribution.
        /// </summary>
        /// <value>The surface contribution.</value>
        [Display("Surface Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(10)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public float SurfaceContribution { get; set; }

        /// <summary>
        /// Gets or sets the micro surface contribution.
        /// </summary>
        /// <value>The micro surface contribution.</value>
        [Display("MicroSurface Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(20)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public float MicroSurfaceContribution { get; set; }

        /// <summary>
        /// Gets or sets the diffuse contribution.
        /// </summary>
        /// <value>The diffuse contribution.</value>
        [Display("Diffuse Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(30)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public float DiffuseContribution { get; set; }

        /// <summary>
        /// Gets or sets the specular contribution.
        /// </summary>
        /// <value>The specular contribution.</value>
        [Display("Specular Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(40)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public float SpecularContribution { get; set; }

        /// <summary>
        /// Gets or sets the occlusion contribution.
        /// </summary>
        /// <value>The occlusion contribution.</value>
        [Display("Occlusion Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(50)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public float OcclusionContribution { get; set; }

        // TODO: Use Vector2 for uv Offset and uv Scales (Check how to integrate with range attribute)

        /// <summary>
        /// Gets or sets the offset u.
        /// </summary>
        /// <value>The offset u.</value>
        [DefaultValue(0.0f)]
        [DataMember(60)]
        public float OffsetU { get; set; }

        /// <summary>
        /// Gets or sets the offset v.
        /// </summary>
        /// <value>The offset v.</value>
        [DefaultValue(0.0f)]
        [DataMember(70)]
        public float OffsetV { get; set; }

        /// <summary>
        /// Gets or sets the scale u.
        /// </summary>
        /// <value>The scale u.</value>
        [DefaultValue(1.0f)]
        [DataMember(80)]
        public float ScaleU { get; set; }

        /// <summary>
        /// Gets or sets the scale v.
        /// </summary>
        /// <value>The scale v.</value>
        [DefaultValue(1.0f)]
        [DataMember(90)]
        public float ScaleV { get; set; }
    }
}