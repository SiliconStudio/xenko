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

using System;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Games
{
 
    /// <summary>
    /// Contains context used to render the game (Control for WinForm, a DrawingSurface for WP8...etc.).
    /// </summary>
    public abstract class GameContext
    {
        /// <summary>
        /// Context type of this instance.
        /// </summary>
        public AppContextType ContextType { get; protected set; }

        // TODO: remove these requested values.

        /// <summary>
        /// The requested width.
        /// </summary>
        internal int RequestedWidth;

        /// <summary>
        /// The requested height.
        /// </summary>
        internal int RequestedHeight;

        /// <summary>
        /// The requested back buffer format.
        /// </summary>
        internal PixelFormat RequestedBackBufferFormat;

        /// <summary>
        /// The requested depth stencil format.
        /// </summary>
        internal PixelFormat RequestedDepthStencilFormat;

        /// <summary>
        /// THe requested graphics profiles.
        /// </summary>
        internal GraphicsProfile[] RequestedGraphicsProfile;

        /// <summary>
        /// Indicate whether the game must initialize the default database when it starts running.
        /// </summary>
        public bool InitializeDatabase = true;
    }

    /// <summary>
    /// Generic version of <see cref="GameContext"/>. The later is used to describe a generic game Context.
    /// This version enables us to constraint the game context to a specifc toolkit and ensures a better cohesion
    /// between the various toolkit specific classes, such as InputManager, GameWindow.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    public abstract class GameContext<TK> : GameContext
    {
        /// <summary>
        /// Underlying control associated with context.
        /// </summary>
        public TK Control { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext" /> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="requestedWidth">Width of the requested.</param>
        /// <param name="requestedHeight">Height of the requested.</param>
        protected GameContext(TK control, int requestedWidth = 0, int requestedHeight = 0)
        {
            Control = control;
            RequestedWidth = requestedWidth;
            RequestedHeight = requestedHeight;
        }
    }
}