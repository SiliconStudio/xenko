// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Known values for <see cref="DepthStencilStateDescription"/>.
    /// </summary>
    public static class DepthStencilStates
    {
        /// <summary>
        /// A built-in state object with default settings for using a depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilStateDescription Default = new DepthStencilStateDescription(true, true);

        /// <summary>
        /// A built-in state object with default settings using greater comparison for Z.
        /// </summary>
        public static readonly DepthStencilStateDescription DefaultInverse = new DepthStencilStateDescription(true, true) { DepthBufferFunction = CompareFunction.GreaterEqual };

        /// <summary>
        /// A built-in state object with settings for enabling a read-only depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilStateDescription DepthRead = new DepthStencilStateDescription(true, false);

        /// <summary>
        /// A built-in state object with settings for not using a depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilStateDescription None = new DepthStencilStateDescription(false, false);
    }
}

