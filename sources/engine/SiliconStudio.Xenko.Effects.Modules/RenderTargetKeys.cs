// Copyright (c) 2011 Silicon Studio

using Xenko.Framework.Graphics;

namespace Xenko.Effects.Modules
{
    /// <summary>
    /// Keys used for render target settings.
    /// </summary>
    public static class RenderTargetKeys
    {
        /// <summary>
        /// The depth stencil buffer key.
        /// </summary>
        public static readonly ParameterResourceKey<DepthStencilBuffer> DepthStencil = ParameterKeys.Resource<DepthStencilBuffer>();

        /// <summary>
        /// The render target key.
        /// </summary>
        public static readonly ParameterResourceKey<RenderTarget> RenderTarget = ParameterKeys.Resource<RenderTarget>();

        /// <summary>
        /// The render target key.
        /// </summary>
        public static readonly ParameterResourceKey<Buffer> StreamTarget = ParameterKeys.Resource<Buffer>();
    }
}
