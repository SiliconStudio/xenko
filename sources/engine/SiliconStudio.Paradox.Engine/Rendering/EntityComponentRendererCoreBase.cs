// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// A default implementation for a <see cref="IEntityComponentRenderer"/>.
    /// </summary>
    public abstract class EntityComponentRendererCoreBase : RendererCoreBase, IEntityComponentRendererCore
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the entity system.
        /// </summary>
        /// <value>The entity system.</value>
        public SceneInstance SceneInstance { get; private set; }

        /// <summary>
        /// Gets the effect system.
        /// </summary>
        /// <value>The effect system.</value>
        public EffectSystem EffectSystem { get; private set; }

        /// <summary>
        /// Gets the camera renderer (can be null).
        /// </summary>
        /// <value>The camera renderer.</value>
        public SceneCameraRenderer SceneCameraRenderer { get; private set; }

        /// <summary>
        /// Gets the current culling mask.
        /// </summary>
        /// <value>The current culling mask.</value>
        public EntityGroupMask CurrentCullingMask { get; set; }

        /// <summary>
        /// Gets the current render frame. Only valid from <see cref="RendererBase.DrawCore"/> method.
        /// </summary>
        /// <value>The current render frame.</value>
        public RenderFrame CurrentRenderFrame { get; private set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            Services = Context.Services;
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            SceneInstance = SceneInstance.GetCurrent(Context);
            SceneCameraRenderer = Context.Tags.Get(SceneCameraRenderer.Current);
            if (SceneCameraRenderer != null)
            {
                CurrentCullingMask = SceneCameraRenderer.CullingMask;
            }
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