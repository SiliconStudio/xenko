// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Keys used for render target settings.
    /// </summary>
    public static class RenderTargetKeys
    {
        /// <summary>
        /// The depth stencil buffer key.
        /// </summary>
        public static readonly ParameterKey<Texture> DepthStencil = ParameterKeys.New<Texture>();

        /// <summary>
        /// The depth stencil buffer key used as an input shader resource.
        /// </summary>
        public static readonly ParameterKey<Texture> DepthStencilSource = ParameterKeys.New<Texture>();

        /// <summary>
        /// The render target key.
        /// </summary>
        public static readonly ParameterKey<Texture> RenderTarget = ParameterKeys.New<Texture>();

        /// <summary>
        /// The render target key.
        /// </summary>
        public static readonly ParameterKey<Buffer> StreamTarget = ParameterKeys.New<Buffer>();

        /// <summary>
        /// Used by <see cref="RenderTargetPlugin"/> to notify that the plugin requires support for depth stencil as shader resource
        /// </summary>
        public static readonly PropertyKey<bool> RequireDepthStencilShaderResource = new PropertyKey<bool>("RequireDepthStencilShaderResource", typeof(RenderTargetKeys));
    }
}
