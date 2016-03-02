using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class HighlightRenderFeature : SubRenderFeature
    {
        public static readonly Dictionary<Material, Color4> MaterialHighlightColors = new Dictionary<Material, Color4>();

        public static readonly Dictionary<Mesh, Color4> MeshHighlightColors = new Dictionary<Mesh, Color4>();

        public static readonly Dictionary<ModelComponent, Color4> ModelHighlightColors = new Dictionary<ModelComponent, Color4>();

        public static readonly HashSet<Material> MaterialsHighlightedForModel = new HashSet<Material>();

        private ConstantBufferOffsetReference color;

        private ObjectPropertyKey<Color4> renderModelObjectInfoKey;

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            renderModelObjectInfoKey = RootRenderFeature.RenderData.CreateObjectKey<Color4>();

            color = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot(HighlightShaderKeys.HighlightColor.Name);
        }

        public override void Extract()
        {
            var renderModelObjectInfo = RootRenderFeature.RenderData.GetData(renderModelObjectInfoKey);

            foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;

                Color4 highlightColor;

                var isHighlighted =
                    MaterialHighlightColors.TryGetValue(renderMesh.Material, out highlightColor) ||
                    MeshHighlightColors.TryGetValue(renderMesh.Mesh, out highlightColor) ||
                    MaterialsHighlightedForModel.Contains(renderMesh.Material) && ModelHighlightColors.TryGetValue(renderMesh.RenderModel.ModelComponent, out highlightColor);

                renderModelObjectInfo[objectNodeReference] = highlightColor;
            }
        }

        /// <inheritdoc/>
        public unsafe override void Prepare(RenderThreadContext context)
        {
            var renderModelObjectInfoData = RootRenderFeature.RenderData.GetData(renderModelObjectInfoKey);

            foreach (var renderNode in ((RootEffectRenderFeature)RootRenderFeature).RenderNodes)
            {
                var perDrawLayout = renderNode.RenderEffect.Reflection.PerDrawLayout;
                if (perDrawLayout == null)
                    continue;

                var colorOffset = perDrawLayout.GetConstantBufferOffset(this.color);
                if (colorOffset == -1)
                    continue;

                var renderModelObjectInfo = renderModelObjectInfoData[renderNode.RenderObject.ObjectNode];

                var mappedCB = renderNode.Resources.ConstantBuffer.Data;
                var perDraw = (Color4*)((byte*)mappedCB + colorOffset);
                *perDraw = renderModelObjectInfo;
            }
        }
    }
}