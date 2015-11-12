// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using UIKit;
using OpenTK.Platform.iPhoneOS;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing WinForm <see cref="GameView"/>.
    /// </summary>
    public partial class GameContextiOS : GameContext<UIWindow>
    {
        /// <inheritDoc/> 
        public GameContextiOS(UIWindow mainWindows, iPhoneOSGameView gameView, XenkoGameController gameViewController, int requestedWidth = 0, int requestedHeight = 0)
            : base(mainWindows, requestedWidth, requestedHeight)
        {
            GameView = gameView;
            GameViewController = gameViewController;
            ContextType = AppContextType.iOS;
        }

        /// <summary>
        /// The view in which is rendered the game.
        /// </summary>
        public readonly iPhoneOSGameView GameView;

        /// <summary>
        /// The controller of the game.
        /// </summary>
        public readonly XenkoGameController GameViewController;
    }
}
#endif
