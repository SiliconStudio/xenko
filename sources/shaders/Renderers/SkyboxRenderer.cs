// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Renderers
{
    public class SkyboxRenderer : Renderer
    {
        private TextureCube skybox;

        private Effect skyboxEffect;
        
        private PostEffectQuad skyQuad;

        public SkyboxRenderer(IServiceRegistry services, TextureCube skyboxTexture)
            : base(services)
        {
            skybox = skyboxTexture;
        }

        public override void Load()
        {
            skyboxEffect = EffectSystem.LoadEffect("SkyboxShader");
            skyboxEffect.Parameters.Set(TexturingKeys.TextureCube0, skybox);
            skyQuad = new PostEffectQuad(GraphicsDevice, skyboxEffect);

            Pass.StartPass += RenderSky;
        }

        public override void Unload()
        {
            skyboxEffect.Dispose();
            skyQuad.Dispose();
        }

        private void RenderSky(RenderContext context)
        {
            skyboxEffect.Apply(context.CurrentPass.Parameters);
            skyQuad.Draw();
        }
    }
}