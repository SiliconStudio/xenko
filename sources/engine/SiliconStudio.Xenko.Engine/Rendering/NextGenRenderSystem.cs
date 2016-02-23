using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Facility to perform rendering: extract rendering data from scene, determine effects and GPU states, compute and prepare data (i.e. matrices, buffers, etc...) and finally draw it.
    /// </summary>
    public partial class NextGenRenderSystem : ComponentBase, IGameSystemBase
    {
        private readonly IServiceRegistry registry;

        /// <summary>
        /// Stores render data.
        /// </summary>
        public RenderDataHolder RenderData;

        public StaticObjectPropertyKey<uint> RenderStageMaskKey;
        public const int RenderStageMaskSizePerEntry = 32; // 32 bits per uint

        /// <summary>
        /// List of objects registered in the rendering system.
        /// </summary>
        public RenderObjectCollection RenderObjects;

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
        public TrackingCollection<RootRenderFeature> RenderFeatures { get; } = new TrackingCollection<RootRenderFeature>();

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

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services => registry;

        public NextGenRenderSystem(IServiceRegistry registry)
        {
            this.registry = registry;

            RenderData.Initialize();
            RenderStageMaskKey = RenderData.CreateStaticObjectKey<uint>(null, (RenderStages.Count + RenderStageMaskSizePerEntry - 1) / RenderStageMaskSizePerEntry);

            registry.AddService(typeof(NextGenRenderSystem), this);
            RenderStages.CollectionChanged += RenderStages_CollectionChanged;
            RenderFeatures.CollectionChanged += RenderFeatures_CollectionChanged;
        }

        /// <summary>
        /// Initializes the render system.
        /// </summary>
        /// <param name="effectSystem">The effect system.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public void Initialize()
        {
            // Get graphics device service
            var graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();

            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();

            // Be notified when a RenderObject is added or removed
            RenderObjects = new RenderObjectCollection(this);
            Views.CollectionChanged += Views_CollectionChanged;

            graphicsDeviceService.DeviceCreated += (sender, args) =>
            {
                GraphicsDevice = graphicsDeviceService.GraphicsDevice;
                RenderContextOld = RenderContext.GetShared(EffectSystem.Services);

                DescriptorPool = DescriptorPool.New(GraphicsDevice, new[]
                {
                    new DescriptorTypeCount(EffectParameterClass.ConstantBuffer, 80000),
                });

                BufferPool = BufferPool.New(GraphicsDevice, 32 * 1024 * 1024);
            };
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

                view.RenderObjects.Clear();

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

        private void PrepareDataArrays()
        {
            // Ensure size for data arrays
            RenderData.PrepareDataArrays(ComputeDataArrayExpectedSize);

            // Also do it for each render feature
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareDataArrays();
            }
        }

        protected int ComputeDataArrayExpectedSize(DataType type)
        {
            switch (type)
            {
                case DataType.StaticObject:
                    return RenderObjects.Count;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RenderStages_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ((RenderStage)e.Item).Index = e.Index;
                    // Make sure mask is big enough
                    RenderData.ChangeDataMultiplier(RenderStageMaskKey, (RenderStages.Count + RenderStageMaskSizePerEntry - 1) / RenderStageMaskSizePerEntry);
                    break;
            }
        }

        private void RenderFeatures_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var renderFeature = (RootRenderFeature)e.Item;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    renderFeature.Index = e.Index;
                    renderFeature.RenderSystem = this;
                    renderFeature.Initialize();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
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

        internal void AddRenderObject(List<RenderObject> renderObjects, RenderObject renderObject)
        {
            renderObject.StaticCommonObjectNode = new StaticObjectNodeReference(renderObjects.Count);

            renderObjects.Add(renderObject);
            
            // Resize arrays to accomodate for new data
            RenderData.PrepareDataArrays(ComputeDataArrayExpectedSize);

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

        internal bool RemoveRenderObject(List<RenderObject> renderObjects, RenderObject renderObject)
        {
            // Get and clear ordered node index
            var orderedRenderNodeIndex = renderObject.StaticObjectNode.Index;
            if (renderObject.StaticCommonObjectNode == StaticObjectNodeReference.Invalid)
                return false;

            renderObject.StaticCommonObjectNode = StaticObjectNodeReference.Invalid;

            // SwapRemove each items in dataArrays
            RenderData.SwapRemoveItem(DataType.StaticObject, orderedRenderNodeIndex, RenderObjects.Count - 1);

            // Remove entry from ordered node index
            renderObjects.SwapRemoveAt(orderedRenderNodeIndex);

            // If last item was moved, update its index
            if (orderedRenderNodeIndex < RenderObjects.Count)
            {
                renderObjects[orderedRenderNodeIndex].StaticCommonObjectNode = new StaticObjectNodeReference(orderedRenderNodeIndex);
            }

            return true;
        }
    }
}