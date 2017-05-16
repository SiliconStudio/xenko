// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public interface ISceneRenderer : IRenderCollector, IGraphicsRenderer
    {
    }

    public interface ISharedRenderer : IIdentifiable, IGraphicsRendererBase
    {
        string Name { get; }
    }

    /// <summary>
    /// Describes the code part of a <see cref="GraphicsCompositor"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class SceneRendererBase : RendererCoreBase, ISceneRenderer
    {
        /// <inheritdoc/>
        [DataMember(-100), Display(Browsable = false)]
        [NonOverridable]
        public Guid Id { get; set; }

        public override string Name => GetType().Name;

        protected SceneRendererBase()
        {
            Id = Guid.NewGuid();
        }

        /// <inheritdoc/>
        public void Collect(RenderContext context)
        {
            EnsureContext(context);

            CollectCore(context);
        }

        /// <inheritdoc/>
        public void Draw(RenderDrawContext context)
        {
            if (Enabled)
            {
                PreDrawCoreInternal(context);
                DrawCore(context.RenderContext, context);
                PostDrawCoreInternal(context);
            }
        }

        /// <summary>
        /// Main collect method.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void CollectCore(RenderContext context)
        {
        }

        /// <summary>
        /// Main drawing method for this renderer that must be implemented. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="drawContext"></param>
        protected abstract void DrawCore(RenderContext context, RenderDrawContext drawContext);
    }
}
