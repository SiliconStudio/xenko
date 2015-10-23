// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    public class MaterialShaderResult : LoggerResult
    {
        public Material Material { get; set; }
    }

    public class MaterialGenerator
    {
        public static MaterialShaderResult Generate(MaterialDescriptor materialDescriptor, MaterialGeneratorContext context, string rootMaterialFriendlyName)
        {
            if (materialDescriptor == null) throw new ArgumentNullException("materialDescriptor");
            if (context == null) throw new ArgumentNullException("context");

            var result = new MaterialShaderResult();
            context.Log = result;

            var material = context.Material;
            result.Material = context.Material;

            context.Parameters = material.Parameters;
            context.PushMaterial(materialDescriptor, rootMaterialFriendlyName);
            context.PushLayer();
            materialDescriptor.Visit(context);
            context.PopLayer();
            context.PopMaterial();

            if (!material.Parameters.ContainsKey(MaterialKeys.TessellationShader))
                material.Parameters.Set(MaterialKeys.TessellationShader, null);

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