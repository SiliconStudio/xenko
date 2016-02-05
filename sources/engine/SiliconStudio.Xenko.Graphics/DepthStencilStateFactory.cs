// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Base factory for <see cref="IDepthStencilState"/>.
    /// </summary>
    public class DepthStencilStateFactory : GraphicsResourceFactoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilStateFactory"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal DepthStencilStateFactory(GraphicsDevice device) : base(device)
        {
            Default = new DepthStencilStateDescription(true, true);
            DefaultInverse = new DepthStencilStateDescription(true, true) { DepthBufferFunction = CompareFunction.GreaterEqual };
            DepthRead = new DepthStencilStateDescription(true, false);
            None = new DepthStencilStateDescription(false, false);
        }

        /// <summary>
        /// A built-in state object with default settings for using a depth stencil buffer.
        /// </summary>
        public readonly DepthStencilStateDescription Default;

        /// <summary>
        /// A built-in state object with default settings using greater comparison for Z.
        /// </summary>
        public readonly DepthStencilStateDescription DefaultInverse;

        /// <summary>
        /// A built-in state object with settings for enabling a read-only depth stencil buffer.
        /// </summary>
        public readonly DepthStencilStateDescription DepthRead;

        /// <summary>
        /// A built-in state object with settings for not using a depth stencil buffer.
        /// </summary>
        public readonly DepthStencilStateDescription None;
    }
}

