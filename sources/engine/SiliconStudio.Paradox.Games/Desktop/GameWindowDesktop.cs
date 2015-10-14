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
using System.Diagnostics;
#if !SILICONSTUDIO_RUNTIME_CORECLR
using System.Drawing;
using System.Windows.Forms;
#else
using SharpDX.Windows;
#endif
using System.Threading;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
#if !SILICONSTUDIO_RUNTIME_CORECLR
using Point = System.Drawing.Point;
#else
using Point = WCL.Structs.Point;
#endif

namespace SiliconStudio.Paradox.Games
{
    /// <summary>
    /// An abstract window.
    /// </summary>
    internal class GameWindowDesktop : GameWindow
    {
        private bool isMouseVisible;

        private bool isMouseCurrentlyHidden;

        public Control Control;

        private WindowHandle windowHandle;

#if !SILICONSTUDIO_RUNTIME_CORECLR
        private Form form;
#else
        private Control form;
#endif

        private bool isFullScreenMaximized;
#if !SILICONSTUDIO_RUNTIME_CORECLR
        private FormBorderStyle savedFormBorderStyle;
#endif
        private bool oldVisible;
        private bool deviceChangeChangedVisible;
        private bool? deviceChangeWillBeFullScreen;

        private bool allowUserResizing;
        private bool isBorderLess;

        internal GameWindowDesktop()
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
#if !SILICONSTUDIO_RUNTIME_CORECLR
            if (willBeFullScreen && !isFullScreenMaximized && form != null)
            {
                savedFormBorderStyle = form.FormBorderStyle;
            }
#endif

            if (willBeFullScreen != isFullScreenMaximized)
            {
                deviceChangeChangedVisible = true;
                oldVisible = Visible;
                Visible = false;

#if !SILICONSTUDIO_RUNTIME_CORECLR
                if (form != null)
                    form.SendToBack();
#endif
            }
            else
            {
                deviceChangeChangedVisible = false;
            }

#if !SILICONSTUDIO_RUNTIME_CORECLR
            if (!willBeFullScreen && isFullScreenMaximized && form != null)
            {
                form.TopMost = false;
                form.FormBorderStyle = savedFormBorderStyle;
            }
#endif

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
#if !SILICONSTUDIO_RUNTIME_CORECLR
                if (form != null)
                {
                    form.BringToFront();
                }
#endif
                isFullScreenMaximized = false;
            }

            UpdateFormBorder();

            if (deviceChangeChangedVisible)
                Visible = oldVisible;

#if !SILICONSTUDIO_RUNTIME_CORECLR
            if (form != null)
            {
                form.ClientSize = new Size(clientWidth, clientHeight);
            }

            // Notifies the GameForm about the fullscreen state
            var gameForm = form as GameForm;
            if (gameForm != null)
            {
                gameForm.IsFullScreen = isFullScreenMaximized;
            }
#endif

