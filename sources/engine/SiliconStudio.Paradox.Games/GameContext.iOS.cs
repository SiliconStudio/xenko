// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using UIKit;
using OpenTK.Platform.iPhoneOS;

namespace SiliconStudio.Paradox.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing WinForm <see cref="GameView"/>.
    /// </summary>
    public partial class GameContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class with null <see cref="MainWindow"/>, <see cref="GameView"/> and <see cref="GameViewController"/>.
        /// </summary>
        public GameContext()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class.
        /// </summary>
        /// <param name="mainWindows">The main windows of the game</param>
        /// <param name="gameView">The view in which the game is rendered</param>
        /// <param name="gameViewController">The paradox game main controller</param>
        /// <param name="requestedWidth">Width of the requested.</param>
        /// <param name="requestedHeight">Height of the requested.</param>
        public GameContext(UIWindow mainWindows, iPhoneOSGameView gameView, ParadoxGameController gameViewController, int requestedWidth = 0, int requestedHeight = 0)
        {
            MainWindow = mainWindows;
            GameView = gameView;
            GameViewController = gameViewController;
            RequestedWidth = requestedWidth;
            RequestedHeight = requestedHeight;
            ContextType = AppContextType.iOS;
        }

        /// <summary>
        /// The main window of the game.
        /// </summary>
        public readonly UIWindow MainWindow;

        /// <summary>
        /// The view in which is rendered the game.
        /// </summary>
        public readonly iPhoneOSGameView GameView;

        /// <summary>
        /// The controller of the game.
        /// </summary>
        public readonly ParadoxGameController GameViewController;
    }
}
#endif