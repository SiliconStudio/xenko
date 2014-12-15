// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Renders its <see cref="RenderSystem.Pipeline"/>, which will usually result in drawing all meshes, UI, etc...
    /// </summary>
    public class RenderSystem : GameSystemBase
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("RenderSystem");

        private RenderContext drawContext;
        private TrackingHashSet<RenderPipeline> pipelines = new TrackingHashSet<RenderPipeline>();

        internal readonly List<SpriteRenderer> SpriteRenderProcessors = new List<SpriteRenderer>(); 

        public RenderSystem(IServiceRegistry registry)
            : base(registry)
        {
            pipelines.CollectionChanged += Pipelines_CollectionChanged;

            // Register both implem and interface
            Services.AddService(typeof(RenderSystem), this);

            // Create default pipeline
            Pipeline = new RenderPipeline("Main");

            // Register default pipeline
            pipelines.Add(Pipeline);

            Visible = true;
        }

        /// <inheritdoc/>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Create the drawing context
            drawContext = new RenderContext(GraphicsDevice);
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

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // TODO should we clear drawing context parameter collection at each frame?

            try
            {
                GraphicsDevice.Begin();

                GraphicsDevice.ClearState();

                // Update global time
                GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
                GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

                if (GraphicsDevice.IsProfilingSupported)
                {
                    GraphicsDevice.EnableProfile(true);
                }

                // Draw recursively the Pipeline
                Draw(Pipeline, drawContext);
            }
            catch (Exception ex)
            {
                Log.Error("An exception occured while rendering", ex);
            }
            finally
            {
                GraphicsDevice.End();
            }
        }

        /// <inheritdoc/>
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