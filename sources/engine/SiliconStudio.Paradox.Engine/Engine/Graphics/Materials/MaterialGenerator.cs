// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Engine.Graphics.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialShaderResult : LoggerResult
    {
        public ShaderSource VertexStageSurfaceShader { get; set; }

        public ShaderSource PixelStageSurfaceShader { get; set; }

        public ParameterCollection Parameters { get; set; }
    }

    public class MaterialGenerator
    {
        public static MaterialShaderResult Generate(MaterialDescriptor material, MaterialGeneratorContext context = null)
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
            result.VertexStageSurfaceShader = context.GenerateMixin(MaterialShaderStage.Vertex);
            result.PixelStageSurfaceShader = context.GenerateMixin(MaterialShaderStage.Pixel);
            result.Parameters.Set(MaterialKeys.VertexStageSurfaceShaders, result.VertexStageSurfaceShader);
            result.Parameters.Set(MaterialKeys.PixelStageSurfaceShaders, result.PixelStageSurfaceShader);

            return result;
        }
    }
}