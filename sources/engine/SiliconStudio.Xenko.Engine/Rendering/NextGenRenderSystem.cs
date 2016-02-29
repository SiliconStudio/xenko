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

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Facility to perform rendering: extract rendering data from scene, determine effects and GPU states, compute and prepare data (i.e. matrices, buffers, etc...) and finally draw it.
    /// </summary>
    public partial class NextGenRenderSystem : ComponentBase
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

        public NextGenRenderSystem()
        {
            RenderStages.CollectionChanged += RenderStages_CollectionChanged;
            RenderFeatures.CollectionChanged += RenderFeatures_CollectionChanged;
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