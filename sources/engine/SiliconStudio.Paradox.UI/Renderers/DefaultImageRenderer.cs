// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Renderers
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
            var imageColor = element.RenderOpacity * Color.White;

            if(image.Source == null)
                return;
            
            Batch.DrawImage(image.Source.Texture, image.Source.TextureAlpha, ref image.WorldMatrixInternal, ref image.Source.RegionInternal,
                ref element.RenderSizeInternal, ref image.Source.BordersInternal, ref imageColor, context.DepthBias, image.Source.Orientation);
        }
    }
}