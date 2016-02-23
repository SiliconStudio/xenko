// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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

            var skyboxRenderFeature = new SkyboxRenderFeature();
            skyboxRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                RenderStage = mainRenderStage,
                EffectName = "SkyboxEffect",
            });

            // Register top level renderers
            renderSystem.RenderFeatures.Add(skyboxRenderFeature);

            // Attach processor
            var sceneInstance = SceneInstance.GetCurrent(Context);
            sceneInstance.Processors.Add(new NextGenSkyboxProcessor());
        }
    }
}