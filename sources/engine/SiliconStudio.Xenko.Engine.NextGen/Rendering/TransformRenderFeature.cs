using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Computes and upload World, View and Projection matrices for each views and for each objects.
    /// </summary>
    public class TransformRenderFeature : SubRenderFeature
    {
        private ObjectPropertyKey<RenderModelFrameInfo> renderModelObjectInfoKey;
        private ViewObjectPropertyKey<RenderModelViewInfo> renderModelViewInfoKey;

        private ConstantBufferOffsetReference viewProjection;
        private ConstantBufferOffsetReference world;

        struct RenderModelFrameInfo
        {
            // Copied during Extract
            public Matrix World;
        }

        struct RenderModelViewInfo
        {
            // Copied during Extract
            public Matrix WorldViewProjection, WorldView;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            renderModelObjectInfoKey = RootRenderFeature.CreateObjectKey<RenderModelFrameInfo>();
            renderModelViewInfoKey = RootRenderFeature.CreateViewObjectKey<RenderModelViewInfo>();

            viewProjection = ((RootEffectRenderFeature)RootRenderFeature).CreateViewCBufferOffsetSlot("Transformation.ViewProjection");
            world = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot("Transformation.World");
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            var renderModelObjectInfo = RootRenderFeature.GetData(renderModelObjectInfoKey);

            foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;
                // TODO: Extract world
                var world = renderMesh.World;

                renderModelObjectInfo[objectNodeReference] = new RenderModelFrameInfo { World = world };
            }
        }

        /// <inheritdoc/>
        public unsafe override void Prepare()
        {
            // Compute WorldView, WorldViewProj
            var renderModelObjectInfoData = RootRenderFeature.GetData(renderModelObjectInfoKey);
            var renderModelViewInfoData = RootRenderFeature.GetData(renderModelViewInfoKey);

            // Copy Entity.World to PerDraw cbuffer
            // TODO: Have a PerObject cbuffer?
            foreach (var renderNode in ((RootEffectRenderFeature)RootRenderFeature).renderNodes)
            {
                var worldOffset = renderNode.RenderEffect.Reflection.PerDrawLayout.GetConstantBufferOffset(this.world);
                if (worldOffset == -1)
                    continue;

                var renderModelObjectInfo = renderModelObjectInfoData[renderNode.RenderObject.ObjectNode];

                var mappedCB = RenderSystem.BufferPool.Buffer.Data + renderNode.DrawConstantBufferOffset;
                var world = (Matrix*)((byte*)mappedCB);
                *world++ = renderModelObjectInfo.World; // World
                *world = renderModelObjectInfo.World; // WorldInverseTranspose
                world->Transpose();
                world->Invert();
            }

            for (int index = 0; index < RenderSystem.Views.Count; index++)
            {
                var view = RenderSystem.Views[index];
                var viewFeature = view.Features[RootRenderFeature.Index];

                Matrix.Multiply(ref view.View, ref view.Projection, out view.ViewProjection);

                // Compute WorldView and WorldViewProjection
                foreach (var renderPerViewNodeReference in viewFeature.ViewObjectNodes)
                {
                    var renderPerViewNode = RootRenderFeature.GetViewObjectNode(renderPerViewNodeReference);
                    var renderModelFrameInfo = renderModelObjectInfoData[renderPerViewNode.ObjectNode];

                    var renderModelViewInfo = new RenderModelViewInfo();
                    Matrix.Multiply(ref renderModelFrameInfo.World, ref view.View, out renderModelViewInfo.WorldView);
                    Matrix.Multiply(ref renderModelFrameInfo.World, ref view.ViewProjection,
                        out renderModelViewInfo.WorldViewProjection);

                    // TODO: Use ref locals or Utilities instead, to avoid double copy
                    renderModelViewInfoData[renderPerViewNodeReference] = renderModelViewInfo;

                    // TODO: Upload to constant buffer
                }

                // Copy ViewProjection to PerFrame cbuffer
                foreach (var viewLayout in viewFeature.Layouts)
                {
                    var viewProjectionOffset = viewLayout.GetConstantBufferOffset(this.viewProjection);
                    if (viewProjectionOffset == -1)
                        continue;

                    var resourceGroup = viewLayout.Entries[view.Index].ResourceGroup;
                    var mappedCB = RenderSystem.BufferPool.Buffer.Data + resourceGroup.ConstantBufferOffset;

                    var viewProjection = (Matrix*)((byte*)mappedCB + viewProjectionOffset);
                    *viewProjection = view.ViewProjection;
                }
            }
        }
    }
}