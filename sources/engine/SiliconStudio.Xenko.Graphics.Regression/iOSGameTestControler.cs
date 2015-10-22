// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Regression
{
    public class iOSGameTestController : ParadoxGameController
    {
        private readonly GameBase game;

        public iOSGameTestController(GameBase game)
        {
            this.game = game;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (game != null)
                game.Dispose();
        }
    }
}

#endif