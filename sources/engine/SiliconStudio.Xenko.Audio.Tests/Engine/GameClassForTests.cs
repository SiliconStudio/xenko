// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Audio.Tests.Engine
{
    class GameClassForTests : Game
    {
        protected bool ContentLoaded;

        protected override void Update(GameTime gameTime)
        {
            LoadContent().Wait();

            if (BeforeUpdating != null)
                BeforeUpdating(this);

            base.Update(gameTime);

            if (AfterUpdating != null)
                AfterUpdating(this);
        }

        protected override void Draw(GameTime gameTime)
        {
            LoadContent().Wait();

            if (BeforeDrawing != null)
                BeforeDrawing(this);

            base.Draw(gameTime);

            if (AfterDrawing != null)
                AfterDrawing(this);
        }

        protected override async Task LoadContent()
        {
            if (ContentLoaded)
                return;

            await base.LoadContent();

            if (LoadingContent != null)
                LoadingContent(this);

            ContentLoaded = true;
        }

        public event Action<Game> LoadingContent;
        public event Action<Game> BeforeUpdating;
        public event Action<Game> AfterUpdating;
        public event Action<Game> BeforeDrawing;
        public event Action<Game> AfterDrawing;
    }
}
