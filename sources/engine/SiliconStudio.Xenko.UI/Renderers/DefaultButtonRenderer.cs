// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="Button"/>.
    /// </summary>
    internal class DefaultButtonRenderer : ElementRenderer
    {
        public DefaultButtonRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);

            var button = (Button)element;
            var color = button.RenderOpacity * Color.White;
            
            var image = button.ButtonImage;
            if(image?.Texture == null)
                return;

            Batch.DrawImage(image.Texture, ref element.WorldMatrixInternal, ref image.RegionInternal, ref element.RenderSizeInternal, ref image.BordersInternal, ref color, context.DepthBias, image.Orientation);
        }
    }
}
