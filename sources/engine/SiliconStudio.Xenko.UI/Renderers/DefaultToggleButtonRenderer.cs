// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="ToggleButton"/>.
    /// </summary>
    internal class DefaultToggleButtonRenderer : ElementRenderer
    {
        public DefaultToggleButtonRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);

            var toggleButton = (ToggleButton)element;
            var color = toggleButton.RenderOpacity * Color.White;

            var image = GetToggleStateImage(toggleButton);
            if (image?.Texture == null)
                return;
            
            Batch.DrawImage(image.Texture, ref toggleButton.WorldMatrixInternal, ref image.RegionInternal, ref toggleButton.RenderSizeInternal, ref image.BordersInternal, ref color, context.DepthBias, image.Orientation);
        }

        private static Sprite GetToggleStateImage(ToggleButton toggleButton)
        {
            switch (toggleButton.State)
            {
                case ToggleState.Checked:
                    return toggleButton.CheckedImage?.GetSprite();
                case ToggleState.Indeterminate:
                    return toggleButton.IndeterminateImage?.GetSprite();
                case ToggleState.UnChecked:
                    return toggleButton.UncheckedImage?.GetSprite();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
