// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A top-level renderer that work on a specific kind of <see cref="RenderObject"/>, such as Mesh, Particle, Sprite, etc...
    /// </summary>
    public abstract class RootRenderFeature : RenderFeature
    {
        private List<ViewObjectNode> viewObjectNodes = new List<ViewObjectNode>();
        private List<ObjectNode> objectNodes = new List<ObjectNode>();

        // storage for properties (struct of arrays)
        public RenderDataHolder RenderData;

        // Index that will be used for collections such as RenderView.RenderNodes and RenderView.ViewObjectNodes
        public int Index { get; internal set; }

        /// <summary>
        /// Sort key used during rendering.
        /// </summary>
        public byte SortKey { get; protected set; } = 128;

        /// <summary>
        /// List of <see cref="RenderObject"/> initialized with this root render feature.
        /// </summary>
        public List<RenderObject> RenderObjects = new List<RenderObject>();

        /// <summary>
        /// Object nodes to process this frame.
        /// </summary>
        public List<ObjectNodeReference> ObjectNodeReferences { get; } = new List<ObjectNodeReference>();

        /// <summary>
        /// List of render nodes for this specific root render feature.
        /// </summary>
        public List<RenderNode> RenderNodes { get; } = new List<RenderNode>();

        /// <summary>
        /// Overrides that allow defining which render stages are enabled for a specific <see cref="RenderObject"/>.
        /// </summary>
        public FastTrackingCollection<RenderStageSelector> RenderStageSelectors { get; } = new FastTrackingCollection<RenderStageSelector>();

        /// <summary>
        /// Gets the type of render object supported by this <see cref="RootRenderFeature"/>.
        /// </summary>
        public abstract Type SupportedRenderObjectType { get; }

        protected RootRenderFeature()
        {
            RenderData.Initialize(ComputeDataArrayExpectedSize);
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            RenderData.Clear();

            base.Unload();
        }

        /// <summary>
        /// Gets the render node from its reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns>The render node.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderNode GetRenderNode(RenderNodeReference reference)
        {
            return RenderNodes[reference.Index];
        }

        /// <summary>
        /// Gets the view object node from its reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns>The view object node.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ViewObjectNode GetViewObjectNode(ViewObjectNodeReference reference)
        {
            return viewObjectNodes[reference.Index];
        }

        /// <summary>
        /// Gets the object node from its reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns>The object node.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectNode GetObjectNode(ObjectNodeReference reference)
        {
            return objectNodes[reference.Index];
        }

        internal RenderNodeReference CreateRenderNode(RenderObject renderObject, RenderView renderView, ViewObjectNodeReference renderPerViewNode, RenderStage renderStage)
        {
            var renderNode = new RenderNode(renderObject, renderView, renderPerViewNode, renderStage);

            // Create view node
            var result = new RenderNodeReference(RenderNodes.Count);
            RenderNodes.Add(renderNode);
            return result;
        }

        /// <summary>
        /// Creates a view object node during Extract phase.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="renderObject"></param>
        /// <returns>The view object node reference.</returns>
        public ViewObjectNodeReference CreateViewObjectNode(RenderView view, RenderObject renderObject)
        {
            var renderViewNode = new ViewObjectNode(renderObject, view, renderObject.ObjectNode);

            // Create view node
            var result = new ViewObjectNodeReference(viewObjectNodes.Count);
            viewObjectNodes.Add(renderViewNode);
            return result;
        }

        internal ObjectNodeReference GetOrCreateObjectNode(RenderObject renderObject)
        {
            if (renderObject.ObjectNode == ObjectNodeReference.Invalid)
            {

                renderObject.ObjectNode = new ObjectNodeReference(objectNodes.Count);
                objectNodes.Add(new ObjectNode(renderObject));

                ObjectNodeReferences.Add(renderObject.ObjectNode);
            }

            return renderObject.ObjectNode;
        }

        /// <summary>
        /// Called when a render object is added.
        /// </summary>
        /// <param name="renderObject"></param>
        protected virtual void OnAddRenderObject(RenderObject renderObject)
        {
            
        }

        /// <summary>
        /// Called when a render object is removed.
        /// </summary>
        /// <param name="renderObject"></param>
        protected virtual void OnRemoveRenderObject(RenderObject renderObject)
        {

        }

        internal void AddRenderObject(RenderObject renderObject)
        {
            renderObject.RenderFeature = this;

            // Generate static data ID
            renderObject.StaticObjectNode = new StaticObjectNodeReference(RenderObjects.Count);

            // Add to render object
            RenderObjects.Add(renderObject);

            OnAddRenderObject(renderObject);
        }

        internal void RemoveRenderObject(RenderObject renderObject)
        {
            OnRemoveRenderObject(renderObject);

            // Get and clear ordered node index
            var orderedRenderNodeIndex = renderObject.StaticObjectNode.Index;
            renderObject.StaticObjectNode = StaticObjectNodeReference.Invalid;

            // SwapRemove each items in dataArrays
            RenderData.SwapRemoveItem(DataType.StaticObject, orderedRenderNodeIndex, RenderObjects.Count - 1);

            // Remove entry from ordered node index
            RenderObjects.SwapRemoveAt(orderedRenderNodeIndex);

            // If last item was moved, update its index
            if (orderedRenderNodeIndex < RenderObjects.Count)
            {
                RenderObjects[orderedRenderNodeIndex].StaticObjectNode = new StaticObjectNodeReference(orderedRenderNodeIndex);
            }

            // Detach render feature
            renderObject.RenderFeature = null;
        }

        public virtual void Reset()
        {
            // Clear nodes
            viewObjectNodes.Clear();
            objectNodes.Clear();
            ObjectNodeReferences.Clear();
            RenderNodes.Clear();
        }

        public void PrepareDataArrays()
        {
            RenderData.PrepareDataArrays();
        }

        protected virtual int ComputeDataArrayExpectedSize(DataType type)
        {
            switch (type)
            {
                case DataType.ViewObject:
                    return viewObjectNodes.Count;
                case DataType.Object:
                    return objectNodes.Count;
                case DataType.Render:
                    return RenderNodes.Count;
                case DataType.View:
                    return RenderSystem.Views.Count;
                case DataType.StaticObject:
                    return RenderObjects.Count;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}