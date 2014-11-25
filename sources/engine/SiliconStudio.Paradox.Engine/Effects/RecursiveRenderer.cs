// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="Renderer"/> recursively render another <see cref="RenderPass"/>.
    /// </summary>
    public class RecursiveRenderer : Renderer
    {
        public RecursiveRenderer(IServiceRegistry services, RenderPipeline recursivePipeline) : base(services)
        {
            RecursivePipeline = recursivePipeline;
        }

        public RenderPipeline RecursivePipeline { get; set; }

        public override void Load()
        {
            base.Load();

            // Register pipeline
            RenderSystem.Pipelines.Add(RecursivePipeline);
        }

        public override void Unload()
        {
            base.Unload();

            // Unregister pipeline
            RenderSystem.Pipelines.Remove(RecursivePipeline);
        }

        protected override void OnRendering(RenderContext context)
        {
            // Save RenderPass
            var currentPass = context.CurrentPass;

            RenderSystem.Draw(RecursivePipeline, context);

            // Restore RenderPass
            context.CurrentPass = currentPass;
        }
    }
}