// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// The microfacet specular shading model.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetModelFeature")]
    [Display("Microfacet")]
    public class MaterialSpecularMicrofacetModelFeature : IMaterialSpecularModelFeature, IEquatable<MaterialSpecularMicrofacetModelFeature>
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

        public bool IsLightDependent
        {
            get
            {
                return true;
            }
        }

        [DataMember(10)]
        [Display("Fresnel")]
        [NotNull]
        public IMaterialSpecularMicrofacetFresnelFunction Fresnel { get; set; }

        [DataMember(20)]
        [Display("Visibility")]
        [NotNull]
        public IMaterialSpecularMicrofacetVisibilityFunction Visibility { get; set; }

        [DataMember(30)]
        [Display("Normal Distribution")]
        [NotNull]
        public IMaterialSpecularMicrofacetNormalDistributionFunction NormalDistribution { get; set; }

        public virtual void Visit(MaterialGeneratorContext context)
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