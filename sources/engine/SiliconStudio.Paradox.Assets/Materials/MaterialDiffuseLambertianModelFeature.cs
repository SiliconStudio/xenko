// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The diffuse Lambertian for the diffuse material model attribute.
    /// </summary>
    [DataContract("MaterialDiffuseLambertianModelFeature")]
    [Display("Lamtertian")]
    public class MaterialDiffuseLambertianModelFeature : IMaterialDiffuseModelFeature
    {
        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            var shaderSource = new ShaderClassSource("MaterialLayerShadingDiffuseLambert");
            context.DiffuseModel = new KeyValuePair<IMaterialDiffuseModelFeature, ShaderSource>(this, shaderSource);
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return other is MaterialDiffuseLambertianModelFeature;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IMaterialShadingModelFeature)obj);
        }

        public override int GetHashCode()
        {
            return typeof(MaterialDiffuseLambertianModelFeature).GetHashCode();
        }
    }
}