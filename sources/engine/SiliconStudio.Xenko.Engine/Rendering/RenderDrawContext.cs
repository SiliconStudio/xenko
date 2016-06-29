using System;
using System.Collections.Generic;
using System.Threading;
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

        public RenderDrawContext(IServiceRegistry services, RenderContext renderContext, GraphicsContext graphicsContext)
        {
            if (services == null) throw new ArgumentNullException("services");

            RenderContext = renderContext;
            ResourceGroupAllocator = graphicsContext.ResourceGroupAllocator;
            GraphicsDevice = RenderContext.GraphicsDevice;
            GraphicsContext = graphicsContext;
            CommandList = graphicsContext.CommandList;
            Resolver = new ResourceResolver(this);
        }

        /// <summary>
        /// Gets the render context.
        /// </summary>
        public RenderContext RenderContext { get; }

        /// <summary>
        /// Gets the <see cref="ResourceGroup"/> allocator.
        /// </summary>
        public ResourceGroupAllocator ResourceGroupAllocator { get; }

        /// <summary>
        /// Gets the command list.
        /// </summary>
        public CommandList CommandList { get; private set; }

        public GraphicsContext GraphicsContext { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ResourceResolver Resolver { get; private set; }

        /// <summary>
        /// Locks the command list until <see cref="IDisposable.Dispose()"/> is called on the returned value type.
        /// </summary>
        /// <returns></returns>
        /// This is necessary only during Collect(), Extract() and Prepare() phases, not during Draw().
        /// Some graphics API might not require actual locking, in which case this object might do nothing.
        public CommandListLock LockCommandList()
        {
            // TODO: Temporary, for now we use the CommandList itself as a lock
            return new CommandListLock(CommandList);
        }

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

            public int RenderTargetCount;

            public Viewport[] Viewports;
            public Texture[] RenderTargets;
            public Texture DepthStencilBuffer;

            public void Capture(CommandList commandList)
            {
                RenderTargetCount = commandList.RenderTargetCount;

                // TODO GRAPHICS REFACTOR avoid unecessary reallocation if size is different
                if (RenderTargetCount > 0 && (RenderTargets == null || RenderTargets.Length != RenderTargetCount))
                {
                    RenderTargets = new Texture[RenderTargetCount];
                    Viewports = new Viewport[RenderTargetCount];
                }

                DepthStencilBuffer = commandList.DepthStencilBuffer;

                for (int i = 0; i < RenderTargetCount; i++)
                {
                    Viewports[i] = commandList.Viewports[i];
                    RenderTargets[i] = commandList.RenderTargets[i];
                }
            }

            public void Restore(CommandList commandList)
            {
                commandList.SetRenderTargetsAndViewport(DepthStencilBuffer, RenderTargetCount > 0 ? RenderTargets : null);
                if (RenderTargetCount > 0)
                    commandList.SetViewports(Viewports);
            }
        }

        /// <summary>
        /// Used to prevent concurrent uses of CommandList.
        /// </summary>
        public struct CommandListLock : IDisposable
        {
            private readonly bool lockTaken;
            private object lockObject;

            public CommandListLock(object lockObject)
            {
                this.lockObject = lockObject;
                lockTaken = false;
                Monitor.Enter(lockObject, ref lockTaken);
            }

            public void Dispose()
            {
                if (lockTaken)
                    Monitor.Exit(lockObject);
                lockObject = null;
            }
        }
    }
}
