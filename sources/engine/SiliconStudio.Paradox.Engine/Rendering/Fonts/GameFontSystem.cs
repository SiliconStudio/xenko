// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics.Font;

namespace SiliconStudio.Paradox.Rendering.Fonts
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