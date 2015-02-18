// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialShaderResult : LoggerResult
    {
        public Dictionary<MaterialShaderStage, ShaderSource> SurfaceShaders  = new Dictionary<MaterialShaderStage, ShaderSource>();
        public Dictionary<MaterialShaderStage, ShaderSource> StreamInitializers = new Dictionary<MaterialShaderStage, ShaderSource>();

        public ParadoxTessellationMethod TessellationMethod;

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
            context.PushLayer(new MaterialBlendOverrides());
            material.Visit(context);
            context.PopLayer();

            // Squash all operations into a single mixin
            foreach (MaterialShaderStage stage in Enum.GetValues(typeof(MaterialShaderStage)))
            {
                result.SurfaceShaders[stage] = context.GenerateSurfaceShader(stage);
                result.StreamInitializers[stage] = context.GenerateStreamInitializer(stage);
            }

            result.Parameters.Set(MaterialKeys.VertexStageSurfaceShaders, result.SurfaceShaders[MaterialShaderStage.Vertex]);
            result.Parameters.Set(MaterialKeys.DomainStageSurfaceShaders, result.SurfaceShaders[MaterialShaderStage.Domain]);
            result.Parameters.Set(MaterialKeys.PixelStageSurfaceShaders, result.SurfaceShaders[MaterialShaderStage.Pixel]);

            result.Parameters.Set(MaterialKeys.VertexStageStreamInitializer, result.StreamInitializers[MaterialShaderStage.Vertex]);
            result.Parameters.Set(MaterialKeys.DomainStageStreamInitializer, result.StreamInitializers[MaterialShaderStage.Domain]);
            result.Parameters.Set(MaterialKeys.PixelStageStreamInitializer, result.StreamInitializers[MaterialShaderStage.Pixel]);

            result.TessellationMethod = context.TessellationMethod;

            return result;
        }
    }
}