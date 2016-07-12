// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Windows;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The diffuse Lambertian for the diffuse material model attribute.
    /// </summary>
    [DataContract("MaterialDiffuseCelShadingModelFeature")]
    [Display("Cel Shading")]
    public class MaterialDiffuseCelShadingModelFeature : MaterialFeature, IMaterialDiffuseModelFeature
    {
        public bool IsLightDependent
        {
            get
            {
                return true;
            }
        }


        [DataMemberIgnore]
        internal bool IsEnergyConservative { get; set; }

        [DataMember(10)]
        [Display("Ramp Function")]
        [NotNull]
        public IMaterialCelShadingCelLightFunction RampFunction { get; set; } = new MaterialCelShadingCelLightDefault();

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            //var shaderSource = new ShaderMixinSource();
            //shaderSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceShadingDiffuseCelShading", IsEnergyConservative));
            //if (RampFunction != null)
            //{
            //    shaderSource.AddComposition("celLightFunction", RampFunction.Generate());
            //}

            var shaderSource = new ShaderClassSource("MaterialSurfaceShadingDiffuseCelShading", IsEnergyConservative);
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