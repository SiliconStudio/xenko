// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Background
{
    public class BackgroundPipelineRenderer : IPipelineRenderer
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var mainRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Main", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var backgroundFeature = renderSystem.RenderFeatures.OfType<BackgroundRenderFeature>().FirstOrDefault();
            if (backgroundFeature == null)
            {
                backgroundFeature = new BackgroundRenderFeature();
                backgroundFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
                {
                    RenderStage = mainRenderStage,
                    EffectName = "Test",
                });

                // Register top level renderers
                // TODO GRAPHICS REFACTOR protect against multiple executions?
                renderSystem.RenderFeatures.Add(backgroundFeature);
            }

            // Attach processor
            //var sceneInstance = SceneInstance.GetCurrent(Context);
            //sceneInstance.Processors.Add(new NextGenBackgroundProcessor());
        }
    }
}