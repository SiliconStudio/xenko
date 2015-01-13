// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialShaderResult : LoggerResult
    {
        public ShaderSource ShaderSource { get; set; }

        public ParameterCollection Parameters { get; set; }
    }

    public class MaterialGenerator
    {
        public static MaterialShaderResult Generate(MaterialAsset material, MaterialGeneratorContext context = null)
        {
            if (material == null) throw new ArgumentNullException("material");
            var result = new MaterialShaderResult();

            if (context == null)
            {
                context = new MaterialGeneratorContext();
            }
            context.Log = result;

            result.Parameters = context.Parameters;
            context.PushLayer();
            material.Visit(context);
            context.PopLayer();

            // Squash all operations into a single mixin
            result.ShaderSource = context.GenerateMixin();
            result.Parameters.Set(MaterialKeys.Material, result.ShaderSource);

            return result;
        }
    }
}