// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Graphics presenter for SwapChain.
    /// </summary>
    public class SwapChainGraphicsPresenter : GraphicsPresenter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SwapChainGraphicsPresenter"/> for <param name="device"/> using 
        /// the <see cref="PresentationParameters"/> <param name="parameters"/>.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="parameters">The presentation parameters.</param>
        public SwapChainGraphicsPresenter(GraphicsDevice device, PresentationParameters parameters) : base(device, parameters)
        {
            NullHelper.ToImplement();
        }

        /// <summary>
        /// Current back buffer.
        /// </summary>
        public override Texture BackBuffer
        {
            get
            {
                NullHelper.ToImplement();
                return default(Texture);
            }
        }

        /// <summary>
        /// Is current presented in full screen mode?
        /// </summary>
        public override bool IsFullScreen
        {
            get
            {
                NullHelper.ToImplement();
                return false;
            }

            set
            {
                NullHelper.ToImplement();
            }
        }

        /// <summary>
        /// Native implementation of the swap chain.
        /// </summary>
        public override object NativePresenter
        {
            get
            {
                NullHelper.ToImplement();
                return null;
            }
        }

        /// <summary>
        /// Present the swap chain.
        /// </summary>
        public override void Present()
        {
            NullHelper.ToImplement();
        }

        /// <summary>
        /// Resize back buffer to accommodate a dimension of <param name="width"/> by <param name="height"/> pixels and
        /// a given <param name="format"/>.
        /// </summary>
        /// <param name="width">The new width in pixels.</param>
        /// <param name="height">The new height in pixels.</param>
        /// <param name="format">The new format.</param>
        protected override void ResizeBackBuffer(int width, int height, PixelFormat format)
        {
            NullHelper.ToImplement();
        }

        /// <summary>
        /// Resize the depth stencil buffer to accommodate a dimension of <param name="width"/> by <param name="height"/> pixels and
        /// a given <param name="format"/>.
        /// </summary>
        /// <param name="width">The new width in pixels.</param>
        /// <param name="height">The new height in pixels.</param>
        /// <param name="format">The new format.</param>
        protected override void ResizeDepthStencilBuffer(int width, int height, PixelFormat format)
        {
            NullHelper.ToImplement();
        }
    }
}
#endif
