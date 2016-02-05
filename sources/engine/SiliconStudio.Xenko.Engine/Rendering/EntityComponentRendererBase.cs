// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A default implementation for a <see cref="IEntityComponentRenderer"/>.
    /// </summary>
    public abstract class EntityComponentRendererBase : EntityComponentRendererCoreBase, IEntityComponentRenderer
    {
        /// <summary>
        /// Gets the current culling mask.
        /// </summary>
        /// <value>The current culling mask.</value>
        protected EntityGroupMask CurrentCullingMask { get; private set; }

        public virtual bool SupportPicking
        {
            get
            {
                return false;
            }
        }

        public void Prepare(RenderDrawContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            if (!Enabled)
            {
                return;
            }

            if (Context == null)
            {
                Initialize(context.RenderContext);
            }
            else if (Context != context.RenderContext)
            {
                throw new InvalidOperationException("Cannot use a different context between Load and Draw");
            }

            if (SceneCameraRenderer != null)
            {
                CurrentCullingMask = SceneCameraRenderer.CullingMask;
            }

            PrepareCore(context, opaqueList, transparentList);
        }

        public void Draw(RenderDrawContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            if (Enabled)
            {
                PreDrawCoreInternal(context);
                DrawCore(context, renderItems, fromIndex, toIndex);
                PostDrawCoreInternal(context);
            }
        }

        protected abstract void PrepareCore(RenderDrawContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList);

        protected abstract void DrawCore(RenderDrawContext context, RenderItemCollection renderItems, int fromIndex, int toIndex);
    }
}