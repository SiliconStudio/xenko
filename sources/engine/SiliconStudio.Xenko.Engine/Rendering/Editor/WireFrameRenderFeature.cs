using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    public class WireFrameRenderFeature : SubRenderFeature
    {
        private ConstantBufferOffsetReference perDrawData;

        // TODO: Make configurable (per object/view/...?)
        private struct PerDraw
        {
            public Color3 FrontColor;
            public float ColorBlend;
            public Color3 BackColor;
            public float AlphaBlend;
        }

        private readonly PerDraw perDrawValue = new PerDraw
        {
            FrontColor = (Color3)Color.FromBgra(0xFFFFDC51),
            BackColor = (Color3)Color.FromBgra(0xFFFF8300),
            ColorBlend = 0.3f,
            AlphaBlend = 0.1f
        };

        /// <inheritdoc/>
        public override void Initialize()
        {
            perDrawData = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot(MaterialFrontBackBlendShaderKeys.ColorFront.Name);
        }

        /// <inheritdoc/>
        public unsafe override void Prepare(RenderContext context)
        {
            foreach (var renderNode in ((RootEffectRenderFeature)RootRenderFeature).RenderNodes)
            {
                var pickingDataOffset = renderNode.RenderEffect.Reflection.PerDrawLayout.GetConstantBufferOffset(this.perDrawData);
                if (pickingDataOffset == -1)
                    continue;

                var mappedCB = renderNode.Resources.ConstantBuffer.Data;
                var perDraw = (PerDraw*)((byte*)mappedCB + pickingDataOffset);
                *perDraw = perDrawValue;
            }
        }
    }
}