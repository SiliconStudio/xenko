// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using System.Drawing;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using OpenTK.Graphics.ES30;
using OpenTK.Platform.iPhoneOS;
using SiliconStudio.Xenko.Games;
using UIKit;

namespace SiliconStudio.Xenko.Starter
{
    // note: for more information on iOS application life cycle, 
    // see http://docs.xamarin.com/guides/cross-platform/application_fundamentals/backgrounding/part_1_introduction_to_backgrounding_in_ios
    [Register("iOSXenkoView")]
    public class iOSXenkoView : iPhoneOSGameView, IAnimatedGameView
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
#endif
