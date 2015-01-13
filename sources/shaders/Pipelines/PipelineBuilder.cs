// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    // Temporary implementation to at least have proper load/unload, need to be redesigned!
    public abstract class PipelineBuilder
    {
        public static implicit operator PipelineBuilder(Renderer renderer)
        {
            return new SimpleRendererPipelineBuilder(renderer);
        }

        private readonly List<KeyValuePair<RenderPipeline, Renderer>> renderers = new List<KeyValuePair<RenderPipeline, Renderer>>();
        private readonly List<PipelineBuilder> pipelineBuilders = new List<PipelineBuilder>();

        private readonly List<EntityProcessor> entityProcessors = new List<EntityProcessor>();

        public IServiceRegistry ServiceRegistry { get; set; }

        public RenderPipeline Pipeline { get; set; }

        public virtual void Load()
        {
        }

        public virtual void Unload()
        {
            foreach (var renderer in renderers)
            {
                renderer.Key.Renderers.Remove(renderer.Value);
            }
            renderers.Clear();

            var entitySystem = ServiceRegistry.GetServiceAs<EntitySystem>();
            if (entitySystem != null)
            {
                foreach (var entityProcessor in entityProcessors)
                {
                    entitySystem.Processors.Remove(entityProcessor);
                }
            }
            entityProcessors.Clear();

            foreach (var pipelineBuilder in pipelineBuilders)
            {
                pipelineBuilder.Unload();
            }
            pipelineBuilders.Clear();
        }

        protected void AddRenderer(RenderPipeline pipeline, Renderer renderer)
        {
            pipeline.Renderers.Add(renderer);
            renderers.Add(new KeyValuePair<RenderPipeline, Renderer>(pipeline, renderer));
        }

        protected void AddRenderer(Renderer renderer)
        {
            AddRenderer(Pipeline, renderer);
        }

        protected void Build(PipelineBuilder pipelineBuilder)
        {
            pipelineBuilders.Add(pipelineBuilder);
            pipelineBuilder.Pipeline = Pipeline;
            pipelineBuilder.ServiceRegistry = ServiceRegistry;
            pipelineBuilder.Load();
        }
    }
}