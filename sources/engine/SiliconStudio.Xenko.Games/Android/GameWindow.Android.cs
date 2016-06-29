// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using System.Diagnostics;
using System.Drawing;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using Android.Views.InputMethods;
using OpenTK;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games.Android;
using SiliconStudio.Xenko.Graphics;
using Rectangle = SiliconStudio.Core.Mathematics.Rectangle;
using OpenTK.Platform.Android;
using Configuration = Android.Content.Res.Configuration;
using Android.Hardware;
using Android.Runtime;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// An abstract window.
    /// </summary>
    internal class GameWindowAndroid : GameWindow<AndroidXenkoGameView>
    {
        private AndroidXenkoGameView xenkoGameForm;
        private WindowHandle nativeWindow;

        public override WindowHandle NativeWindow => nativeWindow;

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(int clientWidth, int clientHeight)
        {

        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Desktop doesn't have orientation (unless on Windows 8?)
        }

        private Activity GetActivity()
        {
            var context = xenkoGameForm.Context;
            while (context is ContextWrapper) {
                var activity = context as Activity;
                if (activity != null) {
                    return activity;
                }
                context = ((ContextWrapper)context).BaseContext;
            }
            return null;
        }

        protected override void Initialize(GameContext<AndroidXenkoGameView> gameContext)
        {
            xenkoGameForm = gameContext.Control;
            nativeWindow = new WindowHandle(AppContextType.Android, xenkoGameForm, xenkoGameForm.Handle);

            xenkoGameForm.Load += gameForm_Resume;
            xenkoGameForm.OnPause += gameForm_OnPause;
            xenkoGameForm.Unload += gameForm_Unload;
            xenkoGameForm.RenderFrame += gameForm_RenderFrame;
            xenkoGameForm.Resize += gameForm_Resize;

            // Setup the initial size of the window
            var width = gameContext.RequestedWidth;
            if (width == 0)
            {
                width = xenkoGameForm.Width;
            }

            var height = gameContext.RequestedHeight;
            if (height == 0)
            {
                height = xenkoGameForm.Height;
            }

            // Transmit requested back buffer and depth stencil formats to OpenTK
            xenkoGameForm.RequestedBackBufferFormat = gameContext.RequestedBackBufferFormat;
            xenkoGameForm.RequestedGraphicsProfile = gameContext.RequestedGraphicsProfile;

            xenkoGameForm.Size = new Size(width, height);
        }

        private SurfaceOrientation currentOrientation;

        private void gameForm_Resize(object sender, EventArgs e)
        {
            var windowManager = xenkoGameForm.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            if (windowManager != null)
            {
                var newOrientation = windowManager.DefaultDisplay.Rotation;

                if (currentOrientation != newOrientation)
                {
                    currentOrientation = newOrientation;
                    OnOrientationChanged(this, EventArgs.Empty);
                }
            }         
        }

        void gameForm_Resume(object sender, EventArgs e)
        {
            // Call InitCallback only first time
            if (InitCallback != null)
            {
                InitCallback();
                InitCallback = null;
            }
            xenkoGameForm.Run();

            OnResume();
        }

        void gameForm_OnPause(object sender, EventArgs e)
        {
            // Hide android soft keyboard (doesn't work anymore if done during Unload)
            var inputMethodManager = (InputMethodManager)PlatformAndroid.Context.GetSystemService(Context.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(GameContext.Control.RootView.WindowToken, HideSoftInputFlags.None);
        }

        void gameForm_Unload(object sender, EventArgs e)
        {
            OnPause();
        }
        
        void gameForm_RenderFrame(object sender, OpenTK.FrameEventArgs e)
        {
            RunCallback();
        }

        internal override void Run()
        {
            Debug.Assert(InitCallback != null);
            Debug.Assert(RunCallback != null);

            if (xenkoGameForm.GraphicsContext != null)
            {
                throw new NotImplementedException("Only supports not yet initialized AndroidXenkoGameView.");
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
                return xenkoGameForm.Visible;
            }
            set
            {
                xenkoGameForm.Visible = value;
            }
        }

        protected override void SetTitle(string title)
        {
            xenkoGameForm.Title = title;
        }

        internal override void Resize(int width, int height)
        {
            xenkoGameForm.Size = new Size(width, height);
        }

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

        public override Rectangle ClientBounds => new Rectangle(0, 0, xenkoGameForm.Size.Width, xenkoGameForm.Size.Height);

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                switch (currentOrientation)
                {
                    case SurfaceOrientation.Rotation0:
                        return DisplayOrientation.Portrait;
                    case SurfaceOrientation.Rotation180:
                        return DisplayOrientation.Portrait;
                    case SurfaceOrientation.Rotation270:
                        return DisplayOrientation.LandscapeRight;
                    case SurfaceOrientation.Rotation90:
                        return DisplayOrientation.LandscapeLeft;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override bool IsMinimized => xenkoGameForm.WindowState == OpenTK.WindowState.Minimized;

        public override bool IsMouseVisible
        {
            get { return false; }
            set { }
        }

        protected override void Destroy()
        {
            if (xenkoGameForm != null)
            {
                xenkoGameForm.Load -= gameForm_Resume;
                xenkoGameForm.OnPause -= gameForm_OnPause;
                xenkoGameForm.Unload -= gameForm_Unload;
                xenkoGameForm.RenderFrame -= gameForm_RenderFrame;

                if (xenkoGameForm.GraphicsContext != null)
                {
                    xenkoGameForm.GraphicsContext.MakeCurrent(null);
                    xenkoGameForm.GraphicsContext.Dispose();
                }
                ((AndroidWindow)xenkoGameForm.WindowInfo).TerminateDisplay();
                //xenkoGameForm.Close(); // bug in xamarin
                xenkoGameForm.Holder.RemoveCallback(xenkoGameForm);
                xenkoGameForm.Dispose();
                xenkoGameForm = null;
            }

            base.Destroy();
        }
    }
}

#endif
