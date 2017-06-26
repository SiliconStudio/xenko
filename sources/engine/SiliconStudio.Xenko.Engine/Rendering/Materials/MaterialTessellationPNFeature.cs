// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Material for Point-Normal tessellation.
    /// </summary>
    [DataContract("MaterialTessellationPNFeature")]
    [Display("Point Normal Tessellation")]
    public class MaterialTessellationPNFeature : MaterialTessellationBaseFeature
    {
        public override void GenerateShader(MaterialGeneratorContext context)
        {
            base.GenerateShader(context);

            if (HasAlreadyTessellationFeature) 
                return;

            // set the tessellation method used enumeration
            context.MaterialPass.TessellationMethod |= XenkoTessellationMethod.PointNormal;

            // create and affect the shader source
            var tessellationShader = new ShaderMixinSource();
            tessellationShader.Mixins.Add(new ShaderClassSource("TessellationPN"));
            if (AdjacentEdgeAverage)
                tessellationShader.Mixins.Add(new ShaderClassSource("TessellationAE4", "PositionWS"));

            context.Parameters.Set(MaterialKeys.TessellationShader, tessellationShader);
        }
    }
}
