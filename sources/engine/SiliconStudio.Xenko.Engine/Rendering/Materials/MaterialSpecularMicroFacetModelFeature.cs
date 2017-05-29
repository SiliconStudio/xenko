// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The microfacet specular shading model.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetModelFeature")]
    [Display("Microfacet")]
    public class MaterialSpecularMicrofacetModelFeature : MaterialFeature, IMaterialSpecularModelFeature, IEquatable<MaterialSpecularMicrofacetModelFeature>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialSpecularMicrofacetModelFeature"/> class.
        /// </summary>
        public MaterialSpecularMicrofacetModelFeature()
        {
            // Defaults
            Fresnel = new MaterialSpecularMicrofacetFresnelSchlick();
            Visibility = new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX();
            NormalDistribution = new MaterialSpecularMicrofacetNormalDistributionGGX();
        }

        public bool IsLightDependent => true;

        /// <userdoc>Specify the function to use to calculate the Fresnel component of the micro-facet lighting equation. 
        /// This defines the amount of the incoming light that is reflected.</userdoc>
        [DataMember(10)]
        [Display("Fresnel")]
        [NotNull]
        public IMaterialSpecularMicrofacetFresnelFunction Fresnel { get; set; }

        /// <userdoc>Specify the function to use to calculate the visibility component of the micro-facet lighting equation.</userdoc>
        [DataMember(20)]
        [Display("Visibility")]
        [NotNull]
        public IMaterialSpecularMicrofacetVisibilityFunction Visibility { get; set; }

        /// <userdoc>Specify the function to use to calculate the normal distribution in the micro-facet lighting equation. 
        /// This defines how the normal is distributed.</userdoc>
        [DataMember(30)]
        [Display("Normal Distribution")]
        [NotNull]
        public IMaterialSpecularMicrofacetNormalDistributionFunction NormalDistribution { get; set; }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            var shaderSource = new ShaderMixinSource();
            shaderSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceShadingSpecularMicrofacet"));
            
            if (Fresnel != null)
            {
                shaderSource.AddComposition("fresnelFunction", Fresnel.Generate());
            }

            if (Visibility != null)
            {
                shaderSource.AddComposition("geometricShadowingFunction", Visibility.Generate());
            }

            if (NormalDistribution != null)
            {
                shaderSource.AddComposition("normalDistributionFunction", NormalDistribution.Generate());
            }

            context.AddShading(this, shaderSource);
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return Equals(other as MaterialSpecularMicrofacetModelFeature);
        }

        public bool Equals(MaterialSpecularMicrofacetModelFeature other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Fresnel, other.Fresnel) && Equals(Visibility, other.Visibility) && Equals(NormalDistribution, other.NormalDistribution);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals(obj as MaterialSpecularMicrofacetModelFeature);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Fresnel != null ? Fresnel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Visibility != null ? Visibility.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NormalDistribution != null ? NormalDistribution.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
