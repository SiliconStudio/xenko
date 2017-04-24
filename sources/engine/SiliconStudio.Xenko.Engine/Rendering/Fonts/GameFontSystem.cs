// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Rendering.Fonts
{
    /// <summary>
    /// The game system in charge of calling <see cref="FontSystem"/>.
    /// </summary>
    public class GameFontSystem : GameSystemBase
    {
        public FontSystem FontSystem { get; private set; }

        public GameFontSystem(IServiceRegistry registry)
            : base(registry)
        {
            Visible = true;
            FontSystem = new FontSystem();
            Services.AddService(typeof(FontSystem), FontSystem);
            Services.AddService(typeof(IFontFactory), FontSystem);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            FontSystem.Draw();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            FontSystem.Load(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            FontSystem.Unload();
        }
    }
}
