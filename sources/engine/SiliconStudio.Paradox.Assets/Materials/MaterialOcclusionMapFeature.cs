// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// An occlusion map for the occlusion material feature.
    /// </summary>
    [DataContract("MaterialOcclusionMapFeature")]
    [Display("Occlusion Map")]
    public class MaterialOcclusionMapFeature : IMaterialOcclusionFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialOcclusionMapFeature"/> class.
        /// </summary>
        public MaterialOcclusionMapFeature()
        {
            AmbientOcclusionMap = new ComputeTextureColor();
            CavityMap = new ComputeTextureColor();
            DiffuseCavity = new ComputeFloat(1.0f);
            SpecularCavity = new ComputeFloat(1.0f);
        }

        /// <summary>
        /// Gets or sets the occlusion map.
        /// </summary>
        /// <value>The occlusion map.</value>
        [Display("Occlusion Map")]
        [DataMember(10)]
        public IComputeColor AmbientOcclusionMap { get; set; }

        /// <summary>
        /// Gets or sets the cavity map.
        /// </summary>
        /// <value>The cavity map.</value>
        [Display("Cavity Map")]
        [DataMember(20)]
        public IComputeColor CavityMap { get; set; }

        /// <summary>
        /// Gets or sets the diffuse cavity influence.
        /// </summary>
        /// <value>The diffuse cavity.</value>
        [Display("Diffuse Cavity")]
        [DataMember(30)]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar DiffuseCavity { get; set; }

        /// <summary>
        /// Gets or sets the specular cavity.
        /// </summary>
        /// <value>The specular cavity.</value>
        [Display("Specular Cavity")]
        [DataMember(40)]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar SpecularCavity { get; set; }


        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matAmbientOcclusion", AmbientOcclusionMap, MaterialKeys.AmbientOcclusionMap, MaterialKeys.AmbientOcclusionValue);

            if (CavityMap != null)
            {
                context.SetStream("matCavity", CavityMap, MaterialKeys.CavityMap, MaterialKeys.CavityValue);
                context.SetStream("matCavityDiffuse", DiffuseCavity, null, MaterialKeys.CavityDiffuseValue);
                context.SetStream("matCavitySpecular", SpecularCavity, null, MaterialKeys.CavitySpecularValue);
            }
        }
    }
}