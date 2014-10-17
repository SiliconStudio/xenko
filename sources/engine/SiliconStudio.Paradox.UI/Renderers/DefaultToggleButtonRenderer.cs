using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Renderers
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

            var image = GetToggleStateImage(toggleButton);
            if (image == null || image.Texture == null)
                return;

            var imageSize = image.ImageIdealSize;
            var color = toggleButton.RenderOpacity * Color.White;

            Batch.DrawImage(image.Texture, image.TextureAlpha, ref toggleButton.WorldMatrixInternal, ref image.RegionInternal, ref toggleButton.RenderSizeInternal, ref imageSize, ref image.BordersInternal, ref color, context.DepthBias, image.Orientation);
        }

        private UIImage GetToggleStateImage(ToggleButton toggleButton)
        {
            switch (toggleButton.State)
            {
                case ToggleState.Checked:
                    return toggleButton.CheckedImage;
                case ToggleState.Indeterminate:
                    return toggleButton.IndeterminateImage;
                case ToggleState.UnChecked:
                    return toggleButton.UncheckedImage;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}