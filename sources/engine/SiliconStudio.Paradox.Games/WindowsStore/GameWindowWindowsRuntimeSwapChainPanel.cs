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
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core.Mathematics;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SiliconStudio.Paradox.Games
{
    /// <summary>
    /// An abstract window.
    /// </summary>
    internal class GameWindowWindowsRuntimeSwapChainPanel : GameWindow
    {
#region Fields

        private SwapChainPanel swapChainPanel;
        private WindowHandle windowHandle;
        private int currentWidth;
        private int currentHeight;

        #endregion

#region Public Properties

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

        public override bool IsMouseVisible { get; set; }

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

        internal override bool CanHandle(GameContext windowContext)
        {
            return windowContext.ContextType == AppContextType.WindowsRuntime;
        }

        internal override void Initialize(GameContext windowContext)
        {
            if (windowContext != null)
            {
                swapChainPanel = windowContext.Control as SwapChainPanel;
                if (swapChainPanel == null)
                {
                    throw new NotSupportedException(string.Format("Unsupported window context [{0}]. Only SwapChainPanel",  windowContext.Control.GetType().FullName));
                }
                windowHandle = new WindowHandle(AppContextType.WindowsRuntime, swapChainPanel);

                //clientBounds = new DrawingRectangle(0, 0, (int)swapChainPanel.ActualWidth, (int)swapChainPanel.ActualHeight);
                swapChainPanel.SizeChanged += swapChainPanel_SizeChanged;
                swapChainPanel.CompositionScaleChanged += swapChainPanel_CompositionScaleChanged;
            }
        }

        void swapChainPanel_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            OnClientSizeChanged(sender, EventArgs.Empty);
        }

        private void swapChainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var bounds = e.NewSize;

            // Only apply SwapChain resize when effective orientation is matching current orientation
            // TODO: This code is just a workaround. We need to improve orientation handling PDX-1140
            var effectiveOrientation = bounds.Width > bounds.Height ? ApplicationViewOrientation.Landscape : ApplicationViewOrientation.Portrait;
            var currentOrientation = ApplicationView.GetForCurrentView().Orientation;
            if (effectiveOrientation == currentOrientation
                && bounds.Width > 0 && bounds.Height > 0 && currentWidth > 0 && currentHeight > 0)
            {
                double panelWidth;
                double panelHeight;
                panelWidth = bounds.Width;
                panelHeight = bounds.Height;
                var panelRatio = panelWidth/panelHeight;
                var currentRatio = currentWidth/currentHeight;

                if (panelRatio < currentRatio)
                {
                    panelWidth = bounds.Width;
                    panelHeight = (int)(currentHeight*bounds.Width/currentWidth);
                }
                else
                {
                    panelHeight = bounds.Height;
                    panelWidth = (int)(currentWidth*bounds.Height/currentHeight);
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

            OnClientSizeChanged(sender, EventArgs.Empty);
        }

        internal override void Resize(int width, int height)
        {
            currentWidth = width;
            currentHeight = height;
        }

        internal override void Run()
        {
            // Initialize GameBase
            InitCallback();

            // Perform the rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
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