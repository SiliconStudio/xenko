// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using UIKit;
using OpenTK.Platform.iPhoneOS;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering in iOS.
    /// </summary>
    public partial class GameContextiOS : GameContext<iOSWindow>
    {
        /// <inheritDoc/> 
        public GameContextiOS(iOSWindow window, int requestedWidth = 0, int requestedHeight = 0)
            : base(window, requestedWidth, requestedHeight)
        {
            ContextType = AppContextType.iOS;
        }
    }

}
#endif
