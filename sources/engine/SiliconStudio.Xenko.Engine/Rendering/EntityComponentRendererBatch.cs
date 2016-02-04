// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A dedicated batch renderer of <see cref="IEntityComponentRenderer"/>
    /// </summary>
    public sealed class EntityComponentRendererBatch : GraphicsRendererCollectionBase<IEntityComponentRenderer>
    {
        private readonly RenderItemCollection opaqueRenderItems;

        private readonly RenderItemCollection transparentRenderItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsRendererCollection{IEntityComponentRenderer}"/> class.
        /// </summary>
        public EntityComponentRendererBatch()
        {
            opaqueRenderItems = new RenderItemCollection(1024, false);
            transparentRenderItems = new RenderItemCollection(1024, true);
        }

        protected override void DrawCore(RenderContext context)
        {
            opaqueRenderItems.Clear();
            transparentRenderItems.Clear();

            base.DrawCore(context);

            // Draw opaque (front to back)
            Draw(context, opaqueRenderItems, RenderItemFrontToBackSorter.Default);

            // Draw transparent (back to front)
            Draw(context, transparentRenderItems, RenderItemBackToFrontSorter.Default);
        }

        protected override void DrawRenderer(RenderContext context, IEntityComponentRenderer renderer)
        {
            if (!context.IsPicking() || renderer.SupportPicking)
                renderer.Prepare(context, opaqueRenderItems, transparentRenderItems);
        }

        private void Draw(RenderContext context, RenderItemCollection renderItems, IComparer<RenderItem> comparer)
        {
            // Early exit
            if (renderItems.Count == 0)
            {
                return;
            }

            // Sort the list
            renderItems.Sort(comparer);
            
            var renderer = renderItems[0].Renderer;
            int fromIndex = 0;
            var lastIndex = renderItems.Count - 1;

            for (int i = 0; i < renderItems.Count; i++)
            {
                var renderItem = renderItems[i];
                bool isNewRenderer = !ReferenceEquals(renderItem.Renderer, renderer);
                bool isLastIndex = i == lastIndex;

                if (isLastIndex)
                {
                    if (isNewRenderer)
                    {
                        DrawRendererInternal(context, renderer, renderItems, fromIndex, i - 1);

                        // TODO GRAPHICS REFACTOR
                        //context.GraphicsDevice.ResetStates();

                        DrawRendererInternal(context, renderItem.Renderer, renderItems, lastIndex, lastIndex);
                    }
                    else
                    {
                        DrawRendererInternal(context, renderer, renderItems, fromIndex, lastIndex);
                    }
                } 
                else if (isNewRenderer)
                {
                    DrawRendererInternal(context, renderer, renderItems, fromIndex, i - 1);
                    fromIndex = i;
                }


                renderer = renderItem.Renderer;
            }
        }

        private void DrawRendererInternal(RenderContext context, IEntityComponentRenderer renderer, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            var graphicsDevice = context.GraphicsDevice;
            graphicsDevice.PushState();
            renderer.Draw(context, renderItems, fromIndex, toIndex);
            graphicsDevice.PopState();
        }

        private class RenderItemFrontToBackSorter : IComparer<RenderItem>
        {
            public static readonly RenderItemFrontToBackSorter Default = new RenderItemFrontToBackSorter();

            public int Compare(RenderItem left, RenderItem right)
            {
                return left.Depth.CompareTo(right.Depth);
            }
        }

        private class RenderItemBackToFrontSorter : IComparer<RenderItem>
        {
            public static readonly RenderItemBackToFrontSorter Default = new RenderItemBackToFrontSorter();

            public int Compare(RenderItem left, RenderItem right)
            {
                return right.Depth.CompareTo(left.Depth);
            }
        }
    }
}