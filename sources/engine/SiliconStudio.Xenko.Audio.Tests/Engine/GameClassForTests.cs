// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Audio.Tests.Engine
{
    internal class GameClassForTests : GameTestBase
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
