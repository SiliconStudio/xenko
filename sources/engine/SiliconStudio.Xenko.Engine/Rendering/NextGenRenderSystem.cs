using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;
using System.Reflection;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Facility to perform rendering: extract rendering data from scene, determine effects and GPU states, compute and prepare data (i.e. matrices, buffers, etc...) and finally draw it.
    /// </summary>
    public class NextGenRenderSystem : ComponentBase
    {
        private readonly Dictionary<Type, RootRenderFeature> renderFeaturesByType = new Dictionary<Type, RootRenderFeature>();
        private readonly Dictionary<Type, IPipelinePlugin> pipelinePlugins = new Dictionary<Type, IPipelinePlugin>();
        private readonly HashSet<Type> renderObjectsDefaultPipelinePlugins = new HashSet<Type>();
        private IServiceRegistry registry;

        // TODO GRAPHICS REFACTOR should probably be controlled by graphics compositor?
        /// <summary>
        /// List of render stages.
        /// </summary>
        public TrackingCollection<RenderStage> RenderStages { get; } = new TrackingCollection<RenderStage>();

        /// <summary>
        /// Frame counter, mostly for internal use.
        /// </summary>
        public int FrameCounter { get; private set; } = 1;

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

        public RenderContext RenderContextOld { get; private set; }

        public event Action RenderStageSelectorsChanged;

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services => registry;

        // Render stages
        internal ForwardLightingRenderFeature forwardLightingRenderFeature;

        public NextGenRenderSystem()
        {
            RenderStages.CollectionChanged += RenderStages_CollectionChanged;
            RenderFeatures.CollectionChanged += RenderFeatures_CollectionChanged;
        }

        public void UpdateCameraToRenderView(RenderDrawContext context, RenderView renderView)
        {
            // TODO: Currently set up during BeforeExtract/Prepare/Draw. Should be initialized before
            if (renderView.SceneCameraRenderer == null)
                return;

            renderView.Camera = renderView.SceneCameraSlotCollection.GetCamera(renderView.SceneCameraRenderer.Camera);

            if (renderView.Camera == null)
                return;

            // Setup viewport size
            var currentViewport = renderView.SceneCameraRenderer.ComputedViewport;
            var aspectRatio = currentViewport.AspectRatio;

            // Update the aspect ratio
            if (renderView.Camera.UseCustomAspectRatio)
            {
                aspectRatio = renderView.Camera.AspectRatio;
            }

            // If the aspect ratio is calculated automatically from the current viewport, update matrices here
            renderView.Camera.Update(aspectRatio);

            renderView.View = renderView.Camera.ViewMatrix;
            renderView.Projection = renderView.Camera.ProjectionMatrix;

            Matrix.Multiply(ref renderView.View, ref renderView.Projection, out renderView.ViewProjection);
        }

        public void Prepare(RenderThreadContext context)
        {
            // Sync point: after extract, before prepare (game simulation could resume now)

            // Generate and execute prepare effect jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.PrepareEffectPermutations();
            }

            // Generate and execute prepare jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Prepare(context);
            }
        }

        public void Extract(RenderThreadContext context)
        {
            // Prepare views
            for (int index = 0; index < Views.Count; index++)
            {
                // Update indices
                var view = Views[index];
                view.Index = index;

                // Create missing RenderViewFeature
                while (view.Features.Count < RenderFeatures.Count)
                {
                    view.Features.Add(new RenderViewFeature());
                }

                for (int i = 0; i < RenderFeatures.Count; i++)
                {
                    var renderViewFeature = view.Features[i];
                    renderViewFeature.RootFeature = RenderFeatures[i];
                }
            }

            // Create nodes for objects to render
            foreach (var view in Views)
            {
                foreach (var renderObject in view.RenderObjects)
                {
                    var renderFeature = renderObject.RenderFeature;
                    var viewFeature = view.Features[renderFeature.Index];

                    // Create object node
                    renderFeature.GetOrCreateObjectNode(renderObject);

                    // Let's create the view object node
                    var renderViewNode = renderFeature.CreateViewObjectNode(view, renderObject);
                    viewFeature.ViewObjectNodes.Add(renderViewNode);

                    // Collect object
                    // TODO: Check which stage it belongs to (and skip everything if it doesn't belong to any stage)
                    // TODO: For now, we build list and then copy. Another way would be to count and then fill (might be worse, need to check)
                    var activeRenderStages = renderObject.ActiveRenderStages;
                    foreach (var renderViewStage in view.RenderStages)
                    {
                        // Check if this RenderObject wants to be rendered for this render stage
                        var renderStageIndex = renderViewStage.RenderStage.Index;
                        if (!activeRenderStages[renderStageIndex].Active)
                            continue;

                        var renderNode = renderFeature.CreateRenderNode(renderObject, view, renderViewNode, renderViewStage.RenderStage);

                        // Note: Used mostly during updating
                        viewFeature.RenderNodes.Add(renderNode);

                        // Note: Used mostly during rendering
                        renderViewStage.RenderNodes.Add(new RenderNodeFeatureReference(renderFeature, renderNode));
                    }
                }

                // TODO: Sort RenderStage.RenderNodes
            }

            // Ensure size of data arrays per objects
            PrepareDataArrays();

            // Generate and execute extract jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Extract();
            }

            // Ensure size of all other data arrays
            PrepareDataArrays();
        }

        public void Draw(RenderDrawContext renderDrawContext, RenderView renderView, RenderStage renderStage)
        {
            // Sync point: draw (from now, we should execute with a graphics device context to perform rendering)

            // Look for the RenderViewStage corresponding to this RenderView | RenderStage combination
            RenderViewStage renderViewStage = null;
            foreach (var currentRenderViewStage in renderView.RenderStages)
            {
                if (currentRenderViewStage.RenderStage == renderStage)
                {
                    renderViewStage = currentRenderViewStage;
                    break;
                }
            }

            if (renderViewStage == null)
            {
                throw new InvalidOperationException("Requested RenderView|RenderStage combination doesn't exist. Please add it to RenderView.RenderStages.");
            }

            // Generate and execute draw jobs
            var renderNodes = renderViewStage.RenderNodes;
            int currentStart, currentEnd;

            for (currentStart = 0; currentStart < renderNodes.Count; currentStart = currentEnd)
            {
                var currentRenderFeature = renderNodes[currentStart].RootRenderFeature;
                currentEnd = currentStart + 1;
                while (currentEnd < renderNodes.Count && renderNodes[currentEnd].RootRenderFeature == currentRenderFeature)
                {
                    currentEnd++;
                }

                // Divide into task chunks for parallelism
                currentRenderFeature.Draw(renderDrawContext, renderView, renderViewStage, currentStart, currentEnd);
            }
        }

        /// <summary>
        /// Initializes the render system.
        /// </summary>
        /// <param name="effectSystem">The effect system.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public void Initialize(RenderContext context)
        {
            registry = context.Services;

            // Get graphics device service
            var graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();

            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();

            // Be notified when a RenderObject is added or removed
            Views.CollectionChanged += Views_CollectionChanged;

            GraphicsDevice = graphicsDeviceService.GraphicsDevice;
            RenderContextOld = context;

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Initialize(RenderContextOld);
            }
        }

        protected override void Destroy()
        {
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Dispose();
            }

            base.Destroy();
        }

        /// <summary>
        /// Reset render objects and features. Should be called at beginning of Extract phase.
        /// </summary>
        public void Reset()
        {
            FrameCounter++;

            // Clear render features node lists
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Reset();
            }

            // Clear views
            foreach (var view in Views)
            {
                // Clear nodes
                view.RenderObjects.Clear();

                foreach (var renderViewFeature in view.Features)
                {
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

        private void PrepareDataArrays()
        {
            // Also do it for each render feature
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareDataArrays();
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

        private void RenderFeatures_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var renderFeature = (RootRenderFeature)e.Item;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    renderFeature.Index = e.Index;
                    renderFeature.RenderSystem = this;

                    if (RenderContextOld != null)
                        renderFeature.Initialize(RenderContextOld);

                    renderFeature.RenderStageSelectors.CollectionChanged += RenderStageSelectors_CollectionChanged;

                    renderFeaturesByType.Add(renderFeature.SupportedRenderObjectType, renderFeature);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
            }
        }

        private void RenderStageSelectors_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            RenderStageSelectorsChanged?.Invoke();
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

        public void AddRenderObject(RenderObject renderObject)
        {
            RootRenderFeature renderFeature;

            if (renderFeaturesByType.TryGetValue(renderObject.GetType(), out renderFeature))
            {
                // Found it
                renderFeature.AddRenderObject(renderObject);
            }
            else
            {
                // New type without render feature, let's do auto pipeline setup
                if (InstantiateDefaultPipelinePlugin(renderObject.GetType()))
                {
                    // Try again, after pipeline plugin setup
                    if (renderFeaturesByType.TryGetValue(renderObject.GetType(), out renderFeature))
                    {
                        renderFeature.AddRenderObject(renderObject);
                    }
                }
            }
        }

        private bool InstantiateDefaultPipelinePlugin(Type renderObjectType)
        {
            // Already processed
            if (!renderObjectsDefaultPipelinePlugins.Add(renderObjectType))
                return false;

            var autoPipelineAttribute = renderObjectType.GetTypeInfo().GetCustomAttribute<DefaultPipelinePluginAttribute>();
            if (autoPipelineAttribute != null)
            {
                GetPipelinePlugin(autoPipelineAttribute.PipelinePluginType, true);
                return true;
            }

            return false;
        }

        public T GetPipelinePlugin<T>(bool createIfNecessary)
        {
            return (T)GetPipelinePlugin(typeof(T), createIfNecessary);
        }

        private IPipelinePlugin GetPipelinePlugin(Type pipelinePluginType, bool createIfNecessary)
        {
            IPipelinePlugin pipelinePlugin;
            if (!pipelinePlugins.TryGetValue(pipelinePluginType, out pipelinePlugin) && createIfNecessary)
            {
                pipelinePlugin = (IPipelinePlugin)Activator.CreateInstance(pipelinePluginType);
                pipelinePlugins.Add(pipelinePluginType, pipelinePlugin);
                pipelinePlugin.SetupPipeline(RenderContextOld, this);
            }

            return pipelinePlugin;
        }

        public void RemoveRenderObject(RenderObject renderObject)
        {
            var renderFeature = renderObject.RenderFeature;
            renderFeature?.RemoveRenderObject(renderObject);
        }
    }
}