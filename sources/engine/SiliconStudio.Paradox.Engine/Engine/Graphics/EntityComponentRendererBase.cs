// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A default implementation for a <see cref="IEntityComponentRenderer"/>.
    /// </summary>
    public abstract class EntityComponentRendererBase : RendererBase, IEntityComponentRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComponentRendererBase" /> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">services</exception>
        protected EntityComponentRendererBase()
        {
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the entity system.
        /// </summary>
        /// <value>The entity system.</value>
        public EntityManager EntityManager { get; private set; }

        /// <summary>
        /// Gets the effect system.
        /// </summary>
        /// <value>The effect system.</value>
        public EffectSystem EffectSystem { get; private set; }

        /// <summary>
        /// Gets the camera renderer.
        /// </summary>
        /// <value>The camera renderer.</value>
        public SceneCameraRenderer SceneCameraRenderer { get; private set; }

        /// <summary>
        /// Gets the current render frame. Only valid from <see cref="RendererBase.DrawCore"/> method.
        /// </summary>
        /// <value>The current render frame.</value>
        public RenderFrame CurrentRenderFrame { get; private set; }

        public override void Load(RenderContext context)
        {
            base.Load(context);
            Services = context.Services;
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            EntityManager = context.Tags.GetSafe(EntityManager.Current);
            SceneCameraRenderer = context.Tags.GetSafe(SceneCameraRenderer.Current);
        }

        protected override void PreDrawCore(RenderContext context)
        {
            base.PreDrawCore(context);
            CurrentRenderFrame = context.Tags.GetSafe(RenderFrame.Current);
        }

        protected override void PostDrawCore(RenderContext context)
        {
            base.PostDrawCore(context);
            CurrentRenderFrame = null;
        }
    }
}