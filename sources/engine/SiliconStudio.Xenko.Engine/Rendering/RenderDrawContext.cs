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
        // States
        private int currentStateIndex = -1;
        private readonly List<StateAndTargets> allocatedStates = new List<StateAndTargets>(10);

        private readonly Dictionary<Type, DrawEffect> sharedEffects = new Dictionary<Type, DrawEffect>();

        public RenderDrawContext(IServiceRegistry services, RenderContext renderContext, CommandList commandList)
        {
            if (services == null) throw new ArgumentNullException("services");

            RenderContext = renderContext;
            GraphicsDevice = RenderContext.GraphicsDevice;
            CommandList = commandList;
        }

        /// <summary>
        /// Gets the command list.
        /// </summary>
        public CommandList CommandList { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public RenderContext RenderContext { get; private set; }

        /// <summary>
        /// Pushes render targets and viewport state.
        /// </summary>
        public void PushRenderTargets()
        {
            // Check if we need to allocate a new StateAndTargets
            StateAndTargets newState;
            currentStateIndex++;
            if (currentStateIndex == allocatedStates.Count)
            {
                newState = new StateAndTargets();
                allocatedStates.Add(newState);
            }
            else
            {
                newState = allocatedStates[currentStateIndex];
            }
            newState.Capture(CommandList);
        }

        /// <summary>
        /// Restores render targets and viewport state.
        /// </summary>
        public void PopRenderTargets()
        {
            if (currentStateIndex < 0)
            {
                throw new InvalidOperationException("Cannot pop more than push");
            }

            var oldState = allocatedStates[currentStateIndex--];
            oldState.Restore(CommandList);
        }

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

        /// <summary>
        /// Holds current viewports and render targets.
        /// </summary>
        private class StateAndTargets
        {
            private const int MaxRenderTargetCount = 8;

            public Viewport[] Viewports;

            public Texture DepthStencilBuffer;

            public Texture[] RenderTargets;

            public void Capture(CommandList commandList)
            {
                int renderTargetCount = MaxRenderTargetCount;
                switch (commandList.GraphicsDevice.Features.Profile)
                {
                    case GraphicsProfile.Level_9_1:
                    case GraphicsProfile.Level_9_2:
                    case GraphicsProfile.Level_9_3:
                        renderTargetCount = 1;
                        break;
                }

                if (RenderTargets == null || RenderTargets.Length != renderTargetCount)
                {
                    RenderTargets = new Texture[renderTargetCount];
                    Viewports = new Viewport[renderTargetCount];
                }

                DepthStencilBuffer = commandList.DepthStencilBuffer;

                for (int i = 0; i < renderTargetCount; i++)
                {
                    Viewports[i] = commandList.Viewports[i];
                    RenderTargets[i] = commandList.RenderTargets[i];
                }
            }

            public void Restore(CommandList commandList)
            {
                commandList.SetDepthAndRenderTargets(DepthStencilBuffer, RenderTargets);
                commandList.SetViewports(Viewports);
            }
        }
    }
}