using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Rendering context used during <see cref="IGraphicsRenderer.Draw"/>.
    /// </summary>
    public sealed class RenderDrawContext : ComponentBase
    {
        private readonly Dictionary<Type, DrawEffect> sharedEffects = new Dictionary<Type, DrawEffect>();

        public RenderDrawContext(IServiceRegistry services, RenderContext renderContext)
        {
            if (services == null) throw new ArgumentNullException("services");

            RenderContext = renderContext;
            GraphicsDevice = RenderContext.GraphicsDevice;
        }

        /// <summary>
        /// Gets the command list.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        public RenderContext RenderContext { get; private set; }

        /// <summary>
        /// Gets or creates a shared effect.
        /// </summary>
        /// <typeparam name="T">Type of the shared effect (mush have a constructor taking a <see cref="Rendering.RenderContext"/></typeparam>
        /// <returns>A singleton instance of <typeparamref name="T"/></returns>
        public T GetSharedEffect<T>() where T : DrawEffect, new()
        {
            // TODO: Add a way to support custom constructor
            lock (sharedEffects)
            {
                DrawEffect effect;
                if (!sharedEffects.TryGetValue(typeof(T), out effect))
                {
                    effect = new T();
                    sharedEffects.Add(typeof(T), effect);
                    effect.Initialize(RenderContext);
                }

                return (T)effect;
            }
        }

    }
}