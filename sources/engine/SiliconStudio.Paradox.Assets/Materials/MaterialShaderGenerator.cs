// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialShaderResult : LoggerResult
    {
        public ShaderSource ShaderSource { get; set; }

        public ParameterCollectionData Parameters { get; set; }
    }

    public class MaterialShaderGenerator
    {
        public static MaterialShaderResult Generate(MaterialAsset material)
        {
            if (material == null) throw new ArgumentNullException("material");
            var result = new MaterialShaderResult();

            var context = new MaterialShaderGeneratorContext()
            {
                Log = result
            };

            result.Parameters = context.Parameters;
            material.GenerateShader(context);

            // Squash all operations into a single mixin
            result.ShaderSource = context.GenerateMixin();
            result.Parameters.Set(MaterialKeys.Material, result.ShaderSource);

            return result;
        }
    }
}