// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Renderers
{
    public class PostProcessRenderer : Renderer
    {
        private string effectName;

        private Texture[] inputTextures;

        private Effect effect;

        private PostEffectQuad quad;

        public PostProcessRenderer(IServiceRegistry services, string postProcessEffectName, params Texture[] textures)
            : base(services)
        {
            effectName = postProcessEffectName;
            inputTextures = textures;
        }

        public override void Load()
        {
            // TODO: share mesh across post process renderers
            // TODO: post processes might need more than one texture

            effect = EffectSystem.LoadEffect(effectName);
            quad = new PostEffectQuad(GraphicsDevice, effect);

            if (effect != null)
                Pass.StartPass += ProcessEffect;
        }

        public override void Unload()
        {
            Pass.StartPass -= ProcessEffect;
            inputTextures = null;
        }

        private void ProcessEffect(RenderContext context)
        {
            quad.Draw(inputTextures);
        }
    }
}