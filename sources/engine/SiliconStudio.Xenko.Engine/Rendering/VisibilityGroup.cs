using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a group of visible <see cref="RenderObject"/>.
    /// </summary>
    public class VisibilityGroup : IDisposable
    {
        private readonly List<RenderObject> renderObjectsWithoutFeatures = new List<RenderObject>();

        [Obsolete("This field is provisional and will be replaced by a proper mechanisms in the future")]
        public static readonly List<Func<RenderView, RenderObject, bool>> ViewObjectFilters = new List<Func<RenderView, RenderObject, bool>>();

        private int stageMaskMultiplier;

        // TODO GRAPHICS REFACTOR not thread-safe
        private uint[] viewRenderStageMask;

        internal bool NeedActiveRenderStageReevaluation;
        internal bool DisableCulling;

        public readonly StaticObjectPropertyKey<uint> RenderStageMaskKey;
        public const int RenderStageMaskSizePerEntry = 32; // 32 bits per uint

        public RenderSystem RenderSystem { get; }

        /// <summary>
        /// Stores render data.
        /// </summary>
        public RenderDataHolder RenderData;

        /// <summary>
        /// List of objects registered in this group.
        /// </summary>
        public RenderObjectCollection RenderObjects { get; }

        public VisibilityGroup(RenderSystem renderSystem)
        {
            RenderSystem = renderSystem;
            RenderObjects = new RenderObjectCollection(this);
            RenderData.Initialize(ComputeDataArrayExpectedSize);

            // Create RenderStageMask key, and keep track of number of RenderStages.Count for future resizing
            RenderStageMaskKey = RenderData.CreateStaticObjectKey<uint>(null, stageMaskMultiplier = (RenderSystem.RenderStages.Count + RenderStageMaskSizePerEntry - 1) / RenderStageMaskSizePerEntry);
            Array.Resize(ref viewRenderStageMask, stageMaskMultiplier);

            RenderSystem.RenderStages.CollectionChanged += RenderStages_CollectionChanged;
            RenderSystem.RenderStageSelectorsChanged += RenderSystem_RenderStageSelectorsChanged;
            RenderSystem.RenderFeatures.CollectionChanged += RenderFeatures_CollectionChanged;
        }

        public void Dispose()
        {
            RenderSystem.RenderStageSelectorsChanged -= RenderSystem_RenderStageSelectorsChanged;
            RenderSystem.RenderStages.CollectionChanged -= RenderStages_CollectionChanged;
        }

        public void Reset()
        {
            // Clear object data
            foreach (var renderObject in RenderObjects)
            {
                renderObject.ObjectNode = ObjectNodeReference.Invalid;
            }
        }

        // TODO GRAPHICS REFACTOR not thread-safe
        public void Collect(RenderView view)
        {
            ReevaluateActiveRenderStages();

            // Collect objects, and perform frustum culling
            // TODO GRAPHICS REFACTOR Create "VisibilityObject" (could contain multiple RenderNode) and separate frustum culling from RenderSystem
            // TODO GRAPHICS REFACTOR optimization: maybe we could process all views at once (swap loop between per object and per view)

            // View bounds calculation
            view.MinimumDistance = float.PositiveInfinity;
            view.MaximumDistance = float.NegativeInfinity;

            Matrix viewInverse = view.View;
            viewInverse.Invert();
            var plane = new Plane(viewInverse.Forward, Vector3.Dot(viewInverse.TranslationVector, viewInverse.Forward)); // TODO: Point-normal-constructor seems wrong. Check.

            // TODO: This should be configured by the creator of the view. E.g. near clipping can be enabled for spot light shadows.
            var ignoreDepthPlanes = view is ShadowMapRenderView;

            // Prepare culling mask
            foreach (var renderViewStage in view.RenderStages)
            {
                var renderStageIndex = renderViewStage.RenderStage.Index;
                viewRenderStageMask[renderStageIndex / RenderStageMaskSizePerEntry] |= 1U << (renderStageIndex % RenderStageMaskSizePerEntry);
            }

            // Create the bounding frustum locally on the stack, so that frustum.Contains is performed with boundingBox that is also on the stack
            // TODO GRAPHICS REFACTOR frustum culling is currently hardcoded (cf previous TODO, we should make this more modular and move it out of here)
            var frustum = new BoundingFrustum(ref view.ViewProjection);
            var cullingMode = DisableCulling ? CameraCullingMode.None : view.CullingMode;

            // TODO GRAPHICS REFACTOR we currently forward SceneCameraRenderer.CullingMask
            // Note sure this is really a good mechanism long term (it forces to recreate multiple time the same view, instead of using RenderStage + selectors or a similar mechanism)
            // This is still supported so that existing gizmo code kept working with new graphics refactor. Might be reconsidered at some point.
            var cullingMask = view.CullingMask;

            // Process objects
            foreach (var renderObject in RenderObjects)
            {
                // Skip not enabled objects
                if (!renderObject.Enabled || ((EntityGroupMask)(1U << (int)renderObject.RenderGroup) & cullingMask) == 0)
                    continue;

                // Custom per-view filtering
                bool skip = false;
                lock (ViewObjectFilters)
                {
                    // TODO HACK First filter with global static filters
                    foreach (var filter in ViewObjectFilters)
                    {
                        if (!filter(view, renderObject))
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (skip)
                        continue;
                }

                // TODO HACK Then filter with RenderSystem filters
                foreach (var filter in RenderSystem.ViewObjectFilters)
                {
                    if (!filter(view, renderObject))
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip)
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
                    && !FrustumContainsBox(ref frustum, ref renderObject.BoundingBox, ignoreDepthPlanes))
                {
                    continue;
                }

                // Add object to list of visible objects
                // TODO GRAPHICS REFACTOR we should be able to push multiple elements with future VisibilityObject
                // TODO GRAPHICS REFACTOR not thread-safe
                view.RenderObjects.Add(renderObject);

                // Calculate bounding box of all render objects in the view
                if (renderObject.BoundingBox.Extent != Vector3.Zero)
                {
                    CalculateMinMaxDistance(ref plane, ref renderObject.BoundingBox, ref view.MinimumDistance, ref view.MaximumDistance);
                }
            }
        }

        private static bool FrustumContainsBox(ref BoundingFrustum frustum, ref BoundingBoxExt boundingBoxExt, bool ignoreDepthPlanes)
        {
            unsafe
            {
                fixed (Plane* planeStart = &frustum.LeftPlane)
                {
                    var plane = planeStart;
                    for (int i = 0; i < 6; ++i)
                    {
                        if (ignoreDepthPlanes && i > 3)
                            continue;

                        // Previous code:
                        if (Vector3.Dot(boundingBoxExt.Center, plane->Normal)
                            + boundingBoxExt.Extent.X * Math.Abs(plane->Normal.X)
                            + boundingBoxExt.Extent.Y * Math.Abs(plane->Normal.Y)
                            + boundingBoxExt.Extent.Z * Math.Abs(plane->Normal.Z)
                            <= -plane->D)
                            return false;
                        plane++;
                    }
                }

                return true;
            }
        }

        private static void CalculateMinMaxDistance(ref Plane plane, ref BoundingBoxExt boundingBox, ref float minDistance, ref float maxDistance)
        {
            var nearCorner = boundingBox.Minimum;
            var farCorner = boundingBox.Maximum;

            if (plane.Normal.X < 0)
                Utilities.Swap(ref nearCorner.X, ref farCorner.X);

            if (plane.Normal.Y < 0)
                Utilities.Swap(ref nearCorner.Y, ref farCorner.Y);

            if (plane.Normal.Z < 0)
                Utilities.Swap(ref nearCorner.Z, ref farCorner.Z);

            var distance = CollisionHelper.DistancePlanePoint(ref plane, ref nearCorner);
            if (minDistance > distance)
                minDistance = distance;

            distance = CollisionHelper.DistancePlanePoint(ref plane, ref farCorner);
            if (maxDistance < distance)
                maxDistance = distance;
        }

        private static void MinMax(float distance, ref float min, ref float max)
        {
            if (distance < min)
                min = distance;
            if (distance > max)
                max = distance;
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
            if (renderObject.RenderFeature != null)
                ReevaluateActiveRenderStages(renderObject);
            else
                renderObjectsWithoutFeatures.Add(renderObject);
        }

        internal bool RemoveRenderObject(List<RenderObject> renderObjects, RenderObject renderObject)
        {
            if (renderObject.RenderFeature == null)
                renderObjectsWithoutFeatures.Remove(renderObject);

            RenderSystem.RemoveRenderObject(renderObject);

            // Get and clear ordered node index
            var orderedRenderNodeIndex = renderObject.VisibilityObjectNode.Index;
            if (renderObject.VisibilityObjectNode == StaticObjectNodeReference.Invalid)
                return false;

            renderObject.VisibilityObjectNode = StaticObjectNodeReference.Invalid;

            // SwapRemove each items in dataArrays
            RenderData.SwapRemoveItem(DataType.StaticObject, orderedRenderNodeIndex, renderObjects.Count - 1);

            // Remove entry from ordered node index
            renderObjects.SwapRemoveAt(orderedRenderNodeIndex);

            // If last item was moved, update its index
            if (orderedRenderNodeIndex < renderObjects.Count)
            {
                renderObjects[orderedRenderNodeIndex].VisibilityObjectNode = new StaticObjectNodeReference(orderedRenderNodeIndex);
            }

            return true;
        }

        private void ReevaluateActiveRenderStages(RenderObject renderObject)
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

        private void ReevaluateActiveRenderStages()
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

        private void RenderFeatures_CollectionChanged(object sender, ref FastTrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int index = 0; index < renderObjectsWithoutFeatures.Count; index++)
                    {
                        var renderObject = renderObjectsWithoutFeatures[index];

                        // Try to reprocess object that didn't have any stage before
                        if (renderObject.RenderFeature == null)
                        {
                            RenderSystem.AddRenderObject(renderObject);
                            if (renderObject.RenderFeature != null)
                            {
                                renderObjectsWithoutFeatures.SwapRemoveAt(index--);
                                ReevaluateActiveRenderStages(renderObject);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var renderObject in RenderObjects)
                    {
                        if (renderObject.RenderFeature == e.Item)
                        {
                            RenderSystem.RemoveRenderObject(renderObject);
                            renderObjectsWithoutFeatures.Add(renderObject);
                        }
                    }
                    break;
            }
        }

        private void RenderStages_CollectionChanged(object sender, ref FastTrackingCollectionChangedEventArgs e)
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