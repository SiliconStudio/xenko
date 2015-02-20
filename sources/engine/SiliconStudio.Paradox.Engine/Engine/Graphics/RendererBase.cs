// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Base implementation of <see cref="IGraphicsRenderer"/>
    /// </summary>
    [DataContract]
    public abstract class RendererBase : ComponentBase, IGraphicsRenderer
    {
        private bool isInDrawCore;
        private readonly List<GraphicsResource> scopedResources = new List<GraphicsResource>();
        private readonly List<IGraphicsRenderer> subRenderersToUnload;

        /// <summary>
        /// Initializes a new instance of the <see cref="RendererBase"/> class.
        /// </summary>
        protected RendererBase() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBase" /> class.
        /// </summary>
        /// <param name="name">The name attached to this component</param>
        protected RendererBase(string name) : base(name)
        {
            Enabled = true;
            subRenderersToUnload = new List<IGraphicsRenderer>();
            Profiling = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EntityComponentRendererBase"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DataMember(-20)]
        [DefaultValue(true)]
        public virtual bool Enabled { get; set; }

        [DataMemberIgnore]
        public bool Profiling { get; set; }

        [DataMemberIgnore]
        protected RenderContext Context { get; private set; }

        public virtual void Initialize(RenderContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            // Unload the previous context if any
            if (Context != null)
            {
                Unload();
            }

            Context = context;
            subRenderersToUnload.Clear();
        }

        /// <summary>
        /// Unloads this instance on dispose.
        /// </summary>
        protected virtual void Unload()
        {
            foreach (var drawEffect in subRenderersToUnload)
            {
                drawEffect.Dispose();
            }
            subRenderersToUnload.Clear();

            Context = null;
        }

        protected virtual void PreDrawCore(RenderContext context)
        {
            if (Name != null && Profiling)
            {
                context.GraphicsDevice.BeginProfile(Color.Green, Name);
            }
        }

        protected virtual void PostDrawCore(RenderContext context)
        {
            if (Name != null && Profiling)
            {
                context.GraphicsDevice.EndProfile();
            }
        }

        /// <summary>
        /// Main drawing method for this renderer that must be implemented. 
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawCore(RenderContext context);

        /// <summary>
        /// Draws this renderer with the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        /// <exception cref="System.InvalidOperationException">Cannot use a different context between Load and Draw</exception>
        public void Draw(RenderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!Enabled)
            {
                return;
            }

            if (Context == null)
            {
                Initialize(context);
            }
            else if (Context != context)
            {
                throw new InvalidOperationException("Cannot use a different context between Load and Draw");
            }

            PreDrawCore(context);
            {
                // Allow scoped allocation RenderTargets
                isInDrawCore = true;
                DrawCore(context);
                isInDrawCore = false;

                // Release scoped RenderTargets
                ReleaseAllScopedResources();
            }
            PostDrawCore(context);
        }

        /// <summary>
        /// Gets a render target with the specified description, scoped for the duration of the <see cref="DrawEffect.DrawCore"/>.
        /// </summary>
        /// <param name="description">The description of the buffer to allocate</param>
        /// <param name="viewFormat">The pixel format seen in shader</param>
        /// <returns>A new instance of texture.</returns>
        protected Buffer NewScopedBuffer(BufferDescription description, PixelFormat viewFormat = PixelFormat.None)
        {
            CheckIsInDrawCore();
            return PushScopedResource(Context.Allocator.GetTemporaryBuffer(description, viewFormat));
        }

        /// <summary>
        /// Gets a render target with the specified description, scoped for the duration of the <see cref="DrawEffect.DrawCore"/>.
        /// </summary>
        /// <returns>A new instance of texture.</returns>
        protected Buffer NewScopedTypedBuffer(int count, PixelFormat viewFormat, bool isUnorderedAccess, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return NewScopedBuffer(new BufferDescription(count * viewFormat.SizeInBytes(), BufferFlags.ShaderResource | (isUnorderedAccess ? BufferFlags.UnorderedAccess : BufferFlags.None), usage), viewFormat);
        }

        /// <summary>
        /// Pushes a new scoped resource to the current Draw.
        /// </summary>
        /// <param name="resource">The scoped resource</param>
        /// <returns></returns>
        protected T PushScopedResource<T>(T resource) where T : GraphicsResource
        {
            scopedResources.Add(resource);
            return resource;
        }

        /// <summary>
        /// Checks that the current execution path is between a PreDraw/PostDraw sequence and throws and exception if not.
        /// </summary>
        protected void CheckIsInDrawCore()
        {
            if (!isInDrawCore)
            {
                throw new InvalidOperationException("The method execution path is not within a DrawCore operation");
            }
        }

        protected override void Destroy()
        {
            // If this instance is destroyed and not unload, force an unload before destryoing it completely
            if (Context != null)
            {
                Unload();
            }
            base.Destroy();
        }

        protected T ToLoadAndUnload<T>(T effect) where T : IGraphicsRenderer
        {
            if (effect == null) throw new ArgumentNullException("effect");
            effect.Initialize(Context);
            subRenderersToUnload.Add(effect);
            return effect;
        }

        private void ReleaseAllScopedResources()
        {
            foreach (var scopedResource in scopedResources)
            {
                Context.Allocator.ReleaseReference(scopedResource);
            }
            scopedResources.Clear();
        }
    }
}