// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public class SkyboxComponentRenderer : EntityComponentRendererBase
    {
        public override void SetupPipeline(NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var mainRenderStage = GetOrCreateRenderStage(renderSystem, "Main", "Main", new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var skyboxRenderFeature = renderSystem.RenderFeatures.OfType<SkyboxRenderFeature>().FirstOrDefault();
            if (skyboxRenderFeature == null)
            {
                skyboxRenderFeature = new SkyboxRenderFeature();
                skyboxRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
                {
                    RenderStage = mainRenderStage,
                    EffectName = "SkyboxEffect",
                });

                // Register top level renderers
                // TODO GRAPHICS REFACTOR protect against multiple executions?
                renderSystem.RenderFeatures.Add(skyboxRenderFeature);
            }
        }
    }
}