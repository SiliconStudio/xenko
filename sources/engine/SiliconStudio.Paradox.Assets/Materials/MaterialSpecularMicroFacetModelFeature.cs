// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The microfacet specular shading model.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetModelFeature")]
    [Display("Microfacet Model")]
    public class MaterialSpecularMicrofacetModelFeature : IMaterialDiffuseModelFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialSpecularMicrofacetModelFeature"/> class.
        /// </summary>
        public MaterialSpecularMicrofacetModelFeature()
        {
            // Defaults
            Fresnel = new MaterialSpecularMicrofacetFresnelSchlick();
            GeometricShadowing = new MaterialSpecularMicrofacetGeometricShadowingSmithGGX();
            NormalDistribution = new MaterialSpecularMicrofacetNormalDistributionGGX();
        }

        public bool IsLightDependent
        {
            get
            {
                return true;
            }
        }

        [DataMember(10)]
        [Display("Fresnel")]
        public IMaterialSpecularMicrofacetFresnelFunction Fresnel { get; set; }

        [DataMember(20)]
        [Display("Geometric Shadowing")]
        public IMaterialSpecularMicrofacetGeometricShadowingFunction GeometricShadowing { get; set; }

        [DataMember(30)]
        [Display("Normal Distribution")]
        public IMaterialSpecularMicrofacetNormalDistributionFunction NormalDistribution { get; set; }

        public virtual void Visit(MaterialGeneratorContext context)
        {
            var shaderSource = new ShaderClassSource("MaterialSurfaceShadingSpecularMicrofacet");
            context.AddShading(this, shaderSource);
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            //return Equals(other as MaterialSpecularMicrofacetModelFeature);
            throw new NotImplementedException();
        }
    }
}