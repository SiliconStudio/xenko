// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// The diffuse Lambertian for the diffuse material model attribute.
    /// </summary>
    [DataContract("MaterialDiffuseLambertModelFeature")]
    [Display("Lambert")]
    public class MaterialDiffuseLambertModelFeature : IMaterialDiffuseModelFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseLambertModelFeature"/> class.
        /// </summary>
        public MaterialDiffuseLambertModelFeature()
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