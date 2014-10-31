// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Renderers
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

            var image = button.PressedImage;
            if(!button.IsPressed)
                image = button.MouseOverState == MouseOverState.MouseOverElement? button.MouseOverImage : button.NotPressedImage;

            if(image == null || image.Texture == null)
                return;
            
            Batch.DrawImage(image.Texture, image.TextureAlpha, ref button.WorldMatrixInternal, ref image.RegionInternal, ref button.RenderSizeInternal, ref image.BordersInternal, ref color, context.DepthBias, image.Orientation);
        }
    }
}