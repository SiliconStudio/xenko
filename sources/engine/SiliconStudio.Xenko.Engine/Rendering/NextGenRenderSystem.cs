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
    /// <summary>
    /// Facility to perform rendering: extract rendering data from scene, determine effects and GPU states, compute and prepare data (i.e. matrices, buffers, etc...) and finally draw it.
    /// </summary>
    public partial class NextGenRenderSystem
    {
        /// <summary>
        /// List of objects registered in the rendering system.
        /// </summary>
        public TrackingHashSet<RenderObject> RenderObjects = new TrackingHashSet<RenderObject>();

        // TODO GRAPHICS REFACTOR should probably be controlled by graphics compositor?
        /// <summary>
        /// List of render stages.
        /// </summary>
        public TrackingCollection<RenderStage> RenderStages { get; } = new TrackingCollection<RenderStage>();

        /// <summary>
        /// Frame counter, mostly for internal use.
        /// </summary>
        public int FrameCounter { get; private set; }

        /// <summary>
        /// List of render features
        /// </summary>
        public List<RootRenderFeature> RenderFeatures { get; } = new List<RootRenderFeature>();

        /// <summary>
        /// The graphics device, used to create graphics resources.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// The effect system, used to compile effects.
        /// </summary>
        public EffectSystem EffectSystem { get; private set; }

        /// <summary>
        /// List of views.
        /// </summary>
        public TrackingCollection<RenderView> Views { get; } = new TrackingCollection<RenderView>();

        // TODO GRAPHICS REFACTOR should grow as needed
        /// <summary>
        /// The graphics resource descriptor pool, to fill resources needed for rendering during current frame.
        /// </summary>
        public DescriptorPool DescriptorPool { get; private set; }

        // TODO GRAPHICS REFACTOR should grow as needed
        /// <summary>
        /// The graphics resource buffer pool, to fill buffer data needed for rendering during current frame.
        /// </summary>
        public BufferPool BufferPool { get; private set; }

        public RenderContext RenderContextOld { get; private set; }

        public NextGenRenderSystem(IServiceRegistry registry)
        {
            registry.AddService(typeof(NextGenRenderSystem), this);
            EffectSystem = registry.GetSafeServiceAs<EffectSystem>();
            RenderStages.CollectionChanged += RenderStages_CollectionChanged;
        }

        /// <summary>
        /// Initializes the render system.
        /// </summary>
        /// <param name="effectSystem">The effect system.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public void Initialize(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            DescriptorPool = DescriptorPool.New(graphicsDevice, new[]
            {
                new DescriptorTypeCount(EffectParameterClass.ConstantBuffer, 80000),
            });

            BufferPool = BufferPool.New(graphicsDevice, 32 * 1024 * 1024);

            // Be notified when a RenderObject is added or removed
            RenderObjects.CollectionChanged += RenderObjectsCollectionChanged;
            Views.CollectionChanged += Views_CollectionChanged;

            RenderContextOld = RenderContext.GetShared(EffectSystem.Services);
        }

        /// <summary>
        /// Resets views in their original state. Should be called after all views have been enumerated.
        /// </summary>
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

        /// <summary>
        /// Reset render objects and features. Should be called at beginning of Extract phase.
        /// </summary>
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

        /// <summary>
        /// Initializes render features. Should be called after all the render features have been set.
        /// </summary>
        public void InitializeFeatures()
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
    }
}