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
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Core;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// An abstract window.
    /// </summary>
    internal class GameWindowWindowsRuntimeSwapChainPanel : GameWindow<SwapChainPanel>
    {
#region Fields
        private const DisplayOrientations PortraitOrientations = DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;
        private const DisplayOrientations LandscapeOrientations = DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped;

        private SwapChainPanel swapChainPanel;
        private WindowHandle windowHandle;
        private int currentWidth;
        private int currentHeight;
        private readonly CoreWindow coreWindow;
        private static readonly Windows.Devices.Input.MouseCapabilities mouseCapabilities = new Windows.Devices.Input.MouseCapabilities();
        private readonly DispatcherTimer resizeTimer;
        private double requiredRatio;
        private ApplicationView applicationView;
        private bool canResize;
#endregion

#region Public Properties

        public GameWindowWindowsRuntimeSwapChainPanel()
        {
            coreWindow = CoreWindow.GetForCurrentThread();
            resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            resizeTimer.Tick += ResizeTimerOnTick;
        } 

        public override bool AllowUserResizing
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                return new Rectangle(0, 0, (int)(this.swapChainPanel.ActualWidth * swapChainPanel.CompositionScaleX + 0.5f), (int)(this.swapChainPanel.ActualHeight * swapChainPanel.CompositionScaleY + 0.5f));
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
                return false;
            }
        }

        private bool isMouseVisible;
        private CoreCursor cursor;

        public override bool IsMouseVisible
        {
            get
            {
                return isMouseVisible;
            }
            set
            {
                if (isMouseVisible == value)
                    return;

                if (mouseCapabilities.MousePresent == 0)
                    return;

                if (value)
                {
                    if (cursor != null)
                    {
                        coreWindow.PointerCursor = cursor;
                    }

                    isMouseVisible = true;
                }
                else
                {
                    if (coreWindow.PointerCursor != null)
                    {
                        cursor = coreWindow.PointerCursor;
                    }

                    //yep thats how you hide the cursor under WinRT api...
                    coreWindow.PointerCursor = null;
                    isMouseVisible = false;
                }
            }
        }

        public override WindowHandle NativeWindow
        {
            get
            {
                return windowHandle;
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
                return true;
            }
            set
            {
            }
        }

        /// <inheritdoc/>
        public override bool IsBorderLess
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

#endregion

#region Public Methods and Operators

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(int clientWidth, int clientHeight)
        {
        }

#endregion

#region Methods

        protected override void Initialize(GameContext<SwapChainPanel> windowContext)
        {
            Debug.Assert(windowContext is GameContextWindowsRuntime, "By design only one descendant of GameContext<SwapChainPanel>");
            swapChainPanel = windowContext.Control;
            windowHandle = new WindowHandle(AppContextType.WindowsRuntime, swapChainPanel, IntPtr.Zero);

#if SILICONSTUDIO_PLATFORM_WINDOWS_10
            applicationView = ApplicationView.GetForCurrentView();            
            if (applicationView != null && windowContext.RequestedWidth != 0 && windowContext.RequestedHeight != 0)
            {
                applicationView.SetPreferredMinSize(new Size(windowContext.RequestedWidth, windowContext.RequestedHeight));
                canResize = applicationView.TryResizeView(new Size(windowContext.RequestedWidth, windowContext.RequestedHeight));
            }
#endif

            requiredRatio = windowContext.RequestedWidth/(double)windowContext.RequestedHeight;

            swapChainPanel.SizeChanged += swapChainPanel_SizeChanged;
            swapChainPanel.CompositionScaleChanged += swapChainPanel_CompositionScaleChanged;
            coreWindow.SizeChanged += CurrentWindowOnSizeChanged;
        }

        private void CurrentWindowOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            var newBounds = windowSizeChangedEventArgs.Size;
            HandleSizeChanged(sender, newBounds);
        }

        void swapChainPanel_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            OnClientSizeChanged(sender, EventArgs.Empty);
        }

        private void ResizeTimerOnTick(object sender, object o)
        {
            resizeTimer.Stop();
            OnClientSizeChanged(sender, EventArgs.Empty);
        }

        private void HandleSizeChanged(object sender, Size newSize)
        {
            var bounds = newSize;

            if (bounds.Width > 0 && bounds.Height > 0 && currentWidth > 0 && currentHeight > 0)
            {
                double panelWidth;
                double panelHeight;
                panelWidth = bounds.Width;
                panelHeight = bounds.Height;

                if (canResize)
                {
                    if (swapChainPanel.Width != panelWidth || swapChainPanel.Height != panelHeight)
                    {
                        // Center the panel
                        swapChainPanel.HorizontalAlignment = HorizontalAlignment.Center;
                        swapChainPanel.VerticalAlignment = VerticalAlignment.Center;

                        swapChainPanel.Width = panelWidth;
                        swapChainPanel.Height = panelHeight;
                    }
                }
                else
                {
                    //mobile device, keep aspect fine
                    var aspect = panelWidth/panelHeight;
                    if (aspect < requiredRatio)
                    {
                        panelWidth = bounds.Width; //real screen width
                        panelHeight = panelWidth / requiredRatio;
                    }
                    else
                    {
                        panelHeight = bounds.Height;
                        panelWidth = panelHeight * requiredRatio;
                    }

                    if (swapChainPanel.Width != panelWidth || swapChainPanel.Height != panelHeight)
                    {
                        // Center the panel
                        swapChainPanel.HorizontalAlignment = HorizontalAlignment.Center;
                        swapChainPanel.VerticalAlignment = VerticalAlignment.Center;

                        swapChainPanel.Width = panelWidth;
                        swapChainPanel.Height = panelHeight;
                    }
                }
            }

            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void swapChainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var bounds = e.NewSize;
            HandleSizeChanged(sender, bounds);
        }

        internal override void Resize(int width, int height)
        {
            currentWidth = width;
            currentHeight = height;
        }

        internal override void Run()
        {
            // Perform the rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            // Call InitCallback only first time
            if (InitCallback != null)
            {
                InitCallback();
                InitCallback = null;
            }
            RunCallback();
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Desktop doesn't have orientation (unless on Windows 8?)
        }

        protected override void SetTitle(string title)
        {

        }

        protected override void Destroy()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            base.Destroy();
        }
#endregion
    }
}

#endif
