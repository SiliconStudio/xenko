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

            skyboxEffect = new ImageEffectShader(DrawEffectContext.GetShared(Services), "SkyboxEffect");
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
                // No Depthstencil tests
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

                // Copy camera/pass parameters
                context.CurrentPass.Parameters.CopySharedTo(skyboxEffect.Parameters);

                // Copy Skybox parameters
                if (skybox.Skybox != null)
                {
                    skybox.Skybox.Parameters.CopySharedTo(skyboxEffect.Parameters);
                }

                // Setup the intensity
                var intensity = skybox.Lighting.Enabled ? skybox.Lighting.Intensity : 1.0f;
                intensity *= skybox.Background.Intensity;
                skyboxEffect.Parameters.Set(SkyboxKeys.Intensity, intensity);
                    
                // Setup the rotation
                skyboxEffect.Parameters.Set(SkyboxKeys.Rotation, skybox.Lighting.Rotation);

                skyboxEffect.SetOutput(Target ?? GraphicsDevice.BackBuffer);
                skyboxEffect.Draw();

                // Restore current target
                GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, Target ?? GraphicsDevice.BackBuffer);
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.Default);
            }
        }
    }
}