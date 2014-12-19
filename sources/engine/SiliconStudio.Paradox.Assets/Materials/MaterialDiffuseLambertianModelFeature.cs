// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;

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
            throw new NotImplementedException();
        }
    }
}