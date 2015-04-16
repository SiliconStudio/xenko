// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="ScrollBar"/>.
    /// </summary>
    internal class DefaultScrollBarRenderer : ElementRenderer
    {
        public DefaultScrollBarRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);
            
            var bar = (ScrollBar)element;

            // round the size of the bar to nearest pixel modulo to avoid to have a bar varying by one pixel length while scrolling
            var barSize = bar.RenderSizeInternal;
            var realVirtualRatio = bar.LayoutingContext.RealVirtualResolutionRatio;
            for (int i = 0; i < 2; i++)
                barSize[i] = (float)(Math.Ceiling(barSize[i] * realVirtualRatio[i]) / realVirtualRatio[i]);
            
            Batch.DrawRectangle(ref element.WorldMatrixInternal, ref barSize, ref bar.BarColorInternal, context.DepthBias);
        }
    }
}