// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    public class MaterialShaderResult : LoggerResult
    {
        public Material Material { get; set; }
    }

    public class MaterialGenerator
    {
        public static MaterialShaderResult Generate(MaterialDescriptor materialDescriptor, MaterialGeneratorContext context = null)
        {
            if (materialDescriptor == null) throw new ArgumentNullException("materialDescriptor");
            var result = new MaterialShaderResult();

            if (context == null)
            {
                context = new MaterialGeneratorContext(new Material());
            }
            context.Log = result;

            var material = context.Material;
            result.Material = context.Material;

            context.Parameters = material.Parameters;
            context.PushLayer();
            materialDescriptor.Visit(context);
            context.PopLayer();

            material.Parameters.Set(MaterialKeys.VertexStageSurfaceShaders, context.GenerateSurfaceShader(MaterialShaderStage.Vertex));
            material.Parameters.Set(MaterialKeys.DomainStageSurfaceShaders, context.GenerateSurfaceShader(MaterialShaderStage.Domain));
            material.Parameters.Set(MaterialKeys.PixelStageSurfaceShaders, context.GenerateSurfaceShader(MaterialShaderStage.Pixel));

            material.Parameters.Set(MaterialKeys.VertexStageStreamInitializer, context.GenerateStreamInitializer(MaterialShaderStage.Vertex));
            material.Parameters.Set(MaterialKeys.DomainStageStreamInitializer, context.GenerateStreamInitializer(MaterialShaderStage.Domain));
            material.Parameters.Set(MaterialKeys.PixelStageStreamInitializer, context.GenerateStreamInitializer(MaterialShaderStage.Pixel));

            return result;
        }
    }
}