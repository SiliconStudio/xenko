// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Contains depth-stencil state for the device.
    /// </summary>
    public partial class DepthStencilState : GraphicsResourceBase
    {
        // For FakeDepthStencilState.
        protected DepthStencilState()
        {
        }

        // For FakeDepthStencilState.
        private DepthStencilState(DepthStencilStateDescription description)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilState"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="description">The description.</param>
        public static DepthStencilState New(GraphicsDevice graphicsDevice, DepthStencilStateDescription description)
        {
            return new DepthStencilState(graphicsDevice, description);
        }

        /// <summary>
        /// Create a new fake depth-stencil state for serialization.
        /// </summary>
        /// <param name="description">The description of the depth-stencil state</param>
        /// <returns>The fake depth-stencil state</returns>
        public static DepthStencilState NewFake(DepthStencilStateDescription description)
        {
            return new DepthStencilState(description);
        }
        
        /// <summary>
        /// Gets the depth stencil state description.
        /// </summary>
        public readonly DepthStencilStateDescription Description;
    }
}