// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Class LightForwardRenderProcessor.
    /// </summary>
    public class LightModelRendererForward : LightModelRendererBase
    {
        private ShadowMapRenderer shadowRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightModelRendererForward" /> class.
        /// </summary>
        public LightModelRendererForward()
        {
            RegisterLightGroupProcessor<LightDirectional>(new LightDirectionalGroupRenderer());
            RegisterLightGroupProcessor<LightSkybox>(new LightSkyboxRenderer());
            RegisterLightGroupProcessor<LightAmbient>(new LightAmbientRenderer());
        }

        protected override void DrawCore(RenderContext context)
        {
            var sceneCamera = context.Tags.Get(SceneEntityRenderer.Current) as SceneCameraRenderer;
            if (sceneCamera != null)
            {
                var modelRenderer = ModelComponentRenderer.GetAttached(sceneCamera);
                if (modelRenderer != null)
                {
                    if (shadowRenderer == null)
                    {
                        // TODO: this is temporary, we need to have a pluggable renderer for shadow maps
                        shadowRenderer = new ShadowMapRenderer(modelRenderer.EffectName);
                        shadowRenderer.Attach(modelRenderer);
                    }

                    // TODO: this is temporary, we need to have a pluggable renderer for shadow maps
                    shadowRenderer.Draw(context);
                }
            }
            base.PrepareLights(context);
        }
    }
}
