// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.PostEffects
{
    /// <summary>
    /// Post effect base class.
    /// </summary>
    public abstract class PostEffectBase : ComponentBase
    {
        private RenderTarget outputRenderTargetView;

        private RenderTarget[] outputRenderTargetViews;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostEffectBase" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        protected PostEffectBase(PostEffectContext context, string name = null) : base(name)
        {
            if (context == null) throw new ArgumentNullException("context");

            Context = context;
            GraphicsDevice = Context.GraphicsDevice;
            Name = name ?? GetType().Name;
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this post effect is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public PostEffectContext Context { get; private set; }

        /// <summary>
        /// Gets the <see cref="AssetManager"/>.
        /// </summary>
        /// <value>The content.</value>
        public AssetManager Assets { get; private set; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice { get; private set; }
        /// <summary>
        /// Sets the render target output.
        /// </summary>
        /// <param name="view">The render target output view.</param>
        public PostEffectBase SetOutput(RenderTarget view)
        {
            if (view == null) throw new ArgumentNullException("view");

            SetOutputInternal(view);
            return this;
        }

        /// <summary>
        /// Sets the render target outputs.
        /// </summary>
        /// <param name="views">The render target output views.</param>
        public virtual PostEffectBase SetOutput(params RenderTarget[] views)
        {
            if (views == null) throw new ArgumentNullException("views");

            SetOutputInternal(views);
            return this;
        }

        /// <summary>
        /// Draws a full screen quad using iterating on each pass of this effect.
        /// </summary>
        public void Draw(string name = null)
        {
            if (!Enabled)
            {
                return;
            }

            PreDrawCore(name);
            DrawCore();
            PostDrawCore();
        }

        protected virtual void PreDrawCore(string name)
        {
            GraphicsDevice.BeginProfile(Color.Green, name ?? Name);

            if (outputRenderTargetView != null)
            {
                GraphicsDevice.SetRenderTarget(outputRenderTargetView);
            }
            else if (outputRenderTargetViews != null)
            {
                GraphicsDevice.SetRenderTargets(outputRenderTargetViews);
            }
        }

        protected virtual void PostDrawCore()
        {
            GraphicsDevice.EndProfile();
        }

        /// <summary>
        /// Draws this post effect for a specific pass, implementation dependent.
        /// </summary>
        protected virtual void DrawCore()
        {

        }

        protected virtual void SetOutputInternal(RenderTarget view)
        {
            outputRenderTargetView = view;
            outputRenderTargetViews = null;
        }

        protected virtual void SetOutputInternal(params RenderTarget[] views)
        {
            outputRenderTargetView = null;
            outputRenderTargetViews = views;
        }

        protected RenderTarget GetSafeOutput(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", string.Format("Invald texture outputindex [{0}] cannot be negative for effect [{1}]", index, Name));
            }

            var renderTexture = outputRenderTargetView ?? (outputRenderTargetViews != null ? outputRenderTargetViews[index] : null);
            if (renderTexture == null)
            {
                throw new InvalidOperationException(string.Format("Expecting texture output on slot [{0}]", index));
            }

            return renderTexture;
        }
   }
}