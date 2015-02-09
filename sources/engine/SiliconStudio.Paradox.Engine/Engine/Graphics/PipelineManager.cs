// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Specialized;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Manages a rendering pipeline.
    /// </summary>
    public class PipelineManager
    {
        private readonly TrackingHashSet<RenderPipeline> pipelines = new TrackingHashSet<RenderPipeline>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineManager"/> class.
        /// </summary>
        public PipelineManager()
        {
            // Create default pipeline
            Pipeline = new RenderPipeline("Main");

            // Register default pipeline
            pipelines.CollectionChanged += Pipelines_CollectionChanged;
            pipelines.Add(Pipeline);
        }

        /// <summary>
        /// Gets the root pipeline, used as entry point for rendering.
        /// </summary>
        /// <value>
        /// The pipeline.
        /// </value>
        public RenderPipeline Pipeline { get; private set; }

        /// <summary>
        /// Gets all the existing registered pipelines.
        /// </summary>
        /// <value>
        /// The registered pipelines.
        /// </value>
        public TrackingHashSet<RenderPipeline> Pipelines
        {
            get { return pipelines; }
        }

        public void Draw(RenderPass pass, RenderContext context)
        {
            context.CurrentPass = pass;

            if (pass.Name != null)
            {
                context.GraphicsDevice.BeginProfile(Color.Green, pass.Name);
            }

            pass.StartPass.Invoke(context);

            foreach (var child in pass.Children)
            {
                Draw(child, context);
            }

            context.CurrentPass = pass;
            pass.EndPass.Invoke(context);

            if (pass.Name != null)
            {
                context.GraphicsDevice.EndProfile();
            }
        }

        private void RenderPassAdded(RenderPass renderPass)
        {
            foreach (var child in renderPass.Children)
            {
                RenderPassAdded(child);
            }
            renderPass.Children.CollectionChanged += Pipelines_CollectionChanged;

            foreach (var processor in renderPass.Renderers)
            {
                processor.Load();
            }
            renderPass.Renderers.CollectionChanged += Renderers_CollectionChanged;
        }

        private void RenderPassRemoved(RenderPass renderPass)
        {
            foreach (var child in renderPass.Children)
            {
                RenderPassRemoved(child);
            }
            renderPass.Children.CollectionChanged -= Pipelines_CollectionChanged;

            foreach (var processor in renderPass.Renderers)
            {
                processor.Unload();
            }
            renderPass.Renderers.CollectionChanged -= Renderers_CollectionChanged;
        }

        private void Pipelines_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var renderPass = (RenderPass)e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RenderPassAdded(renderPass);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RenderPassRemoved(renderPass);
                    break;
            }
        }

        private void Renderers_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var renderer = (Renderer)e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    renderer.Load();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    renderer.Unload();
                    break;
            }
        }
    }
}