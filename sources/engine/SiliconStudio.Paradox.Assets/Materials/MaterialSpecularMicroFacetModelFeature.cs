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
    public class MaterialSpecularMicrofacetModelFeature : IMaterialSpecularModelFeature, IEquatable<MaterialSpecularMicrofacetModelFeature>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialSpecularMicrofacetModelFeature"/> class.
        /// </summary>
        public MaterialSpecularMicrofacetModelFeature()
        {
            // Defaults
            Fresnel = new MaterialSpecularMicrofacetFresnelSchlick();
            GeometricShadowing = new MaterialSpecularMicrofacetGeometricShadowingSmithGGXCorrelated();
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
            var shaderSource = new ShaderMixinSource();
            shaderSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceShadingSpecularMicrofacet"));
            
            if (Fresnel != null)
            {
                shaderSource.AddComposition("fresnelFunction", Fresnel.Generate());
            }

            if (GeometricShadowing != null)
            {
                shaderSource.AddComposition("geometricShadowingFunction", GeometricShadowing.Generate());
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
            return Equals(Fresnel, other.Fresnel) && Equals(GeometricShadowing, other.GeometricShadowing) && Equals(NormalDistribution, other.NormalDistribution);
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
                hashCode = (hashCode * 397) ^ (GeometricShadowing != null ? GeometricShadowing.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NormalDistribution != null ? NormalDistribution.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}