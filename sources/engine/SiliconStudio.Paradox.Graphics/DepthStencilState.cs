// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Contains depth-stencil state for the device.
    /// </summary>
    [ContentSerializer(typeof(DepthStencilStateSerializer))]
    public partial class DepthStencilState : GraphicsResourceBase
    {
        // For FakeDepthStencilState.
        protected DepthStencilState()
        {
        }

        // For FakeDepthStencilState.
        protected DepthStencilState(DepthStencilStateDescription description)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IDepthStencilState"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="description">The description.</param>
        public static DepthStencilState New(GraphicsDevice graphicsDevice, DepthStencilStateDescription description)
        {
            return new DepthStencilState(graphicsDevice, description);
        }
        
        /// <summary>
        /// Gets the depth stencil state description.
        /// </summary>
        public readonly DepthStencilStateDescription Description;
    }
}