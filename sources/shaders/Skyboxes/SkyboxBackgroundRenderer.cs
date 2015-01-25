// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// A renderer for a skybox.
    /// </summary>
    public class SkyboxBackgroundRenderer : Renderer
    {
        private ImageEffectShader skyboxEffect;
        private readonly SkyboxProcessor skyboxProcessor;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxBackgroundRenderer" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public SkyboxBackgroundRenderer(IServiceRegistry services)
            : base(services)
        {
            skyboxProcessor = EntitySystem.GetProcessor<SkyboxProcessor>();
        }

        public Texture Target { get; set; }

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
            if (skyboxProcessor == null)
            {
                return;
            }

            var skybox = skyboxProcessor.ActiveSkyboxBackground;

            if (skybox != null)
            {
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.DepthRead);

                context.CurrentPass.Parameters.CopySharedTo(skyboxEffect.Parameters);

                //if (skybox.Skybox != null && skybox.Skybox.Parameters.ContainsKey(TexturingKeys.TextureCube0))
                if (skybox.SkyboxTexture != null)
                {
                    //skyboxEffect.SetInput(skybox.Skybox.Parameters.Get(TexturingKeys.TextureCube0));
                    skyboxEffect.SetInput(skybox.SkyboxTexture);
                }

                skyboxEffect.SetOutput(Target ?? GraphicsDevice.BackBuffer);
                skyboxEffect.Draw();

                // Restore current target
                GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, Target ?? GraphicsDevice.BackBuffer);
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.Default);
            }
        }
    }
}