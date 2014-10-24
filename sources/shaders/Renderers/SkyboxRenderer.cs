// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Renderers
{
    public class SkyboxRenderer : Renderer
    {
        private Texture skybox;
        public SkyboxRenderer(IServiceRegistry services, Texture skyboxTexture)
            : base(services)
        {
            skybox = skyboxTexture;
        }

        public override void Load()
        {
            throw new System.NotImplementedException();
        }

        public override void Unload()
        {
            throw new System.NotImplementedException();
        }
    }
}