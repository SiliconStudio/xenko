// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Material for flat (dicing) tessellation.    
    /// </summary>
    [DataContract("MaterialTessellationFlatFeature")]
    [Display("Flat Tessellation")]
    public class MaterialTessellationFlatFeature : MaterialTessellationBaseFeature
    {
        public override void GenerateShader(MaterialGeneratorContext context)
        {
            base.GenerateShader(context);

            if (HasAlreadyTessellationFeature)
                return;

            // set the tessellation method used enumeration
            context.MaterialPass.TessellationMethod |= XenkoTessellationMethod.Flat;

            // create and affect the shader source
            var tessellationShader = new ShaderMixinSource();
            tessellationShader.Mixins.Add(new ShaderClassSource("TessellationFlat"));

            context.Parameters.Set(MaterialKeys.TessellationShader, tessellationShader);
        }
    }
}
