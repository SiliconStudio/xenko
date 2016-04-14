// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using System.Reflection;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Facility to perform rendering: extract rendering data from scene, determine effects and GPU states, compute and prepare data (i.e. matrices, buffers, etc...) and finally draw it.
    /// </summary>
    public class RenderSystem : ComponentBase
    {
        [Obsolete("This field is provisional and will be replaced by a proper mechanisms in the future")]
        public readonly List<Func<RenderView, RenderObject, bool>> ViewObjectFilters = new List<Func<RenderView, RenderObject, bool>>();

        private readonly Dictionary<Type, RootRenderFeature> renderFeaturesByType = new Dictionary<Type, RootRenderFeature>();
        private readonly HashSet<Type> renderObjectsDefaultPipelinePlugins = new HashSet<Type>();
        private IServiceRegistry registry;

        private SortKey[] sortKeys;

        // TODO GRAPHICS REFACTOR should probably be controlled by graphics compositor?
        /// <summary>
        /// List of render stages.
        /// </summary>
        public FastTrackingCollection<RenderStage> RenderStages { get; } = new FastTrackingCollection<RenderStage>();

        /// <summary>
        /// Frame counter, mostly for internal use.
        /// </summary>
        public int FrameCounter { get; private set; } = 1;

        /// <summary>
        /// List of render features
        /// </summary>
        public FastTrackingCollection<RootRenderFeature> RenderFeatures { get; } = new FastTrackingCollection<RootRenderFeature>();

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
        public FastTrackingCollection<RenderView> Views { get; } = new FastTrackingCollection<RenderView>();

        public RenderContext RenderContextOld { get; private set; }

        public event Action RenderStageSelectorsChanged;

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services => registry;

        public PipelinePluginManager PipelinePlugins { get; }

        public RenderSystem()
        {
            PipelinePlugins = new PipelinePluginManager(this);
            RenderStages.CollectionChanged += RenderStages_CollectionChanged;
            RenderFeatures.CollectionChanged += RenderFeatures_CollectionChanged;
        }

        /// <summary>
        /// Performs pipeline initialization, enumerates views and populates visibility groups.
        /// </summary>
        /// <param name="context"></param>
        public void Collect(RenderDrawContext context)
        {
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Collect();
            }
        }

        /// <summary>
        /// Extract data from entities, should be as fast as possible to not block simulation loop. It should be mostly copies, and the actual processing should be part of Prepare().
        /// </summary>
        public void Extract(RenderDrawContext context)
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
                // Sort per render feature (used for later sorting)
                // We'll be able to process data more efficiently for the next steps
                view.RenderObjects.Sort(RenderObjectFeatureComparer.Default);

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
                        renderViewStage.RenderNodes.Add(new RenderNodeFeatureReference(renderFeature, renderNode, renderObject));
                    }
                }

                // Also sort view|stage per render feature
                foreach (var renderViewStage in view.RenderStages)
                {
                    renderViewStage.RenderNodes.Sort(RenderNodeFeatureReferenceComparer.Default);
                }
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

        /// <summary>
        /// Performs most of the work (computation and resource preparation). Later game simulation might be running during that step.
        /// </summary>
        /// <param name="context"></param>
        public unsafe void Prepare(RenderDrawContext context)
        {
            // Sync point: after extract, before prepare (game simulation could resume now)

            // Generate and execute prepare effect jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.PrepareEffectPermutations(context);
            }

            // Generate and execute prepare jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Prepare(context);
            }

            // Sort
            foreach (var view in Views)
            {
                foreach (var renderViewStage in view.RenderStages)
                {
                    var renderNodes = renderViewStage.RenderNodes;
                    if (renderNodes.Count == 0)
                        continue;

                    var renderStage = renderViewStage.RenderStage;

                    // Allocate sorted render nodes
                    if (renderViewStage.SortedRenderNodes == null || renderViewStage.SortedRenderNodes.Length < renderNodes.Count)
                        Array.Resize(ref renderViewStage.SortedRenderNodes, renderNodes.Count);
                    var sortedRenderNodes = renderViewStage.SortedRenderNodes;

                    if (renderStage.SortMode != null)
                    {
                        // Make sure sortKeys is big enough
                        if (sortKeys == null || sortKeys.Length < renderNodes.Count)
                            Array.Resize(ref sortKeys, renderNodes.Count);

                        // renderNodes[start..end] belongs to the same render feature
                        fixed (SortKey* sortKeysPtr = sortKeys)
                            renderStage.SortMode.GenerateSortKey(view, renderViewStage, sortKeysPtr);

                        Array.Sort(sortKeys, 0, renderNodes.Count);

                        // Reorder list
                        for (int i = 0; i < renderNodes.Count; ++i)
                        {
                            sortedRenderNodes[i] = renderNodes[sortKeys[i].Index];
                        }
                    }
                    else
                    {
                        // No sorting, copy as is
                        for (int i = 0; i < renderNodes.Count; ++i)
                        {
                            sortedRenderNodes[i] = renderNodes[i];
                        }
                    }
                }
            }
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
            var renderNodes = renderViewStage.SortedRenderNodes;
            var renderNodeCount = renderViewStage.RenderNodes.Count;
            int currentStart, currentEnd;

            for (currentStart = 0; currentStart < renderNodeCount; currentStart = currentEnd)
            {
                var currentRenderFeature = renderNodes[currentStart].RootRenderFeature;
                currentEnd = currentStart + 1;
                while (currentEnd < renderNodeCount && renderNodes[currentEnd].RootRenderFeature == currentRenderFeature)
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

        /// <summary>
        /// Adds a <see cref="RenderObject"/> to the rendering.
        /// </summary>
        /// An appropriate <see cref="RootRenderFeature"/> will be found and the object will be initialized with it.
        /// If nothing could be found, <see cref="RenderObject.RenderFeature"/> will be null.
        /// <param name="renderObject"></param>
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

        /// <summary>
        /// Removes a <see cref="RenderObject"/> from the rendering.
        /// </summary>
        /// <param name="renderObject"></param>
        public void RemoveRenderObject(RenderObject renderObject)
        {
            var renderFeature = renderObject.RenderFeature;
            renderFeature?.RemoveRenderObject(renderObject);
        }

        private void PrepareDataArrays()
        {
            // Also do it for each render feature
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareDataArrays();
            }
        }

        private void RenderStages_CollectionChanged(object sender, ref FastTrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ((RenderStage)e.Item).Index = e.Index;
                    break;
            }
        }

        private void RenderFeatures_CollectionChanged(object sender, ref FastTrackingCollectionChangedEventArgs e)
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
                    renderFeature.RenderStageSelectors.CollectionChanged -= RenderStageSelectors_CollectionChanged;
                    renderFeaturesByType.Remove(renderFeature.SupportedRenderObjectType);
                    renderFeature.Unload();
                    break;
            }
        }

        private void RenderStageSelectors_CollectionChanged(object sender, ref FastTrackingCollectionChangedEventArgs e)
        {
            RenderStageSelectorsChanged?.Invoke();
        }

        private void Views_CollectionChanged(object sender, ref FastTrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ((RenderView)e.Item).Index = e.Index;
                    break;
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
                PipelinePlugins.InstantiatePlugin(autoPipelineAttribute.PipelinePluginType);
                return true;
            }

            return false;
        }

        private class RenderNodeFeatureReferenceComparer : IComparer<RenderNodeFeatureReference>
        {
            public static readonly RenderNodeFeatureReferenceComparer Default = new RenderNodeFeatureReferenceComparer();

            public int Compare(RenderNodeFeatureReference x, RenderNodeFeatureReference y)
            {
                return x.RootRenderFeature.Index - y.RootRenderFeature.Index;
            }
        }

        private class RenderObjectFeatureComparer : IComparer<RenderObject>
        {
            public static readonly RenderObjectFeatureComparer Default = new RenderObjectFeatureComparer();

            public int Compare(RenderObject x, RenderObject y)
            {
                return x.RenderFeature.Index - y.RenderFeature.Index;
            }
        }
    }
}