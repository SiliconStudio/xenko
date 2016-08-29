// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Audio.Tests.Engine
{
    class GameClassForTests : Game
    {
        protected bool ContentLoaded;

        protected override void Update(GameTime gameTime)
        {
            LoadContent().Wait();

            BeforeUpdating?.Invoke(this);

            base.Update(gameTime);

            AfterUpdating?.Invoke(this);
        }

        protected override void Draw(GameTime gameTime)
        {
            LoadContent().Wait();

            BeforeDrawing?.Invoke(this);

            base.Draw(gameTime);

            AfterDrawing?.Invoke(this);
        }

        protected override async Task LoadContent()
        {
            if (ContentLoaded)
                return;

            await base.LoadContent();

            LoadingContent?.Invoke(this);

            ContentLoaded = true;
        }

        public event Action<Game> LoadingContent;
        public event Action<Game> BeforeUpdating;
        public event Action<Game> AfterUpdating;
        public event Action<Game> BeforeDrawing;
        public event Action<Game> AfterDrawing;
    }
}
