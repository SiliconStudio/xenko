using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public class WireFrameRenderFeature : SubRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private ConstantBufferOffsetReference perDrawData;

        // TODO: Make configurable (per object/view/...?)
        private struct PerDraw
        {
            public Color3 FrontColor;
            public float ColorBlend;
            public Color3 BackColor;
            public float AlphaBlend;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            perDrawData = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawCBufferOffsetSlot(MaterialFrontBackBlendShaderKeys.ColorFront.Name);

            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;
        }

        public override void PrepareEffectPermutations()
        {
            var renderEffects = RootRenderFeature.RenderData.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            foreach (var renderObject in RootRenderFeature.RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    renderEffect.EffectValidator.ValidateParameter(MaterialFrontBackBlendShaderKeys.UseNormalBackFace, true);
                }
            }
        }

        /// <inheritdoc/>
        public unsafe override void Prepare(RenderContext context)
        {
            var perDrawValue = new PerDraw
            {
                FrontColor = ((Color3)Color.FromBgra(0xFFFFDC51)).ToColorSpace(context.GraphicsDevice.ColorSpace),
                BackColor = ((Color3)Color.FromBgra(0xFFFF8300)).ToColorSpace(context.GraphicsDevice.ColorSpace),
                ColorBlend = 0.3f,
                AlphaBlend = 0.1f
            };

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