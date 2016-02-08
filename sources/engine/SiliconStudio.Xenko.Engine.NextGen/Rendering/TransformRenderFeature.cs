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

        private ConstantBufferOffsetReference view;
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

            view = ((RootEffectRenderFeature)RootRenderFeature).CreateViewCBufferOffsetSlot(TransformationKeys.View.Name);
            world = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot(TransformationKeys.World.Name);
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            var renderModelObjectInfo = RootRenderFeature.GetData(renderModelObjectInfoKey);

            foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = objectNode.RenderObject as RenderMesh;
                // TODO: Extract world
                var world = (renderMesh != null) ? renderMesh.World : Matrix.Identity;

                renderModelObjectInfo[objectNodeReference] = new RenderModelFrameInfo { World = world };
            }
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public unsafe override void Prepare(RenderContext context)
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

                var mappedCB = renderNode.Resources.ConstantBuffer.Data;
                var world = (Matrix*)((byte*)mappedCB + worldOffset);
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
                    var viewProjectionOffset = viewLayout.GetConstantBufferOffset(this.view);
                    if (viewProjectionOffset == -1)
                        continue;

                    var resourceGroup = viewLayout.Entries[view.Index].Resources;
                    var mappedCB = resourceGroup.ConstantBuffer.Data;

                    var viewMatrices = (Matrix*)((byte*)mappedCB + viewProjectionOffset);

                    // View
                    *viewMatrices++ = view.View;

                    // ViewInverse
                    *viewMatrices = view.View;
                    viewMatrices->Invert();
                    viewMatrices++;

                    // Projection
                    *viewMatrices++ = view.Projection;

                    // ProjectionInverse
                    *viewMatrices = view.Projection;
                    viewMatrices->Invert();
                    viewMatrices++;

                    // ViewProjection
                    *viewMatrices = view.ViewProjection;
                }
            }
        }
    }
}