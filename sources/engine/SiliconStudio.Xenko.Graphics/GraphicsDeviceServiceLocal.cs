// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A default implementation of <see cref="IGraphicsDeviceService"/>
    /// </summary>
    public class GraphicsDeviceServiceLocal : IGraphicsDeviceService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDeviceServiceLocal"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        public GraphicsDeviceServiceLocal(GraphicsDevice graphicsDevice) : this(null, graphicsDevice)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDeviceServiceLocal"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        public GraphicsDeviceServiceLocal(IServiceRegistry registry, GraphicsDevice graphicsDevice)
        {
            if (registry != null)
            {
                registry.AddService(typeof(IGraphicsDeviceService), this);
            }
            GraphicsDevice = graphicsDevice;
        }

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice { get; private set; }
    }
}