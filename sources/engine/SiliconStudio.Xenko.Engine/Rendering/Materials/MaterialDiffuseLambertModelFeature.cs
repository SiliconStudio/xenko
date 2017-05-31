// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The diffuse Lambertian for the diffuse material model attribute.
    /// </summary>
    [DataContract("MaterialDiffuseLambertModelFeature")]
    [Display("Lambert")]
    public class MaterialDiffuseLambertModelFeature : MaterialFeature, IMaterialDiffuseModelFeature, IEnergyConservativeDiffuseModelFeature
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

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            var shaderSource = new ShaderClassSource("MaterialSurfaceShadingDiffuseLambert", IsEnergyConservative);
            context.AddShading(this, shaderSource);
        }

        public bool Equals(MaterialDiffuseLambertModelFeature other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsEnergyConservative.Equals(other.IsEnergyConservative);
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return Equals(other as MaterialDiffuseLambertModelFeature);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as MaterialDiffuseLambertModelFeature);
        }

        public override int GetHashCode()
        {
            return IsEnergyConservative.GetHashCode();
        }
    }
}
