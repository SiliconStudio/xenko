// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using System;
using System.Drawing;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using UIKit;
using OpenTK.Graphics.ES30;
using OpenTK.Platform.iPhoneOS;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

using RenderbufferInternalFormat = OpenTK.Graphics.ES30.RenderbufferInternalFormat;

namespace SiliconStudio.Xenko.Starter
{
    public class XenkoApplicationDelegate : UIApplicationDelegate
    {
        /// <summary>
        /// The instance of the game to run.
        /// </summary>
	    protected Game Game;
		
		/// <summary>
		/// The main windows of the application.
		/// </summary>
        protected UIWindow MainWindow { get; private set; }
		
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
			if(Game == null)
				throw new InvalidOperationException("Please set 'Game' to a valid instance of Game before calling this method.");
				
            var bounds = UIScreen.MainScreen.Bounds;

            // create the game main windows
            MainWindow = new UIWindow(bounds);

            // create the xenko game view 
            var xenkoGameView = new iOSXenkoView((RectangleF)bounds) {ContentScaleFactor = UIScreen.MainScreen.Scale};

            // create the view controller used to display the xenko game
            var xenkoGameController = new XenkoGameController { View = xenkoGameView };

            // create the game context
            var gameContext = new GameContextiOS(new iOSWindow(MainWindow, xenkoGameView, xenkoGameController));

            // Force fullscreen
            UIApplication.SharedApplication.SetStatusBarHidden(true, false);

            // Added UINavigationController to switch between UIViewController because the game is killed if the FinishedLaunching (in the AppDelegate) method doesn't return true in 10 sec.
            var navigationController = new UINavigationController {NavigationBarHidden = true};
            navigationController.PushViewController(gameContext.Control.GameViewController, false);
            MainWindow.RootViewController = navigationController;

            // launch the main window
            MainWindow.MakeKeyAndVisible();

            // launch the game
            Game.Run(gameContext);

            return Game.IsRunning;
        }

        // note: for more information on iOS application life cycle, 
        // see http://docs.xamarin.com/guides/cross-platform/application_fundamentals/backgrounding/part_1_introduction_to_backgrounding_in_ios

        [Register("iOSXenkoView")]
        internal class iOSXenkoView : iPhoneOSGameView, IAnimatedGameView
        {
            CADisplayLink displayLink;
            private bool isRunning;

            public iOSXenkoView(RectangleF frame)
                : base(frame)
            {
            }

            protected override void CreateFrameBuffer()
            {
                base.CreateFrameBuffer();

                // TODO: PDX-364: depth format is currently hard coded (need to investigate how it can be transmitted)
                // Create a depth renderbuffer
                uint depthRenderBuffer;
                GL.GenRenderbuffers(1, out depthRenderBuffer);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderBuffer);

                // Allocate storage for the new renderbuffer
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.Depth24Stencil8, (int)(Size.Width * Layer.ContentsScale), (int)(Size.Height * Layer.ContentsScale));

                // Attach the renderbuffer to the framebuffer's depth attachment point
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderBuffer);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.StencilAttachment, RenderbufferTarget.Renderbuffer, depthRenderBuffer);
            }

            [Export("layerClass")]
            public static Class LayerClass()
            {
                return GetLayerClass();
            }

            public void StartAnimating()
            {
                if (isRunning)
                    return;

                CreateFrameBuffer();

                var displayLink = UIScreen.MainScreen.CreateDisplayLink(this, new Selector("drawFrame"));
                displayLink.FrameInterval = 0;
                displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
                this.displayLink = displayLink;

                isRunning = true;
            }

            public void StopAnimating()
            {
                if (!isRunning)
                    return;

                displayLink.Invalidate();
                displayLink = null;

                DestroyFrameBuffer();

                isRunning = false;
            }

            [Export("drawFrame")]
            void DrawFrame()
            {
                OnRenderFrame(new OpenTK.FrameEventArgs());
            }
        }
    }
}

#endif
