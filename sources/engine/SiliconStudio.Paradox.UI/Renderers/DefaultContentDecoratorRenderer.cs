// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="ContentDecorator"/>.
    /// </summary>
    internal class DefaultContentDecoratorRenderer : ElementRenderer
    {
        public DefaultContentDecoratorRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);

            var decorator = (ContentDecorator)element;

            if (decorator.BackgroundImage == null)
                return;

            var image = decorator.BackgroundImage;
            var color = decorator.RenderOpacity * Color.White;

            Batch.DrawImage(image.Texture, image.TextureAlpha, ref decorator.WorldMatrixInternal, ref image.RegionInternal,
                            ref decorator.RenderSizeInternal, ref image.BordersInternal, ref color, context.DepthBias, image.Orientation);
        }
    }
}