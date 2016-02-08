using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A top-level renderer that work on a specific kind of RenderObject. Example: Mesh, Particle, Sprite, etc...
    /// </summary>
    public abstract partial class RootRenderFeature : RenderFeature
    {
        // Index that will be used for collections such as RenderView.RenderNodes and RenderView.ViewObjectNodes
        internal int Index;

        /// <summary>
        /// List of <see cref="RenderObject"/> initialized with this root render feature.
        /// </summary>
        public List<RenderObject> RenderObjects = new List<RenderObject>(); // Note: user should add/remove with RenderObjectSet

        // Nodes to process this frame
        internal List<ObjectNodeReference> ObjectNodeReferences { get; } = new List<ObjectNodeReference>();

        // Nodes for temporary storage
        internal List<RenderNode> renderNodes = new List<RenderNode>();
        private List<ViewObjectNode> viewObjectNodes = new List<ViewObjectNode>();
        private List<ObjectNode> objectNodes = new List<ObjectNode>();

        // storage for properties (struct of arrays)
        private Dictionary<object, int> dataArraysByDefinition = new Dictionary<object, int>();
        private FastListStruct<DataArray> dataArrays = new FastListStruct<DataArray>(8);

        public Action<RenderObject> ComputeRenderStages;

        /// <summary>
        /// Decide whether a <see cref="RenderObject"/> is supported by this <see cref="RootRenderFeature"/>.
        /// </summary>
        /// <param name="renderObject">The <see cref="RenderObject"/> to test.</param>
        /// <returns>True if this type of object is supported, false otherwise.</returns>
        public abstract bool SupportsRenderObject(RenderObject renderObject);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RenderNode GetRenderNode(RenderNodeReference reference)
        {
            return renderNodes[reference.Index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ViewObjectNode GetViewObjectNode(ViewObjectNodeReference reference)
        {
            return viewObjectNodes[reference.Index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ObjectNode GetObjectNode(ObjectNodeReference reference)
        {
            return objectNodes[reference.Index];
        }

        internal RenderNodeReference CreateRenderNode(RenderObject renderObject, RenderView renderView, ViewObjectNodeReference renderPerViewNode, RenderStage renderStage)
        {
            var renderNode = new RenderNode(renderObject, renderView, renderPerViewNode, renderStage);

            // Create view node
            var result = new RenderNodeReference(renderNodes.Count);
            renderNodes.Add(renderNode);
            return result;
        }

        internal ViewObjectNodeReference CreateViewObjectNode(RenderView view, RenderObject renderObject)
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

        protected virtual void OnAddRenderObject(RenderObject renderObject)
        {
            
        }

        protected virtual void OnRemoveRenderObject(RenderObject renderObject)
        {

        }

        internal void AddRenderObject(NextGenRenderSystem renderSystem, RenderObject renderObject)
        {
            renderObject.RenderFeature = this;

            // Generate static data ID
            renderObject.StaticObjectNode = new StaticObjectNodeReference(RenderObjects.Count);

            // Determine which render stages are activated for this object
            renderObject.ActiveRenderStages = new ActiveRenderStage[renderSystem.RenderStages.Count];
            ComputeRenderStages?.Invoke(renderObject);

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

            // TODO: SwapRemove each items in dataArrays (using Array.Copy and Array.Clear?)
            for (int i = 0; i < dataArrays.Count; ++i)
            {
                var dataArray = dataArrays[i];
                RemoveRenderObjectFromDataArray(dataArray, orderedRenderNodeIndex);
            }

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

        protected virtual void RemoveRenderObjectFromDataArray(DataArray dataArray, int removedIndex)
        {
            if (dataArray.Info.Type == DataType.StaticObject)
            {
                // SwapRemove StaticObject info for this object
                dataArray.Info.SwapRemoveItems(dataArray.Array, removedIndex, RenderObjects.Count - 1, 1);
            }
        }

        public virtual void Reset()
        {
            // Clear nodes
            viewObjectNodes.Clear();
            objectNodes.Clear();
            ObjectNodeReferences.Clear();
            renderNodes.Clear();
        }

        public void PrepareDataArrays()
        {
            for (int i = 0; i < dataArrays.Count; ++i)
            {
                var dataArrayInfo = dataArrays[i].Info;
                var expectedSize = ComputeDataArrayExpectedSize(dataArrayInfo.Type);

                dataArrayInfo.EnsureSize(ref dataArrays.Items[i].Array, expectedSize);
            }
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
                    return renderNodes.Count;
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