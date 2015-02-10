// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Rendering context.
    /// </summary>
    public class RenderContext
    {
        private readonly GraphicsDevice graphicsDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderContext"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public RenderContext(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return graphicsDevice;
            }
        }

        public ParameterCollection Parameters { get; set; }
    }
}
