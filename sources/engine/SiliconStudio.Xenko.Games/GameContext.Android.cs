// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using Android.Widget;
using OpenTK.Platform.Android;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing WinForm <see cref="Control"/>.
    /// </summary>
    public partial class GameContext 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class with null control and edit text layout.
        /// </summary>
        public GameContext() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="editTextLayout">The layout to use in order to display android <see cref="EditText"/></param>
        /// <param name="requestedWidth">Width of the requested.</param>
        /// <param name="requestedHeight">Height of the requested.</param>
        public GameContext(AndroidGameView control, RelativeLayout editTextLayout, int requestedWidth = 0, int requestedHeight = 0)
        {
            Control = control;
            EditTextLayout = editTextLayout;
            RequestedWidth = requestedWidth;
            RequestedHeight = requestedHeight;
            ContextType = AppContextType.Android;
        }

        /// <summary>
        /// The control used as a GameWindow context.
        /// </summary>
        public readonly AndroidGameView Control;

        /// <summary>
        /// The layout used to add the <see cref="EditText"/>s.
        /// </summary>
        public readonly RelativeLayout EditTextLayout;
    }
}
#endif