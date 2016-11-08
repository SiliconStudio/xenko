using System.Collections.Generic;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// Render shadow maps; should be set on <see cref="ForwardLightingRenderFeature.ShadowMapRenderer"/>.
    /// </summary>
    public interface IShadowMapRenderer
    {
        RenderSystem RenderSystem { get; set; }

        RenderStage ShadowMapRenderStage { get; set; }

        void Collect(RenderContext context, Dictionary<RenderView, ForwardLightingRenderFeature.RenderViewLightData> renderViewLightDatas);

        void PrepareAtlasAsRenderTargets(CommandList commandList);

        void PrepareAtlasAsShaderResourceViews(CommandList commandList);
    }
}