            deviceChangeWillBeFullScreen = null;
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Desktop doesn't have orientation (unless on Windows 8?)
        }

        internal override bool CanHandle(GameContext gameContext)
        {
            return gameContext.ContextType == AppContextType.Desktop;
        }

        internal override void Initialize(GameContext gameContext)
        {
            this.GameContext = gameContext;

            Control = (Control)gameContext.Control;

#if !SILICONSTUDIO_RUNTIME_CORECLR
            // Setup the initial size of the window
            var width = gameContext.RequestedWidth;
            if (width == 0)
            {
                width = Control is Form ? GraphicsDeviceManager.DefaultBackBufferWidth : Control.ClientSize.Width;
            }

            var height = gameContext.RequestedHeight;
            if (height == 0)
            {
                height = Control is Form ? GraphicsDeviceManager.DefaultBackBufferHeight : Control.ClientSize.Height;
            }
#endif

            windowHandle = new WindowHandle(AppContextType.Desktop, Control);

#if !SILICONSTUDIO_RUNTIME_CORECLR
            Control.ClientSize = new System.Drawing.Size(width, height);

            Control.MouseEnter += GameWindowForm_MouseEnter;
            Control.MouseLeave += GameWindowForm_MouseLeave;

            form = Control as Form;
            var gameForm = Control as GameForm;
            if (gameForm != null)
            {
                //gameForm.AppActivated += OnActivated;
                //gameForm.AppDeactivated += OnDeactivated;
                gameForm.UserResized += OnClientSizeChanged;
            }
            else
            {
                Control.Resize += OnClientSizeChanged;
            }
#endif
        }

        internal override void Run()
        {
            Debug.Assert(InitCallback != null);
            Debug.Assert(RunCallback != null);

            // Initialize the init callback
            InitCallback();

            if (GameContext.IsUserManagingRun)
            {
                GameContext.RunCallback = RunCallback;
                GameContext.ExitCallback = ExitCallback;
            }
            else
            {
                var runCallback = new WindowsMessageLoop.RenderCallback(RunCallback);
                // Run the rendering loop
                try
                {
                    WindowsMessageLoop.Run(Control, () =>
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
        }

        private void GameWindowForm_MouseEnter(object sender, System.EventArgs e)
        {
            if (!isMouseVisible && !isMouseCurrentlyHidden)
            {
#if !SILICONSTUDIO_RUNTIME_CORECLR
                Cursor.Hide();
#endif
                isMouseCurrentlyHidden = true;
            }
        }

        private void GameWindowForm_MouseLeave(object sender, System.EventArgs e)
        {
            if (isMouseCurrentlyHidden)
            {
#if !SILICONSTUDIO_RUNTIME_CORECLR
                Cursor.Show();
#endif
                isMouseCurrentlyHidden = false;
            }
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
#if !SILICONSTUDIO_RUNTIME_CORECLR
                            Cursor.Show();
#endif
                            isMouseCurrentlyHidden = false;
                        }
                    }
                    else if (!isMouseCurrentlyHidden)
                    {
#if !SILICONSTUDIO_RUNTIME_CORECLR
                        Cursor.Hide();
#endif
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
#if !SILICONSTUDIO_RUNTIME_CORECLR
                return Control.Visible;
#else
                return true;
#endif
            }
            set
            {
#if !SILICONSTUDIO_RUNTIME_CORECLR
                Control.Visible = value;
#endif
            }
        }

        public override Int2 Position
        {
            get
            {
                if (Control == null)
                    return base.Position;

#if !SILICONSTUDIO_RUNTIME_CORECLR
                return new Int2(Control.Location.X, Control.Location.Y);
#else
                return new Int2(10, 10);
#endif
            }
            set
            {
#if !SILICONSTUDIO_RUNTIME_CORECLR
                if (Control != null)
                    Control.Location = new Point(value.X, value.Y);
#endif

                base.Position = value;
            }
        }

        protected override void SetTitle(string title)
        {
#if !SILICONSTUDIO_RUNTIME_CORECLR
            if (form != null)
            {
                form.Text = title;
            }
#endif
        }

        internal override void Resize(int width, int height)
        {
#if !SILICONSTUDIO_RUNTIME_CORECLR
            Control.ClientSize = new Size(width, height);
#endif
        }

        public override bool AllowUserResizing
        {
            get
            {
                return allowUserResizing;
            }
            set
            {
                if (form != null)
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
#if !SILICONSTUDIO_RUNTIME_CORECLR
            if (form != null)
            {
                form.MaximizeBox = allowUserResizing;
                form.FormBorderStyle = isFullScreenMaximized || isBorderLess ? FormBorderStyle.None : allowUserResizing ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;

                if (isFullScreenMaximized)
                {
                    form.TopMost = true;
                    form.BringToFront();
                }
            }
#endif
        }

        public override SiliconStudio.Core.Mathematics.Rectangle ClientBounds
        {
            get
            {
                // Ensure width and height are at least 1 to avoid divisions by 0
#if !SILICONSTUDIO_RUNTIME_CORECLR
                return new SiliconStudio.Core.Mathematics.Rectangle(0, 0, Math.Max(Control.ClientSize.Width, 1), Math.Max(Control.ClientSize.Height, 1));
#else
                return new SiliconStudio.Core.Mathematics.Rectangle(0, 0, 500, 500);
#endif
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
#if !SILICONSTUDIO_RUNTIME_CORECLR
                if (form != null)
                {
                    return form.WindowState == FormWindowState.Minimized;
                }
#endif
                // Check for non-form control
                return false;
            }
        }

        protected override void Destroy()
        {
            if (Control != null)
            {
#if !SILICONSTUDIO_RUNTIME_CORECLR
                Control.Dispose();
#endif
                Control = null;
            }

            base.Destroy();
        }
    }
}
#endif