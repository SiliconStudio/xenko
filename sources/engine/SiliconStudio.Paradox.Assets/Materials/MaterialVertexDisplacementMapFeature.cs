// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The displacement map for a surface material feature.
    /// </summary>
    [DataContract("MaterialVertexDisplacementMapFeature")]
    [Display("Vertex Displacement Map")]
    public class MaterialVertexDisplacementMapFeature : IMaterialDisplacementFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialVertexDisplacementMapFeature"/> class.
        /// </summary>
        public MaterialVertexDisplacementMapFeature() : this(new MaterialTextureComputeScalar())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialVertexDisplacementMapFeature"/> class.
        /// </summary>
        /// <param name="displacementMap">The displacement map.</param>
        public MaterialVertexDisplacementMapFeature(IMaterialComputeScalar displacementMap)
        {
            ScaleAndBias = true;
            DisplacementMap = displacementMap;
            Intensity = new MaterialFloatComputeNode(1.0f);
        }

        /// <summary>
        /// Gets or sets the displacement map.
        /// </summary>
        /// <value>The displacement map.</value>
        /// <userdoc>
        /// The displacement map.
        /// </userdoc>
        [DataMember(10)]
        [Display("Displacement Map")]
        [NotNull]
        public IMaterialComputeScalar DisplacementMap { get; set; }

        /// <summary>
        /// Gets or sets the displacement map.
        /// </summary>
        /// <value>The displacement map.</value>
        /// <userdoc>
        /// The displacement map.
        /// </userdoc>
        [DataMember(20)]
        [Display("Intensity")]
        [NotNull]
        public IMaterialComputeScalar Intensity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale by (2,2,2) and bias by (-1,-1,-1) the displacement map.
        /// </summary>
        /// <value><c>true</c> if scale and bias this displacement map; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Scale by (2,2,2) and bias by (-1,-1,-1) this displacement map.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(true)]
        [Display("Scale & Bias")]
        public bool ScaleAndBias { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            if (DisplacementMap != null)
            {
                // Workaround to inform compute colors that sampling is occuring from a vertex shader
                context.IsVertexStage = true;
                context.SetStream(MaterialShaderStage.Vertex, "matDisplacement", DisplacementMap, MaterialKeys.DisplacementMap, MaterialKeys.DisplacementValue);
                context.SetStream(MaterialShaderStage.Vertex, "matDisplacementIntensity", Intensity, MaterialKeys.DisplacementIntensityMap, MaterialKeys.DisplacementIntensityValue);
                context.IsVertexStage = false;

                context.AddVertexStreamModifier<MaterialVertexDisplacementMapFeature>(new ShaderClassSource("MaterialSurfaceVertexDisplacement", ScaleAndBias));
            }
        }
    }
}