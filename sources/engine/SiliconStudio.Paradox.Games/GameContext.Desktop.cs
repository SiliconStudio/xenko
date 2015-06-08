// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D
using System;
using System.Windows.Forms;
using SiliconStudio.Paradox.Games.Resources;

namespace SiliconStudio.Paradox.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing WinForm <see cref="Control"/>.
    /// </summary>
    public partial class GameContext 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class with a default <see cref="GameForm"/>.
        /// </summary>
        public GameContext() : this((Control)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="isUserManagingRun">if set to <c>true</c>, the user will have to manage the rendering loop by calling <see cref="Run"/>.</param>
        public GameContext(Control control, bool isUserManagingRun)
        {
            Control = control ?? CreateForm();
            IsUserManagingRun = isUserManagingRun;
            ContextType = AppContextType.Desktop;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="requestedWidth">Width of the requested.</param>
        /// <param name="requestedHeight">Height of the requested.</param>
        public GameContext(Control control, int requestedWidth = 0, int requestedHeight = 0)
        {
            Control = control ?? CreateForm();
            RequestedWidth = requestedWidth;
            RequestedHeight = requestedHeight;
            ContextType = AppContextType.Desktop;
        }

        /// <summary>
        /// The control used as a GameWindow context (either an instance of <see cref="System.Windows.Forms.Control"/> or <see cref="System.Windows.Controls.Control"/>.
        /// </summary>
        public readonly object Control;

        /// <summary>
        /// The is running delegate
        /// </summary>
        public readonly bool IsUserManagingRun;

        /// <summary>
        /// Gets the run loop to be called when <see cref="IsUserManagingRun"/> is true.
        /// </summary>
        /// <value>The run loop.</value>
        public Action RunCallback { get; internal set; }

        /// <summary>
        /// Gets the exit callback to be called when <see cref="IsUserManagingRun"/> is true when exiting the game.
        /// </summary>
        /// <value>The run loop.</value>
        public Action ExitCallback { get; internal set; }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Control"/> to <see cref="GameContext"/>.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator GameContext(Control control)
        {
            return new GameContext(control);
        }

        private static Form CreateForm()
        {
            return new GameForm();
        }
    }
}
#endif