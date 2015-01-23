// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// A renderer for a skybox.
    /// </summary>
    public class SkyboxBackgroundRenderer : Renderer
    {
        private ImageEffectShader skyboxEffect;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxBackgroundRenderer" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public SkyboxBackgroundRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void Load()
        {
            base.Load();

            skyboxEffect = new ImageEffectShader(DrawEffectContext.GetShared(Services), "SkyboxShader");
        }

        public override void Unload()
        {
            base.Unload();

            skyboxEffect.Dispose();
        }

        protected override void OnRendering(RenderContext context)
        {
            // get the lightprocessor
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            var skyboxProcessor = entitySystem.GetProcessor<SkyboxProcessor>();
            if (skyboxProcessor == null)
                return;

            foreach (var skyboxPair in skyboxProcessor.Skyboxes)
            {
                var skybox = skyboxPair.Value;
                // Just display the first valid skybox
                if (skybox.Enabled && skybox.Skybox != null && skybox.Background.Enabled)
                {
                    GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.DepthRead);

                    // TODO: Pass parameters to skybox effect
                    skyboxEffect.Draw();
                    GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.Default);
                    break;
                }
            }
        }
    }
}