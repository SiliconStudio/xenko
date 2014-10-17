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
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

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
        public GameContext() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="requestedWidth">Width of the requested.</param>
        /// <param name="requestedHeight">Height of the requested.</param>
        public GameContext(OpenTK.GameWindow control, int requestedWidth = 0, int requestedHeight = 0)
        {
            var creationFlags = GraphicsContextFlags.Default;
            int versionMajor, versionMinor;

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            versionMajor = 2;
            versionMinor = 0;
            OpenTK.Platform.Utilities.ForceEmbedded = true;
            creationFlags |= GraphicsContextFlags.Embedded;
#else
            versionMajor = 4;
            versionMinor = 2;
#endif

            if (requestedWidth == 0 || requestedHeight == 0)
            {
                requestedWidth = 1280;
                requestedHeight = 720;
            }

            // force the stencil buffer to be not null.
            var defaultMode = GraphicsMode.Default;
            var graphicMode = new GraphicsMode(defaultMode.ColorFormat, defaultMode.Depth, 8, defaultMode.Samples, defaultMode.AccumulatorFormat, defaultMode.Buffers, defaultMode.Stereo);
            
            GraphicsContext.ShareContexts = true;
            Control = control ?? new OpenTK.GameWindow(requestedWidth, requestedHeight, graphicMode, "Paradox Game", GameWindowFlags.Default, DisplayDevice.Default, versionMajor, versionMinor, creationFlags);
            RequestedWidth = requestedWidth;
            RequestedHeight = requestedHeight;
            ContextType = AppContextType.DesktopOpenTK;
        }

        /// <summary>
        /// The control used as a GameWindow context (either an instance of <see cref="System.Windows.Forms.Control"/> or <see cref="System.Windows.Controls.Control"/>.
        /// </summary>
        public readonly object Control;

        /// <summary>
        /// Performs an implicit conversion from <see cref="Control"/> to <see cref="GameContext"/>.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator GameContext(OpenTK.GameWindow gameWindow)
        {
            return new GameContext(gameWindow);
        }
    }
}
#endif