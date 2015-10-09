// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
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
            Default = DepthStencilState.New(device, new DepthStencilStateDescription(true, true)).DisposeBy(this);
            Default.Name = "DepthStencilState.Default";

            DefaultInverse = DepthStencilState.New(device, new DepthStencilStateDescription(true, true) { DepthBufferFunction = CompareFunction.GreaterEqual }).DisposeBy(this);
            DefaultInverse.Name = "DepthStencilState.DefaultInverse";

            DepthRead = DepthStencilState.New(device, new DepthStencilStateDescription(true, false)).DisposeBy(this);
            DepthRead.Name = "DepthStencilState.DepthRead";

            None = DepthStencilState.New(device, new DepthStencilStateDescription(false, false)).DisposeBy(this);
            None.Name = "DepthStencilState.None";
        }

        /// <summary>
        /// A built-in state object with default settings for using a depth stencil buffer.
        /// </summary>
        public readonly DepthStencilState Default;

        /// <summary>
        /// A built-in state object with default settings using greater comparison for Z.
        /// </summary>
        public readonly DepthStencilState DefaultInverse;

        /// <summary>
        /// A built-in state object with settings for enabling a read-only depth stencil buffer.
        /// </summary>
        public readonly DepthStencilState DepthRead;

        /// <summary>
        /// A built-in state object with settings for not using a depth stencil buffer.
        /// </summary>
        public readonly DepthStencilState None;
    }
}

