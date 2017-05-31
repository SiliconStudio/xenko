// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The diffuse Cel Shading for the diffuse material model attribute.
    /// </summary>
    [DataContract("MaterialDiffuseCelShadingModelFeature")]
    [Display("Cel Shading")]
    public class MaterialDiffuseCelShadingModelFeature : MaterialFeature, IMaterialDiffuseModelFeature, IEquatable<MaterialDiffuseCelShadingModelFeature>, IEnergyConservativeDiffuseModelFeature
    {
        public bool IsLightDependent
        {
            get
            {
                return true;
            }
        }

        [DataMemberIgnore]
        bool IEnergyConservativeDiffuseModelFeature.IsEnergyConservative { get; set; }

        private bool IsEnergyConservative => ((IEnergyConservativeDiffuseModelFeature)this).IsEnergyConservative;

        /// <summary>
        /// When positive, the dot product between N and L will be modified to account for light intensity with the specified value as a factor
        /// </summary>
        [DataMember(5)]
        [Display("Modify N.L factor")]
        public float FakeNDotL { get; set; } = 0;

        [DataMember(10)]
        [Display("Ramp Function")]
        [NotNull]
        public IMaterialCelShadingLightFunction RampFunction { get; set; } = new MaterialCelShadingLightDefault();

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            var shaderSource = new ShaderMixinSource();
            shaderSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceShadingDiffuseCelShading", IsEnergyConservative, FakeNDotL));
            if (RampFunction != null)
            {
                shaderSource.AddComposition("celLightFunction", RampFunction.Generate(context));
            }

            context.AddShading(this, shaderSource);
        }

        public bool Equals(MaterialDiffuseCelShadingModelFeature other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsEnergyConservative.Equals(other.IsEnergyConservative);
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return Equals(other as MaterialDiffuseCelShadingModelFeature);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as MaterialDiffuseCelShadingModelFeature);
        }

        public override int GetHashCode()
        {
            var hashCode = RampFunction?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ (IsEnergyConservative.GetHashCode());
            return hashCode;
        }
    }
}
