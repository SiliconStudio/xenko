// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="ImageElement"/>.
    /// </summary>
    internal class DefaultImageRenderer : ElementRenderer
    {
        public DefaultImageRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);

            var image = (ImageElement)element;
            var imageColor = element.RenderOpacity * image.Color;

            var source = image.Source?.GetSprite();
            if (source == null)
                return;

            Batch.DrawImage(source.Texture, ref image.WorldMatrixInternal, ref source.RegionInternal,
                ref element.RenderSizeInternal, ref source.BordersInternal, ref imageColor, context.DepthBias, source.Orientation);
        }
    }
}
