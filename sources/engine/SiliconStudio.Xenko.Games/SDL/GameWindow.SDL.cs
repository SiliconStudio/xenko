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

#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using System.Diagnostics;
using SDL2;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.SDL;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// An abstract window.
    /// </summary>
    internal class GameWindowSDL : GameWindow<Window>
    {
        private bool isMouseVisible;

        private bool isMouseCurrentlyHidden;

        private Window window;

        private WindowHandle windowHandle;


        private bool isFullScreenMaximized;
        private FormBorderStyle savedFormBorderStyle;
        private bool oldVisible;
        private bool deviceChangeChangedVisible;
        private bool? deviceChangeWillBeFullScreen;

        private bool allowUserResizing;
        private bool isBorderLess;

        internal GameWindowSDL()
        {
        }

        public override WindowHandle NativeWindow
        {
            get
            {
                return windowHandle;
            }
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            if (willBeFullScreen && !isFullScreenMaximized && window != null)
            {
                savedFormBorderStyle = window.FormBorderStyle;
            }

            if (willBeFullScreen != isFullScreenMaximized)
            {
                deviceChangeChangedVisible = true;
                oldVisible = Visible;
                Visible = false;

                if (window != null)
                {
                    window.SendToBack();
                }
            }
            else
            {
                deviceChangeChangedVisible = false;
            }

            if (!willBeFullScreen && isFullScreenMaximized && window != null)
            {
                window.TopMost = false;
                window.FormBorderStyle = savedFormBorderStyle;
            }

            deviceChangeWillBeFullScreen = willBeFullScreen;
        }

        public override void EndScreenDeviceChange(int clientWidth, int clientHeight)
        {
            if (!deviceChangeWillBeFullScreen.HasValue)
                return;

            if (deviceChangeWillBeFullScreen.Value)
            {
                isFullScreenMaximized = true;
            }
            else if (isFullScreenMaximized)
            {
                if (window != null)
                {
                    window.BringToFront();
                }
                isFullScreenMaximized = false;
            }

            UpdateFormBorder();

            if (deviceChangeChangedVisible)
                Visible = oldVisible;

            if (window != null)
            {
                window.ClientSize = new Size2(clientWidth, clientHeight);
            }

            // Notifies the GameForm about the fullscreen state
            var gameForm = window as GameFormSDL;
            if (gameForm != null)
            {
                gameForm.IsFullScreen = isFullScreenMaximized;
            }

            deviceChangeWillBeFullScreen = null;
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Desktop doesn't have orientation (unless on Windows 8?)
        }

        protected override void Initialize(GameContext<Window> gameContext)
        {
            window = gameContext.Control;

            // Setup the initial size of the window
            var width = gameContext.RequestedWidth;
            if (width == 0)
            {
                width = window.ClientSize.Width;
            }

            var height = gameContext.RequestedHeight;
            if (height == 0)
            {
                height = window.ClientSize.Height;
            }

            windowHandle = new WindowHandle(AppContextType.Desktop, window, window.Handle);

            window.ClientSize = new Size2(width, height);

            window.MouseEnterActions +=WindowOnMouseEnterActions;   
            window.MouseLeaveActions += WindowOnMouseLeaveActions;

            var gameForm = window as GameFormSDL;
            if (gameForm != null)
            {
                //gameForm.AppActivated += OnActivated;
                //gameForm.AppDeactivated += OnDeactivated;
                gameForm.UserResized += OnClientSizeChanged;
            }
            else
            {
                window.ResizeEndActions += WindowOnResizeEndActions;
            }
        }

        internal override void Run()
        {
            Debug.Assert(InitCallback != null);
            Debug.Assert(RunCallback != null);

            // Initialize the init callback
            InitCallback();

            var runCallback = new SDLMessageLoop.RenderCallback(RunCallback);
            // Run the rendering loop
            try
            {
                SDLMessageLoop.Run(window, () =>
                {
                    if (Exiting)
                    {
                        Destroy();
                        return;
                    }

                    runCallback();
                });
            }
            finally
            {
                if (ExitCallback != null)
                {
                    ExitCallback();
                }
            }
        }

        private void WindowOnMouseEnterActions(SDL.SDL_WindowEvent sdlWindowEvent)
        {
            if (!isMouseVisible && !isMouseCurrentlyHidden)
            {
                Cursor.Hide();
                isMouseCurrentlyHidden = true;
            }
        }

        private void WindowOnMouseLeaveActions(SDL.SDL_WindowEvent sdlWindowEvent)
        {
            if (isMouseCurrentlyHidden)
            {
                Cursor.Show();
                isMouseCurrentlyHidden = false;
            }
        }

        private void WindowOnResizeEndActions(SDL.SDL_WindowEvent sdlWindowEvent)
        {
            OnClientSizeChanged(window, EventArgs.Empty);
        }

        public override bool IsMouseVisible
        {
            get
            {
                return isMouseVisible;
            }
            set
            {
                if (isMouseVisible != value)
                {
                    isMouseVisible = value;
                    if (isMouseVisible)
                    {
                        if (isMouseCurrentlyHidden)
                        {
                            Cursor.Show();
                            isMouseCurrentlyHidden = false;
                        }
                    }
                    else if (!isMouseCurrentlyHidden)
                    {
                        Cursor.Hide();
                        isMouseCurrentlyHidden = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GameWindow" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public override bool Visible
        {
            get
            {
                return window.Visible;
            }
            set
            {
                window.Visible = value;
            }
        }

        public override Int2 Position
        {
            get
            {
                if (window == null)
                    return base.Position;

                return new Int2(window.Location.X, window.Location.Y);
            }
            set
            {
                if (window != null)
                    window.Location = new Point(value.X, value.Y);

                base.Position = value;
            }
        }

        protected override void SetTitle(string title)
        {
            if (window != null)
            {
                window.Text = title;
            }
        }

        internal override void Resize(int width, int height)
        {
            window.ClientSize = new Size2(width, height);
        }

        public override bool AllowUserResizing
        {
            get
            {
                return allowUserResizing;
            }
            set
            {
                if (window != null)
                {
                    allowUserResizing = value;
                    UpdateFormBorder();
                }
            }
        }

        public override bool IsBorderLess
        {
            get
            {
                return isBorderLess;
            }
            set
            {
                if (isBorderLess != value)
                {
                    isBorderLess = value;
                    UpdateFormBorder();
                }
            }
        }

        private void UpdateFormBorder()
        {
            if (window != null)
            {
                window.MaximizeBox = allowUserResizing;
                window.FormBorderStyle = isFullScreenMaximized || isBorderLess ? FormBorderStyle.None : allowUserResizing ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;

                if (isFullScreenMaximized)
                {
                    window.TopMost = true;
                    window.BringToFront();
                }
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                // Ensure width and height are at least 1 to avoid divisions by 0
                return new Rectangle(0, 0, Math.Max(window.ClientSize.Width, 1), Math.Max(window.ClientSize.Height, 1));
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return DisplayOrientation.Default;
            }
        }

        public override bool IsMinimized
        {
            get
            {
                if (window != null)
                {
                    return window.WindowState == FormWindowState.Minimized;
                }
                // Check for non-window control
                return false;
            }
        }

        protected override void Destroy()
        {
            if (window != null)
            {
                window.Dispose();
                window = null;
            }

            base.Destroy();
        }
    }
}
#endif
