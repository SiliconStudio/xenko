using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a group of visible <see cref="RenderObject"/>.
    /// </summary>
    public class VisibilityGroup : IDisposable
    {
        private int stageMaskMultiplier;

        // TODO GRAPHICS REFACTOR not thread-safe
        private uint[] viewRenderStageMask;

        internal bool NeedActiveRenderStageReevaluation;

        public readonly StaticObjectPropertyKey<uint> RenderStageMaskKey;
        public const int RenderStageMaskSizePerEntry = 32; // 32 bits per uint

        public NextGenRenderSystem RenderSystem { get; }

        /// <summary>
        /// List of views this visibility group will render to. Those views should exist in <see cref="NextGenRenderSystem.Views"/>.
        /// </summary>
        public List<RenderView> Views { get; } = new List<RenderView>();

        /// <summary>
        /// Stores render data.
        /// </summary>
        public RenderDataHolder RenderData;

        /// <summary>
        /// List of objects registered in this group.
        /// </summary>
        public RenderObjectCollection RenderObjects { get; }

        public VisibilityGroup(NextGenRenderSystem renderSystem)
        {
            RenderSystem = renderSystem;
            RenderObjects = new RenderObjectCollection(this);
            RenderData.Initialize(ComputeDataArrayExpectedSize);

            // Create RenderStageMask key, and keep track of number of RenderStages.Count for future resizing
            RenderStageMaskKey = RenderData.CreateStaticObjectKey<uint>(null, stageMaskMultiplier = (RenderSystem.RenderStages.Count + RenderStageMaskSizePerEntry - 1) / RenderStageMaskSizePerEntry);
            Array.Resize(ref viewRenderStageMask, stageMaskMultiplier);

            RenderSystem.RenderStages.CollectionChanged += RenderStages_CollectionChanged;
            RenderSystem.RenderStageSelectorsChanged += RenderSystem_RenderStageSelectorsChanged;
        }

        public void Dispose()
        {
            RenderSystem.RenderStageSelectorsChanged -= RenderSystem_RenderStageSelectorsChanged;
            RenderSystem.RenderStages.CollectionChanged -= RenderStages_CollectionChanged;
        }

        // TODO GRAPHICS REFACTOR not thread-safe
        public void Collect()
        {
            // Check if active render stages need reevaluation for those render objects
            ReevaluateActiveRenderStages();

            // Clear object data
            foreach (var renderObject in RenderObjects)
            {
                renderObject.ObjectNode = ObjectNodeReference.Invalid;
            }

            // Collect objects, and perform frustum culling
            // TODO GRAPHICS REFACTOR Create "VisibilityObject" (could contain multiple RenderNode) and separate frustum culling from RenderSystem
            // TODO GRAPHICS REFACTOR optimization: maybe we could process all views at once (swap loop between per object and per view)

            foreach (var view in Views)
            {
                // Prepare culling mask
                foreach (var renderViewStage in view.RenderStages)
                {
                    var renderStageIndex = renderViewStage.RenderStage.Index;
                    viewRenderStageMask[renderStageIndex / RenderStageMaskSizePerEntry] |= 1U << (renderStageIndex % RenderStageMaskSizePerEntry);
                }

                // Create the bounding frustum locally on the stack, so that frustum.Contains is performed with boundingBox that is also on the stack
                // TODO GRAPHICS REFACTOR frustum culling is currently hardcoded (cf previous TODO, we should make this more modular and move it out of here)
                var frustum = new BoundingFrustum(ref view.ViewProjection);
                var cullingMode = view.SceneCameraRenderer.CullingMode;

                // TODO GRAPHICS REFACTOR we currently forward SceneCameraRenderer.CullingMask
                // Note sure this is really a good mechanism long term (it forces to recreate multiple time the same view, instead of using RenderStage + selectors or a similar mechanism)
                // This is still supported so that existing gizmo code kept working with new graphics refactor. Might be reconsidered at some point.
                var cullingMask = view.SceneCameraRenderer.CullingMask;

                // Process objects
                foreach (var renderObject in RenderObjects)
                {
                    // Skip not enabled objects
                    if (!renderObject.Enabled || ((EntityGroupMask)(1U << (int)renderObject.RenderGroup) & cullingMask) == 0)
                        continue;

                    var renderStageMask = RenderData.GetData(RenderStageMaskKey);
                    var renderStageMaskNode = renderObject.VisibilityObjectNode * stageMaskMultiplier;

                    // Determine if this render object belongs to this view
                    bool renderStageMatch = false;
                    unsafe
                    {
                        fixed (uint* viewRenderStageMaskStart = viewRenderStageMask)
                        fixed (uint* objectRenderStageMaskStart = renderStageMask.Data)
                        {
                            var viewRenderStageMaskPtr = viewRenderStageMaskStart;
                            var objectRenderStageMaskPtr = objectRenderStageMaskStart + renderStageMaskNode.Index;
                            for (int i = 0; i < viewRenderStageMask.Length; ++i)
                            {
                                if ((*viewRenderStageMaskPtr++ & *objectRenderStageMaskPtr++) != 0)
                                {
                                    renderStageMatch = true;
                                    break;
                                }
                            }
                        }
                    }

                    // Object not part of this view because no render stages in this objects are visible in this view
                    if (!renderStageMatch)
                        continue;

                    // Fast AABB transform: http://zeuxcg.org/2010/10/17/aabb-from-obb-with-component-wise-abs/
                    // Compute transformed AABB (by world)
                    if (cullingMode == CameraCullingMode.Frustum
                        && renderObject.BoundingBox.Extent != Vector3.Zero
                        && !frustum.Contains(ref renderObject.BoundingBox))
                    {
                        continue;
                    }

                    // Add object to list of visible objects
                    // TODO GRAPHICS REFACTOR we should be able to push multiple elements with future VisibilityObject
                    // TODO GRAPHICS REFACTOR not thread-safe
                    view.RenderObjects.Add(renderObject);
                }
            }
        }

        internal void AddRenderObject(List<RenderObject> renderObjects, RenderObject renderObject)
        {
            if (renderObject.VisibilityObjectNode != StaticObjectNodeReference.Invalid)
                return;

            renderObject.VisibilityObjectNode = new StaticObjectNodeReference(renderObjects.Count);

            renderObjects.Add(renderObject);

            // Resize arrays to accomodate for new data
            RenderData.PrepareDataArrays();

            RenderSystem.AddRenderObject(renderObject);

            ReevaluateActiveRenderStages(renderObject);
        }

        internal bool RemoveRenderObject(List<RenderObject> renderObjects, RenderObject renderObject)
        {
            RenderSystem.RemoveRenderObject(renderObject);

            // Get and clear ordered node index
            var orderedRenderNodeIndex = renderObject.VisibilityObjectNode.Index;
            if (renderObject.VisibilityObjectNode == StaticObjectNodeReference.Invalid)
                return false;

            renderObject.VisibilityObjectNode = StaticObjectNodeReference.Invalid;

            // SwapRemove each items in dataArrays
            RenderData.SwapRemoveItem(DataType.StaticObject, orderedRenderNodeIndex, RenderObjects.Count - 1);

            // Remove entry from ordered node index
            renderObjects.SwapRemoveAt(orderedRenderNodeIndex);

            // If last item was moved, update its index
            if (orderedRenderNodeIndex < RenderObjects.Count)
            {
                renderObjects[orderedRenderNodeIndex].VisibilityObjectNode = new StaticObjectNodeReference(orderedRenderNodeIndex);
            }

            return true;
        }

        internal void ReevaluateActiveRenderStages(RenderObject renderObject)
        {
            var renderFeature = renderObject.RenderFeature;
            if (renderFeature == null)
                return;

            // Determine which render stages are activated for this object
            renderObject.ActiveRenderStages = new ActiveRenderStage[RenderSystem.RenderStages.Count];

            foreach (var renderStageSelector in renderFeature.RenderStageSelectors)
                renderStageSelector.Process(renderObject);

            // Compute render stage mask
            var renderStageMask = RenderData.GetData(RenderStageMaskKey);
            var renderStageMaskNode = renderObject.VisibilityObjectNode * stageMaskMultiplier;

            for (int index = 0; index < renderObject.ActiveRenderStages.Length; index++)
            {
                // TODO: Could easily be optimized to read and set value only once per uint
                var activeRenderStage = renderObject.ActiveRenderStages[index];
                if (activeRenderStage.Active)
                    renderStageMask[renderStageMaskNode + (index / RenderStageMaskSizePerEntry)] |= 1U << (index % RenderStageMaskSizePerEntry);
            }
        }

        internal void ReevaluateActiveRenderStages()
        {
            if (!NeedActiveRenderStageReevaluation)
                return;

            NeedActiveRenderStageReevaluation = false;

            foreach (var renderObject in RenderObjects)
            {
                ReevaluateActiveRenderStages(renderObject);
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

        private void RenderSystem_RenderStageSelectorsChanged()
        {
            // Everything will need reevaluation
            // TODO GRAPHICS REFACTOR optimization: only reprocess object with the given RenderFeature?
            NeedActiveRenderStageReevaluation = true;
        }

        private void RenderStages_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Make sure mask is big enough
                    RenderData.ChangeDataMultiplier(RenderStageMaskKey, stageMaskMultiplier = (RenderSystem.RenderStages.Count + RenderStageMaskSizePerEntry - 1) / RenderStageMaskSizePerEntry);
                    Array.Resize(ref viewRenderStageMask, stageMaskMultiplier);

                    // Everything will need reevaluation
                    NeedActiveRenderStageReevaluation = true;

                    break;
            }
        }
    }
}