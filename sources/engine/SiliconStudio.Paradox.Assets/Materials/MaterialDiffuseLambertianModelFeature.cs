// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The diffuse Lambertian for the diffuse material model attribute.
    /// </summary>
    [DataContract("MaterialDiffuseLambertianModelFeature")]
    [Display("Lamtertian Model")]
    public class MaterialDiffuseLambertianModelFeature : IMaterialDiffuseModelFeature
    {
        public MaterialDiffuseLambertianModelFeature()
        {
        }

        public bool IsLightDependent
        {
            get
            {
                return true;
            }
        }


        [DataMemberIgnore]
        internal bool IsEnergyConservative { get; set; }

        public virtual void Visit(MaterialGeneratorContext context)
        {
            var shaderSource = new ShaderClassSource("MaterialSurfaceShadingDiffuseLambert", IsEnergyConservative);
            context.AddShading(this, shaderSource);
        }

        public bool Equals(MaterialDiffuseLambertianModelFeature other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsEnergyConservative.Equals(other.IsEnergyConservative);
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return Equals(other as MaterialDiffuseLambertianModelFeature);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as MaterialDiffuseLambertianModelFeature);
        }

        public override int GetHashCode()
        {
            return IsEnergyConservative.GetHashCode();
        }
    }
}