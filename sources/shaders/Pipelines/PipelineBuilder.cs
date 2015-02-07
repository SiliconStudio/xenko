// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    // Temporary implementation to at least have proper load/unload, need to be redesigned!
    public abstract class PipelineBuilder
    {
        private readonly List<KeyValuePair<RenderPipeline, Renderer>> renderers = new List<KeyValuePair<RenderPipeline, Renderer>>();
        //private readonly List<PipelineBuilder> pipelineBuilders = new List<PipelineBuilder>();

        private readonly List<EntityProcessor> entityProcessors = new List<EntityProcessor>();

        private readonly IServiceRegistry serviceRegistry;

        private readonly EntitySystem entities;

        private readonly RenderPipeline pipeline;

        protected PipelineBuilder(IServiceRegistry serviceRegistry, RenderPipeline pipeline)
        {
            if (serviceRegistry == null) throw new ArgumentNullException("serviceRegistry");
            if (pipeline == null) throw new ArgumentNullException("pipeline");
            this.serviceRegistry = serviceRegistry;
            this.pipeline = pipeline;

            entities = Services.GetSafeServiceAs<EntitySystem>();
        }

        public IServiceRegistry Services
        {
            get
            {
                return serviceRegistry;
            }
        }

        public EntitySystem Entities
        {
            get
            {
                return entities;
            }
        }

        public RenderPipeline Pipeline
        {
            get
            {
                return pipeline;
            }
        }

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

            //foreach (var pipelineBuilder in pipelineBuilders)
            //{
            //    pipelineBuilder.Unload();
            //}
            //pipelineBuilders.Clear();
        }

        public void AddRenderer(RenderPipeline selectedPipeline, Renderer renderer)
        {
            selectedPipeline.Renderers.Add(renderer);
            renderers.Add(new KeyValuePair<RenderPipeline, Renderer>(selectedPipeline, renderer));
        }

        public void AddRenderer(Renderer renderer)
        {
            AddRenderer(Pipeline, renderer);
        }

        public void RemoveRenderer(RenderPipeline selectedPipeline, Renderer renderer)
        {
            selectedPipeline.Renderers.Remove(renderer);
            renderers.Remove(new KeyValuePair<RenderPipeline, Renderer>(selectedPipeline, renderer));
        }

        public void RemoveRenderer(Renderer renderer)
        {
            RemoveRenderer(Pipeline, renderer);
        }

        public IEnumerable<Renderer> Renderers
        {
            get
            {
                return renderers.Select(key => key.Value);
            }
        }

        //protected void Build(PipelineBuilder pipelineBuilder)
        //{
        //    pipelineBuilders.Add(pipelineBuilder);
        //    pipelineBuilder.Pipeline = Pipeline;
        //    pipelineBuilder.ServiceRegistry = ServiceRegistry;
        //    pipelineBuilder.Load();
        //}
    }
}