// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering
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

        public void Prepare(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
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

            if (SceneCameraRenderer != null)
            {
                CurrentCullingMask = SceneCameraRenderer.CullingMask;
            }

            PrepareCore(context, opaqueList, transparentList);
        }

        public void Draw(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            if (Enabled)
            {
                PreDrawCoreInternal(context);
                DrawCore(context, renderItems, fromIndex, toIndex);
                PostDrawCoreInternal(context);
            }
        }

        protected abstract void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList);

        protected abstract void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex);
    }
}