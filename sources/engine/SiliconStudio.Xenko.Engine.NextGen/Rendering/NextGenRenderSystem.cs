using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public partial class NextGenRenderSystem
    {
        // Public registered object management
        public TrackingHashSet<RenderObject> RenderObjects = new TrackingHashSet<RenderObject>();

        // List of render stages (will probably be controlled by graphics compositor)
        internal TrackingCollection<RenderStage> RenderStages = new TrackingCollection<RenderStage>();

        /// <summary>
        /// Frame counter, mostly for internal use.
        /// </summary>
        public int FrameCounter { get; private set; }

        // List of render features
        public List<RootRenderFeature> RenderFeatures = new List<RootRenderFeature>();

        // Engine entry points
        public GraphicsDevice GraphicsDevice { get; private set; }
        public EffectSystem EffectSystem { get; private set; }

        // Views
        public TrackingCollection<RenderView> Views { get; } = new TrackingCollection<RenderView>();

        public DescriptorPool DescriptorPool { get; private set; }
        public BufferPool BufferPool { get; private set; }

        public RenderContext RenderContextOld { get; private set; }

        public NextGenRenderSystem(IServiceRegistry registry)
        {
            registry.AddService(typeof(NextGenRenderSystem), this);
            RenderStages.CollectionChanged += RenderStages_CollectionChanged;
        }

        public void Initialize(EffectSystem effectSystem, GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            EffectSystem = effectSystem;

            DescriptorPool = DescriptorPool.New(graphicsDevice, new[]
            {
                new DescriptorTypeCount(EffectParameterClass.ConstantBuffer, 80000),
            });

            BufferPool = BufferPool.New(graphicsDevice, 32 * 1024 * 1024);

            // Be notified when a RenderObject is added or removed
            RenderObjects.CollectionChanged += RenderObjectsCollectionChanged;
            Views.CollectionChanged += Views_CollectionChanged;

            RenderContextOld = RenderContext.GetShared(effectSystem.Services);
        }

        private void RenderObjectsCollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    AddRenderObject((RenderObject)e.Item);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    RemoveRenderObject((RenderObject)e.OldItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Views_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ((RenderView)e.Item).Index = e.Index;
                    break;
            }
        }

        private void AddRenderObject(RenderObject renderObject)
        {
            // Determine which RenderFeatures is responsible for this object
            foreach (var renderFeature in RenderFeatures)
            {
                // Find matching render feature
                if (renderFeature.SupportsRenderObject(renderObject))
                {
                    renderFeature.AddRenderObject(this, renderObject);
                    break;
                }
            }
        }

        private void RemoveRenderObject(RenderObject renderObject)
        {
            renderObject.RenderFeature?.RemoveRenderObject(renderObject);
        }

        public void ResetViews()
        {
            // Prepare views
            for (int index = 0; index < Views.Count; index++)
            {
                // Update indices
                var view = Views[index];
                view.Index = index;

                // Clear nodes
                while (view.Features.Count < RenderFeatures.Count)
                {
                    view.Features.Add(new RenderViewFeature());
                }

                for (int i = 0; i < RenderFeatures.Count; i++)
                {
                    var renderViewFeature = view.Features[i];
                    renderViewFeature.RootFeature = RenderFeatures[i];

                    renderViewFeature.RenderNodes.Clear();
                    renderViewFeature.ViewObjectNodes.Clear();
                    renderViewFeature.Layouts.Clear();
                }

                foreach (var renderViewStage in view.RenderStages)
                {
                    renderViewStage.RenderNodes.Clear();
                }
            }
        }

        public void Reset()
        {
            FrameCounter++;

            // Clear pools
            BufferPool.Reset();
            DescriptorPool.Reset();

            // Clear object data
            foreach (var renderObject in RenderObjects)
            {
                renderObject.ObjectNode = ObjectNodeReference.Invalid;
            }

            // Clear render features node lists
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Reset();
            }
        }

        public void Initialize()
        {
            for (int index = 0; index < RenderFeatures.Count; index++)
            {
                var renderFeature = RenderFeatures[index];
                renderFeature.Index = index;
                renderFeature.RenderSystem = this;
                renderFeature.Initialize();
            }
        }

        private void RenderStages_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ((RenderStage)e.Item).Index = e.Index;
                    break;
            }
        }
    }
}