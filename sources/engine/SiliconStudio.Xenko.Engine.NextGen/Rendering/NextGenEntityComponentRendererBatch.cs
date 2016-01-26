// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A dedicated batch renderer of <see cref="IEntityComponentRenderer"/>
    /// </summary>
    public sealed class EntityComponentRendererBatch : GraphicsRendererCollectionBase<INextGenEntityComponentRenderer>, INextGenEntityComponentRenderer
    {
        public void Extract(NextGenRenderSystem renderSystem)
        {
            foreach (var componentRenderer in this)
            {
                componentRenderer.Extract(renderSystem);
            }
        }

        public void Draw(NextGenRenderSystem renderSystem)
        {
            foreach (var componentRenderer in this)
            {
                componentRenderer.Draw(renderSystem);
            }
        }

        protected override void DrawCore(RenderContext context)
        {
        }

        protected override void DrawRenderer(RenderContext context, INextGenEntityComponentRenderer renderer)
        {
        }
    }
}