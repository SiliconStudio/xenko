// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The Cel Shading specular shading model.
    /// </summary>
    [DataContract("MaterialSpecularCelShadingModelFeature")]
    [Display("Cel Shading")]
    public class MaterialSpecularCelShadingModelFeature : MaterialFeature, IMaterialSpecularModelFeature, IEquatable<MaterialSpecularCelShadingModelFeature>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialSpecularCelShadingModelFeature"/> class.
        /// </summary>
        public MaterialSpecularCelShadingModelFeature()
        {
            // Defaults
            Fresnel = new MaterialSpecularMicrofacetFresnelSchlick();
            Visibility = new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX();
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
        [Display("Ramp Function")]
        [NotNull]
        public IMaterialCelShadingLightFunction RampFunction { get; set; } = new MaterialCelShadingLightDefault();

        /// <userdoc>Specify the function to use to calculate the Fresnel component of the micro-facet lighting equation. 
        /// This defines the amount of the incoming light that is reflected.</userdoc>
        [DataMember(20)]
        [Display("Fresnel")]
        [NotNull]
        public IMaterialSpecularMicrofacetFresnelFunction Fresnel { get; set; }

        /// <userdoc>Specify the function to use to calculate the visibility component of the micro-facet lighting equation.</userdoc>
        [DataMember(30)]
        [Display("Visibility")]
        [NotNull]
        public IMaterialSpecularMicrofacetVisibilityFunction Visibility { get; set; }

        /// <userdoc>Specify the function to use to calculate the normal distribution in the micro-facet lighting equation. 
        /// This defines how the normal is distributed.</userdoc>
        [DataMember(40)]
        [Display("Normal Distribution")]
        [NotNull]
        public IMaterialSpecularMicrofacetNormalDistributionFunction NormalDistribution { get; set; }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            var shaderSource = new ShaderMixinSource();
            shaderSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceShadingSpecularCelShading"));

            if (RampFunction != null)
            {
                shaderSource.AddComposition("celLightFunction", RampFunction.Generate(context));
            }

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
            return Equals(other as MaterialSpecularCelShadingModelFeature);
        }

        public bool Equals(MaterialSpecularCelShadingModelFeature other)
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
            return Equals(obj as MaterialSpecularCelShadingModelFeature);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Fresnel != null ? Fresnel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Visibility != null ? Visibility.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NormalDistribution != null ? NormalDistribution.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RampFunction != null ? RampFunction.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
