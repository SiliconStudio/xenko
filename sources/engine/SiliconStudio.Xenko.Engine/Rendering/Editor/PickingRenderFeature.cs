using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Utils;

namespace SiliconStudio.Xenko.Rendering
{
    public class PickingRenderFeature : SubRenderFeature
    {
        private ObjectPropertyKey<RenderObjectInfo> renderObjectInfoKey;

        private ConstantBufferOffsetReference pickingData;

        struct RenderObjectInfo
        {
            public int ModelComponentId;

            public int MeshId;

            public int MaterialId;
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            renderObjectInfoKey = RootRenderFeature.RenderData.CreateObjectKey<RenderObjectInfo>();

            pickingData = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot(PickingShaderKeys.PickingData.Name);
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            var modelObjectInfo = RootRenderFeature.RenderData.GetData(renderObjectInfoKey);

            foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;

                int meshIndex = 0;
                for (int i = 0; i < renderMesh.RenderModel.ModelComponent.Model.Meshes.Count; i++)
                {
                    if (renderMesh.RenderModel.ModelComponent.Model.Meshes[i] == renderMesh.Mesh)
                    {
                        meshIndex = i;
                        break;
                    }
                }

                modelObjectInfo[objectNodeReference] = new RenderObjectInfo
                {
                    ModelComponentId = RuntimeIdHelper.ToRuntimeId(renderMesh.RenderModel.ModelComponent),
                    MeshId = meshIndex,
                    MaterialId = renderMesh.Mesh.MaterialIndex
                };
            }
        }

        /// <inheritdoc/>
        public unsafe override void Prepare(RenderThreadContext context)
        {
            var renderObjectInfo = RootRenderFeature.RenderData.GetData(renderObjectInfoKey);

            foreach (var renderNode in ((RootEffectRenderFeature)RootRenderFeature).RenderNodes)
            {
                var perDrawLayout = renderNode.RenderEffect.Reflection.PerDrawLayout;
                if (perDrawLayout == null)
                    continue;

                var pickingDataOffset = perDrawLayout.GetConstantBufferOffset(this.pickingData);
                if (pickingDataOffset == -1)
                    continue;

                var renderModelObjectInfo = renderObjectInfo[renderNode.RenderObject.ObjectNode];

                var mappedCB = renderNode.Resources.ConstantBuffer.Data;
                var pickingData = (int*)((byte*)mappedCB + pickingDataOffset);

                *pickingData++ = renderModelObjectInfo.ModelComponentId;
                *pickingData++ = renderModelObjectInfo.MeshId;
                *pickingData = renderModelObjectInfo.MaterialId;
            }
        }
    }
}