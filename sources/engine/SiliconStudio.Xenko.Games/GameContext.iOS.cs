// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using UIKit;
using OpenTK.Platform.iPhoneOS;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing <see cref="iPhoneOSGameView"/>.
    /// </summary>
    public partial class GameContextiOS : GameContext<iPhoneOSGameView>
    {
        /// <inheritDoc/> 
        public GameContextiOS(UIWindow mainWindow, iPhoneOSGameView gameView, XenkoGameController gameViewController, int requestedWidth = 0, int requestedHeight = 0)
            : base(gameView, requestedWidth, requestedHeight)
        {
            Window = mainWindow;
            GameViewController = gameViewController;
            ContextType = AppContextType.iOS;
        }

        /// <summary>
        /// The view in which is rendered the game.
        /// </summary>
        public readonly UIWindow Window;

        /// <summary>
        /// The controller of the game.
        /// </summary>
        public readonly XenkoGameController GameViewController;
    }
}
#endif
