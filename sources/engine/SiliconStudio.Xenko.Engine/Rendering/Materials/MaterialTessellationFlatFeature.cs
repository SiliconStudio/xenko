// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
        public override void VisitFeature(MaterialGeneratorContext context)
        {
            base.VisitFeature(context);

            if (HasAlreadyTessellationFeature)
                return;

            // set the tessellation method used enumeration
            context.Material.TessellationMethod |= XenkoTessellationMethod.Flat;

            // create and affect the shader source
            var tessellationShader = new ShaderMixinSource();
            tessellationShader.Mixins.Add(new ShaderClassSource("TessellationFlat"));

            context.Parameters.SetResourceSlow(MaterialKeys.TessellationShader, tessellationShader);
        }
    }
}