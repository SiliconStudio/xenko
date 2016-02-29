using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    public class HighlightRenderFeature : SubRenderFeature
    {
        /// <summary>
        /// Gets the color to use to highlight directly referenced assets.
        /// </summary>
        /// <remarks>This color does not have premultiplied alpha.</remarks>
        public static Color4 DirectReferenceColor { get; private set; }

        /// <summary>
        /// Gets the color to use to highlight indirectly referenced assets.
        /// </summary>
        /// <remarks>This color does not have premultiplied alpha.</remarks>
        public static Color4 IndirectReferenceColor { get; private set; }

        private ConstantBufferOffsetReference color;

        private ObjectPropertyKey<Color4> renderModelObjectInfoKey;

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            DirectReferenceColor = new Color4(1.0f, 0.35f, 0.25f, 0.8f);
            IndirectReferenceColor = new Color4(1.0f, 0.65f, 0.60f, 0.8f);

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

                Color4 color = DirectReferenceColor;
                //if ()
                //{
                //    color = DirectReferenceColor;
                //}
                //else if ()
                //{
                //    color = IndirectReferenceColor;
                //}

                renderModelObjectInfo[objectNodeReference] = color;
            }
        }

        /// <inheritdoc/>
        public unsafe override void Prepare(RenderContext context)
        {
            var renderModelObjectInfoData = RootRenderFeature.RenderData.GetData(renderModelObjectInfoKey);

            foreach (var renderNode in ((RootEffectRenderFeature)RootRenderFeature).RenderNodes)
            {
                var colorOffset = renderNode.RenderEffect.Reflection.PerDrawLayout.GetConstantBufferOffset(this.color);
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