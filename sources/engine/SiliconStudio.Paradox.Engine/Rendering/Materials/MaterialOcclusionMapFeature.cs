// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// An occlusion map for the occlusion material feature.
    /// </summary>
    [DataContract("MaterialOcclusionMapFeature")]
    [Display("Occlusion Map")]
    public class MaterialOcclusionMapFeature : IMaterialOcclusionFeature, IMaterialStreamProvider
    {
        private static readonly MaterialStreamDescriptor OcclusionStream = new MaterialStreamDescriptor("Occlusion", "matAmbientOcclusion", MaterialKeys.AmbientOcclusionValue.PropertyType);
        private static readonly MaterialStreamDescriptor CavityStream = new MaterialStreamDescriptor("Cavity", "matCavity", MaterialKeys.CavityValue.PropertyType);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialOcclusionMapFeature"/> class.
        /// </summary>
        public MaterialOcclusionMapFeature()
        {
            AmbientOcclusionMap = new ComputeTextureScalar();
            DirectLightingFactor = new ComputeFloat(0.0f);
            CavityMap = new ComputeTextureScalar();
            DiffuseCavity = new ComputeFloat(1.0f);
            SpecularCavity = new ComputeFloat(1.0f);
        }

        /// <summary>
        /// Gets or sets the occlusion map.
        /// </summary>
        /// <value>The occlusion map.</value>
        [Display("Occlusion Map")]
        [DataMember(10)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar AmbientOcclusionMap { get; set; }

        /// <summary>
        /// Gets or sets how much the occlusion map can influence direct lighting (default: 0).
        /// </summary>
        /// <value>The direct lighting factor.</value>
        [Display("Direct Lighting Influence")]
        [DataMember(15)]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar DirectLightingFactor { get; set; }

        /// <summary>
        /// Gets or sets the cavity map.
        /// </summary>
        /// <value>The cavity map.</value>
        [Display("Cavity Map")]
        [DataMember(20)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar CavityMap { get; set; }

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
            // Exclude ambient occlusion from uv-scale overrides
            var revertOverrides = new MaterialOverrides();
            revertOverrides.UVScale = 1.0f / context.CurrentOverrides.UVScale;

            context.PushOverrides(revertOverrides);
            context.SetStream(OcclusionStream.Stream, AmbientOcclusionMap, MaterialKeys.AmbientOcclusionMap, MaterialKeys.AmbientOcclusionValue, Color.White);
            context.PopOverrides();

            context.SetStream("matAmbientOcclusionDirectLightingFactor", DirectLightingFactor, null, MaterialKeys.AmbientOcclusionDirectLightingFactorValue);

            if (CavityMap != null)
            {
                context.SetStream(CavityStream.Stream, CavityMap, MaterialKeys.CavityMap, MaterialKeys.CavityValue, Color.White);
                context.SetStream("matCavityDiffuse", DiffuseCavity, null, MaterialKeys.CavityDiffuseValue);
                context.SetStream("matCavitySpecular", SpecularCavity, null, MaterialKeys.CavitySpecularValue);
            }
        }

        public IEnumerable<MaterialStreamDescriptor> GetStreams()
        {
            yield return OcclusionStream;
            yield return CavityStream;
        }
    }
}