// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Keys used for render target settings.
    /// </summary>
    public static class RenderTargetKeys
    {
        /// <summary>
        /// The depth stencil buffer key.
        /// </summary>
        public static readonly ObjectParameterKey<Texture> DepthStencil = ParameterKeys.NewObject<Texture>();

        /// <summary>
        /// The depth stencil buffer key used as an input shader resource.
        /// </summary>
        public static readonly ObjectParameterKey<Texture> DepthStencilSource = ParameterKeys.NewObject<Texture>();

        /// <summary>
        /// The render target key.
        /// </summary>
        public static readonly ObjectParameterKey<Texture> RenderTarget = ParameterKeys.NewObject<Texture>();

        /// <summary>
        /// The render target key.
        /// </summary>
        public static readonly ObjectParameterKey<Buffer> StreamTarget = ParameterKeys.NewObject<Buffer>();

        /// <summary>
        /// Used by <see cref="RenderTargetPlugin"/> to notify that the plugin requires support for depth stencil as shader resource
        /// </summary>
        public static readonly PropertyKey<bool> RequireDepthStencilShaderResource = new PropertyKey<bool>("RequireDepthStencilShaderResource", typeof(RenderTargetKeys));
    }
}
