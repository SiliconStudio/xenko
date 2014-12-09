// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Context for post effects.
    /// </summary>
    public class ImageEffectContext : ComponentBase
    {
        private const string SharedImageEffectContextKey = "__SharedImageEffectContext__";
        private readonly Dictionary<Type, ImageEffect> sharedEffects = new Dictionary<Type, ImageEffect>();

        private readonly GraphicsResourceAllocator allocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectContext" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        public ImageEffectContext(Game game)
            : this(game.Services)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectContext" /> class.
        /// </summary>
        /// <param name="serviceRegistry">The service registry.</param>
        /// <param name="allocator">The allocator.</param>
        /// <exception cref="System.ArgumentNullException">serviceRegistry</exception>
        public ImageEffectContext(IServiceRegistry serviceRegistry, GraphicsResourceAllocator allocator = null)
        {
            if (serviceRegistry == null) throw new ArgumentNullException("serviceRegistry");
            Services = serviceRegistry;
            Effects = serviceRegistry.GetSafeServiceAs<EffectSystem>();
            this.allocator = allocator ?? new GraphicsResourceAllocator(Services).DisposeBy(this);
            GraphicsDevice = serviceRegistry.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        /// <value>The content manager.</value>
        public EffectSystem Effects { get; private set; }

        /// <summary>
        /// Gets or sets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the parameters shared with all <see cref="ImageEffect"/> instance.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Gets the <see cref="GraphicsResource"/> allocator.
        /// </summary>
        /// <value>The allocator.</value>
        public GraphicsResourceAllocator Allocator
        {
            get
            {
                return allocator;
            }
        }

        /// <summary>
        /// Gets or creates a shared effect.
        /// </summary>
        /// <typeparam name="T">Type of the shared effect (mush have a constructor taking a <see cref="ImageEffectContext"/></typeparam>
        /// <returns>A singleton instance of <typeparamref name="T"/></returns>
        public T GetSharedEffect<T>() where T : ImageEffect
        {
            // TODO: Add a way to support custom constructor
            lock (sharedEffects)
            {
                ImageEffect effect;
                if (!sharedEffects.TryGetValue(typeof(T), out effect))
                {
                    effect = (ImageEffect)Activator.CreateInstance(typeof(T), this);
                    sharedEffects.Add(typeof(T), effect);
                }

                return (T)effect;
            }
        }

        /// <summary>
        /// Gets a global shared context.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>ImageEffectContext.</returns>
        public static ImageEffectContext GetShared(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException("services");

            // Store ImageEffectContext shared into the GraphicsDevice
            var graphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            return graphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, SharedImageEffectContextKey, () => new ImageEffectContext(services));
        }

        protected override void Destroy()
        {
            foreach (var effectPair in sharedEffects)
            {
                effectPair.Value.Dispose();
            }
            sharedEffects.Clear();

            base.Destroy();
        }
    }
}