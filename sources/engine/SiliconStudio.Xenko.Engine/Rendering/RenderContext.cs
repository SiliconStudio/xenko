// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using ComponentBase = SiliconStudio.Core.ComponentBase;
using IServiceRegistry = SiliconStudio.Core.IServiceRegistry;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Rendering context.
    /// </summary>
    public sealed class RenderContext : ComponentBase
    {
        private const string SharedImageEffectContextKey = "__SharedRenderContext__";
        private readonly ThreadLocal<RenderDrawContext> threadContext;

        // Used for API that don't support multiple command lists
        internal CommandList SharedCommandList;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderContext" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        internal RenderContext(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            Services = services;
            Effects = services.GetSafeServiceAs<EffectSystem>();
            GraphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            Allocator = services.GetServiceAs<GraphicsContext>().Allocator ?? new GraphicsResourceAllocator(GraphicsDevice).DisposeBy(GraphicsDevice);

            threadContext = new ThreadLocal<RenderDrawContext>(() => new RenderDrawContext(Services, this, new GraphicsContext(GraphicsDevice, Allocator)), true);
        }

        /// <summary>
        /// Occurs when a renderer is initialized.
        /// </summary>
        public event Action<IGraphicsRendererCore> RendererInitialized;

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        /// <value>The content manager.</value>
        public EffectSystem Effects { get; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services { get; }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>The time.</value>
        public GameTime Time { get; internal set; }

        /// <summary>
        /// Gets the <see cref="GraphicsResource"/> allocator.
        /// </summary>
        /// <value>The allocator.</value>
        public GraphicsResourceAllocator Allocator { get; }

        /// <summary>
        /// The current render system.
        /// </summary>
        public RenderSystem RenderSystem { get; set; }

        /// <summary>
        /// The current scene instance.
        /// </summary>
        public SceneInstance SceneInstance { get; set; }

        /// <summary>
        /// The current visibility group from the <see cref="SceneInstance"/> and <see cref="RenderSystem"/>.
        /// </summary>
        public VisibilityGroup VisibilityGroup { get; set; }

        /// <summary>
        /// The current render output format (used during the collect phase).
        /// </summary>
        public Stack<RenderOutputDescription> RenderOutputs { get; } = new Stack<RenderOutputDescription>();

        /// <summary>
        /// The current render output format (used during the collect phase).
        /// </summary>
        public Stack<ViewportState> ViewportStates { get; } = new Stack<ViewportState>();

        /// <summary>
        /// The current render view.
        /// </summary>
        public RenderView RenderView { get; set; }

        /// <summary>
        /// Gets a global shared context.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>RenderContext.</returns>
        public static RenderContext GetShared(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Store RenderContext shared into the GraphicsDevice
            var graphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            return graphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, SharedImageEffectContextKey, d => new RenderContext(services));
        }

        public RenderDrawContext GetThreadContext() => threadContext.Value;

        public void Reset()
        {
            foreach (var context in threadContext.Values)
            {
                context.ResourceGroupAllocator.Reset(context.CommandList);
            }
        }

        public void Flush()
        {
            foreach (var context in threadContext.Values)
            {
                context.ResourceGroupAllocator.Flush();
            }
        }

        internal void OnRendererInitialized(IGraphicsRendererCore obj)
        {
            RendererInitialized?.Invoke(obj);
        }
    }
    public class ViewportState
    {
        public Viewport Viewport0;
        //public Viewport Viewport1;
        //public Viewport Viewport2;
        //public Viewport Viewport3;
        //public Viewport Viewport4;
        //public Viewport Viewport5;
        //public Viewport Viewport6;
        //public Viewport Viewport7;

        /// <summary>
        /// Capture state from <see cref="CommandList.Viewports"/>.
        /// </summary>
        /// <param name="commandList">The command list to capture state from.</param>
        public unsafe void CaptureState(CommandList commandList)
        {
            // TODO: Support multiple viewports
            var viewports = commandList.Viewports;
            fixed (Viewport* viewport0 = &Viewport0)
            {
                for (int i = 0; i < 1; ++i)
                {
                    viewport0[i] = viewports[i];
                }
            }
        }
    }
}
