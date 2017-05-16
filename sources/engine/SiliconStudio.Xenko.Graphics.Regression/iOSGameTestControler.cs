// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_IOS

using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public class iOSGameTestController : XenkoGameController
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
