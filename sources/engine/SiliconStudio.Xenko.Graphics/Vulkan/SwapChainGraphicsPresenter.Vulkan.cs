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

#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using SharpVulkan;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Graphics presenter for SwapChain.
    /// </summary>
    public class SwapChainGraphicsPresenter : GraphicsPresenter
    {
        private Swapchain swapChain;
        private Surface surface;

        private Texture backbuffer;
        private Texture[] buffers;
        private uint currentBufferIndex;

        public SwapChainGraphicsPresenter(GraphicsDevice device, PresentationParameters presentationParameters)
            : base(device, presentationParameters)
        {
            PresentInterval = presentationParameters.PresentationInterval;

            // Initialize the swap chain
            swapChain = CreateSwapChain();

        }

        public override Texture BackBuffer
        {
            get
            {
                return backbuffer;
            }
        }

        public override object NativePresenter
        {
            get
            {
                return swapChain;
            }
        }

        public override bool IsFullScreen
        {
            get
            {
                //return swapChain.IsFullScreen;
                return false;
            }

            set
            {
//#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
//                if (swapChain == null)
//                    return;

//                var outputIndex = Description.PreferredFullScreenOutputIndex;

//                // no outputs connected to the current graphics adapter
//                var output = GraphicsDevice.Adapter != null && outputIndex < GraphicsDevice.Adapter.Outputs.Length ? GraphicsDevice.Adapter.Outputs[outputIndex] : null;

//                Output currentOutput = null;

//                try
//                {
//                    RawBool isCurrentlyFullscreen;
//                    swapChain.GetFullscreenState(out isCurrentlyFullscreen, out currentOutput);

//                    // check if the current fullscreen monitor is the same as new one
//                    if (isCurrentlyFullscreen == value && output != null && currentOutput != null && currentOutput.NativePointer == output.NativeOutput.NativePointer)
//                        return;
//                }
//                finally
//                {
//                    if (currentOutput != null)
//                        currentOutput.Dispose();
//                }

//                bool switchToFullScreen = value;
//                // If going to fullscreen mode: call 1) SwapChain.ResizeTarget 2) SwapChain.IsFullScreen
//                var description = new ModeDescription(backBuffer.ViewWidth, backBuffer.ViewHeight, Description.RefreshRate.ToSharpDX(), (SharpDX.DXGI.Format)Description.BackBufferFormat);
//                if (switchToFullScreen)
//                {
//                    // Force render target destruction
//                    // TODO: We should track all user created render targets that points to back buffer as well (or deny their creation?)
//                    backBuffer.OnDestroyed();

//                    OnDestroyed();

//                    Description.IsFullScreen = true;

//                    OnRecreated();

//                    // Recreate render target
//                    backBuffer.OnRecreate();
//                }
//                else
//                {
//                    Description.IsFullScreen = false;
//                    swapChain.IsFullScreen = false;

//                    // call 1) SwapChain.IsFullScreen 2) SwapChain.Resize
//                    Resize(backBuffer.ViewWidth, backBuffer.ViewHeight, backBuffer.ViewFormat);
//                }

//                // If going to window mode: 
//                if (!switchToFullScreen)
//                {
//                    // call 1) SwapChain.IsFullScreen 2) SwapChain.Resize
//                    description.RefreshRate = new SharpDX.DXGI.Rational(0, 0);
//                    swapChain.ResizeTarget(ref description);
//                }
//#endif
            }
        }

        public unsafe override void Present()
        {
            var semaphoreCreateInfo = new SemaphoreCreateInfo { StructureType = StructureType.SemaphoreCreateInfo };
            var presentCompleteSemaphore = GraphicsDevice.NativeDevice.CreateSemaphore(ref semaphoreCreateInfo);

            try
            {
                // TODO VULKAN: draw + image layout transition

                var swapChainCopy = swapChain;
                var currentBufferIndexCopy = currentBufferIndex;
                var presentInfo = new PresentInfo
                {
                    SwapchainCount = 1,
                    Swapchains = new IntPtr(&swapChainCopy),
                    ImageIndices = new IntPtr(&currentBufferIndexCopy),
                };

                // Present
                GraphicsDevice.NativeCommandQueue.Present(ref presentInfo);
                GraphicsDevice.NativeCommandQueue.WaitIdle();

                // Get next image
                currentBufferIndex = GraphicsDevice.NativeDevice.AcquireNextImage(swapChain, ulong.MaxValue, presentCompleteSemaphore, default(Fence));

                // Flip render targets
                BackBuffer.SetNativeHandles(buffers[currentBufferIndex].NativeImage, buffers[currentBufferIndex].NativeColorAttachmentView);
            }
            catch (SharpVulkanException e) when (e.Result == Result.ErrorOutOfDate)
            {
                // TODO VULKAN 
            }
            finally
            {

                GraphicsDevice.NativeDevice.DestroySemaphore(presentCompleteSemaphore);
            }
        }

        protected override void OnNameChanged()
        {
            base.OnNameChanged();
        }

        public unsafe override void OnDestroyed()
        {
            backbuffer.Dispose();
            backbuffer = null;

            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
            buffers = null;

            GraphicsDevice.NativeDevice.DestroySwapchain(swapChain);
            swapChain = Swapchain.Null;

            base.OnDestroyed();
        }

        public override void OnRecreated()
        {
            base.OnRecreated();

            // Recreate swap chain
            swapChain = CreateSwapChain();
        }

        protected override void ResizeBackBuffer(int width, int height, PixelFormat format)
        {
            throw new NotImplementedException();

            foreach (var buffer in buffers)
            {
                buffer.OnDestroyed();
            }
        }

        protected override void ResizeDepthStencilBuffer(int width, int height, PixelFormat format)
        {
            var newTextureDescription = DepthStencilBuffer.Description;
            newTextureDescription.Width = width;
            newTextureDescription.Height = height;

            // Manually update the texture
            DepthStencilBuffer.OnDestroyed();

            // Put it in our back buffer texture
            DepthStencilBuffer.InitializeFrom(newTextureDescription);
        }


        private unsafe Swapchain CreateSwapChain()
        {

            Description.BackBufferFormat = PixelFormat.B8G8R8A8_UNorm;

            CreateSurface();

            // Create swapchain
            SurfaceCapabilities surfaceCapabilities;
            GraphicsDevice.Adapter.PhysicalDevice.GetSurfaceCapabilities(surface, out surfaceCapabilities);

            // Buffer count
            uint desiredImageCount = surfaceCapabilities.MinImageCount + 1;
            if (surfaceCapabilities.MaxImageCount > 0 && desiredImageCount > surfaceCapabilities.MaxImageCount)
            {
                desiredImageCount = surfaceCapabilities.MaxImageCount;
            }

            // Transform
            SurfaceTransformFlags preTransform;
            if ((surfaceCapabilities.SupportedTransforms & SurfaceTransformFlags.Identity) != 0)
            {
                preTransform = SurfaceTransformFlags.Identity;
            }
            else
            {
                preTransform = surfaceCapabilities.CurrentTransform;
            }

            // Find present mode
            var presentModes = GraphicsDevice.Adapter.PhysicalDevice.GetSurfacePresentModes(surface);
            var swapChainPresentMode = PresentMode.Fifo;
            foreach (var presentMode in presentModes)
            {
                if (presentMode == PresentMode.Mailbox)
                {
                    swapChainPresentMode = PresentMode.Mailbox;
                    break;
                }

                if (swapChainPresentMode != PresentMode.Mailbox && presentMode == PresentMode.Immediate)
                {
                    swapChainPresentMode = PresentMode.Immediate;
                }
            }

            // Native format
            Format backBufferFormat;
            int pixelSize;
            bool compressed;
            VulkanConvertExtensions.ConvertPixelFormat(Description.BackBufferFormat, out backBufferFormat, out pixelSize, out compressed);

            // Create swapchain
            var swapchainCreateInfo = new SwapchainCreateInfo
            {
                StructureType = StructureType.SwapchainCreateInfo,
                Surface = surface,
                ImageArrayLayers = 1,
                ImageSharingMode = SharingMode.Exclusive,
                ImageExtent = new Extent2D((uint)Description.BackBufferWidth, (uint)Description.BackBufferHeight),
                ImageFormat = backBufferFormat,
                ImageColorSpace = Description.ColorSpace == ColorSpace.Gamma ? SharpVulkan.ColorSpace.SRgbNonlinear : 0,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                PresentMode = swapChainPresentMode,
                CompositeAlpha = CompositeAlphaFlags.Opaque,
                MinImageCount = desiredImageCount,
                PreTransform = preTransform,
                // OldSwapchain = 
                Clipped = true
            };
            swapChain = GraphicsDevice.NativeDevice.CreateSwapchain(ref swapchainCreateInfo);

            CreateBackBuffers();

            return swapChain;
        }

        private unsafe void CreateSurface()
        {
            // Check for Window Handle parameter
            if (Description.DeviceWindowHandle == null)
            {
                throw new ArgumentException("DeviceWindowHandle cannot be null");
            }

            var control = Description.DeviceWindowHandle.NativeHandle as Control;
            if (control == null)
            {
                throw new NotSupportedException($"Form of type [{Description.DeviceWindowHandle.GetType().Name}] is not supported. Only System.Windows.Control are supported");
            }

            // TODO VULKAN Check queue surface support

            // Create surface
#if SILICONSTUDIO_PLATFORM_WINDOWS
            var surfaceCreateInfo = new Win32SurfaceCreateInfo
            {
                StructureType = StructureType.Win32SurfaceCreateInfo,
                InstanceHandle = Process.GetCurrentProcess().Handle,
                WindowHandle = control.Handle,
            };
            surface = GraphicsAdapterFactory.Instance.CreateWin32Surface(surfaceCreateInfo);
#endif
        }

        private void CreateBackBuffers()
        {
            var backBufferDescription = new TextureDescription
            {
                ArraySize = 1,
                Dimension = TextureDimension.Texture2D,
                Height = Description.BackBufferHeight,
                Width = Description.BackBufferWidth,
                Depth = 1,
                Flags = TextureFlags.RenderTarget,
                Format = Description.BackBufferFormat,
                MipLevels = 1,
                MultiSampleLevel = MSAALevel.None,
                Usage = GraphicsResourceUsage.Default
            };

            var swapChainImages = GraphicsDevice.NativeDevice.GetSwapchainImages(swapChain);
            buffers = new Texture[swapChainImages.Length];
            for (int i = 0; i < swapChainImages.Length; i++)
            {
                buffers[i] = new Texture(GraphicsDevice).InitializeFromPersistent(backBufferDescription, swapChainImages[i]);
            }

            backbuffer = new Texture(GraphicsDevice).InitializeWithoutResources(backBufferDescription);
            backbuffer.SetNativeHandles(buffers[0].NativeImage, buffers[0].NativeColorAttachmentView);
        }
    }
}
#endif
