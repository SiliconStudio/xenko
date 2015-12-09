// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using OpenTK.Platform.iPhoneOS;
using UIKit;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// Tuple of 3 elements that an iOS GameContext needs to hold on.
    /// </summary>
    public struct iOSWindow {
    
        /// <summary>
        /// Initializes current struct with a UIWindow <paramref name="w"/>, a GameView <paramref name="g"/>and a controller <paramref name="c"/>.
        /// </summary>
        public iOSWindow(UIWindow w, iPhoneOSGameView g, XenkoGameController c)
        {
            MainWindow = w;
            GameView = g;
            GameViewController = c;
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
        public readonly XenkoGameController GameViewController;
    }
}
#endif