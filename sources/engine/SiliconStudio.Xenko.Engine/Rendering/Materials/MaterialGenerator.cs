// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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

            result.Material = context.Material;

            context.PushMaterial(materialDescriptor, rootMaterialFriendlyName);
            
            context.Step = MaterialGeneratorStep.PassesEvaluation;
            materialDescriptor.Visit(context);

            context.Step = MaterialGeneratorStep.GenerateShader;
            for (int pass = 0; pass < context.PassCount; ++pass)
            {
                var materialPass = context.PushPass();

                context.PushLayer(null);
                materialDescriptor.Visit(context);
                context.PopLayer();

                materialPass.Parameters.Set(MaterialKeys.VertexStageSurfaceShaders, context.ComputeShaderSource(MaterialShaderStage.Vertex));
                materialPass.Parameters.Set(MaterialKeys.DomainStageSurfaceShaders, context.ComputeShaderSource(MaterialShaderStage.Domain));
                materialPass.Parameters.Set(MaterialKeys.PixelStageSurfaceShaders, context.ComputeShaderSource(MaterialShaderStage.Pixel));

                materialPass.Parameters.Set(MaterialKeys.VertexStageStreamInitializer, context.GenerateStreamInitializers(MaterialShaderStage.Vertex));
                materialPass.Parameters.Set(MaterialKeys.DomainStageStreamInitializer, context.GenerateStreamInitializers(MaterialShaderStage.Domain));
                materialPass.Parameters.Set(MaterialKeys.PixelStageStreamInitializer, context.GenerateStreamInitializers(MaterialShaderStage.Pixel));

                context.PopPass();
            }
            context.PopMaterial();

            return result;
        }
    }
}
