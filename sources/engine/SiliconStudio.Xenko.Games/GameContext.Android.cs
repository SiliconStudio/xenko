// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_ANDROID
using Android.Widget;
using OpenTK.Platform.Android;
using SiliconStudio.Xenko.Games.Android;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing WinForm <see cref="Control"/>.
    /// </summary>
    public partial class GameContextAndroid : GameContext<AndroidXenkoGameView>
    {
        /// <inheritDoc/>
        public GameContextAndroid(AndroidXenkoGameView control, RelativeLayout editTextLayout, int requestedWidth = 0, int requestedHeight = 0)
            : base(control, requestedWidth, requestedHeight)
        {
            EditTextLayout = editTextLayout;
            ContextType = AppContextType.Android;
        }

        /// <summary>
        /// The layout used to add the <see cref="EditText"/>s.
        /// </summary>
        public readonly RelativeLayout EditTextLayout;
    }
}
#endif
