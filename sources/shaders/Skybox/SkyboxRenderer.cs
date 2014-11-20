// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Skybox
{
    /// <summary>
    /// A renderer for a skybox.
    /// </summary>
    public class SkyboxRenderer : Renderer
    {
        private readonly TextureCube skybox;

        private Effect skyboxEffect;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxRenderer"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="skyboxTexture">The skybox texture.</param>
        public SkyboxRenderer(IServiceRegistry services, TextureCube skyboxTexture)
            : base(services)
        {
            skybox = skyboxTexture;
        }

        public override void Load()
        {
            base.Load();

            skyboxEffect = EffectSystem.LoadEffect("SkyboxShader");
            skyboxEffect.Parameters.Set(TexturingKeys.TextureCube0, skybox);
        }

        public override void Unload()
        {
            base.Unload();

            skyboxEffect.Dispose();
        }

        protected override void OnRendering(RenderContext context)
        {
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.DepthRead);
            skyboxEffect.Apply(context.CurrentPass.Parameters);
            GraphicsDevice.DrawQuad();
        }
    }
}