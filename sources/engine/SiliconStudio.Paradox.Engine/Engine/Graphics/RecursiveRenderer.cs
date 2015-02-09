// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="Renderer"/> recursively render another <see cref="RenderPass"/>.
    /// </summary>
    public class RecursiveRenderer : Renderer
    {
        private readonly PipelineManager pipelineManager;

        public RecursiveRenderer(IServiceRegistry services, PipelineManager pipelineManager, RenderPipeline recursivePipeline) : base(services)
        {
            if (pipelineManager == null) throw new ArgumentNullException("pipelineManager");
            this.pipelineManager = pipelineManager;
            RecursivePipeline = recursivePipeline;
        }

        public RenderPipeline RecursivePipeline { get; set; }

        public override void Load()
        {
            base.Load();

            // Register pipeline
            pipelineManager.Pipelines.Add(RecursivePipeline);
        }

        public override void Unload()
        {
            base.Unload();

            // Unregister pipeline
            pipelineManager.Pipelines.Remove(RecursivePipeline);
        }

        protected override void OnRendering(RenderContext context)
        {
            // Save RenderPass
            var currentPass = context.CurrentPass;

            pipelineManager.Draw(RecursivePipeline, context);

            // Restore RenderPass
            context.CurrentPass = currentPass;
        }
    }
}