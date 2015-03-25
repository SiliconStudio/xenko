// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Shadows;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Class LightForwardRenderProcessor.
    /// </summary>
    public class LightModelRendererForward : LightModelRendererBase
    {
        private readonly ShadowMapRenderer shadowRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightModelRendererForward"/> class.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        public LightModelRendererForward(ModelComponentRenderer modelRenderer)
            : base(modelRenderer)
        {
            RegisterLightGroupProcessor<LightDirectional>(new LightDirectionalGroupRenderer());
            RegisterLightGroupProcessor<LightSkybox>(new LightSkyboxRenderer());
            RegisterLightGroupProcessor<LightAmbient>(new LightAmbientRenderer());

            // TODO: this is temporary, we need to have a pluggable renderer for shadow maps
            shadowRenderer = new ShadowMapRenderer(modelRenderer.EffectName);
            shadowRenderer.Attach(modelRenderer);
        }

        public override void PrepareLights(RenderContext context)
        {
            base.PrepareLights(context);

            // TODO: this is temporary, we need to have a pluggable renderer for shadow maps
            shadowRenderer.Draw(context);
        }
    }
}
