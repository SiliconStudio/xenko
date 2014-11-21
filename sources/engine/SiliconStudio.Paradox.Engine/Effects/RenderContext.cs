// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Thread-local storage context used during rendering.
    /// </summary>
    public class RenderContext
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly ParameterCollection parameters;

        public RenderContext(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            parameters = new ParameterCollection("Thread Context parameters");
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

        /// <summary>
        /// Gets the parameters shared by this instance.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// Gets or sets the current pass being rendered.
        /// </summary>
        /// <value>The current pass.</value>
        public RenderPass CurrentPass { get; set; }
    }
}
