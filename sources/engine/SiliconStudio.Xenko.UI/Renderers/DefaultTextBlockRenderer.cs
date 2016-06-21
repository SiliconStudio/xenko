// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="TextBlock"/>.
    /// </summary>
    internal class DefaultTextBlockRenderer : ElementRenderer
    {
        public DefaultTextBlockRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);

            var textBlock = (TextBlock)element;

            if (textBlock.Font == null || textBlock.TextToDisplay == null)
                return;
            
            var drawCommand = new SpriteFont.InternalUIDrawCommand
            {
                Color = textBlock.RenderOpacity * textBlock.TextColor,
                DepthBias = context.DepthBias,
                RealVirtualResolutionRatio = element.LayoutingContext.RealVirtualResolutionRatio,
                RequestedFontSize = textBlock.TextSize,
                Batch = Batch,
                SnapText = context.ShouldSnapText && !textBlock.DoNotSnapText,
                Matrix = textBlock.WorldMatrixInternal,
                Alignment = textBlock.TextAlignment,
                TextBoxSize = new Vector2(textBlock.ActualWidth, textBlock.ActualHeight)
            };

            if (textBlock.Font.FontType == SpriteFontType.SDF)
            {
                Batch.End();

                Batch.BeginCustom(context.GraphicsContext, 1);                
            }

            Batch.DrawString(textBlock.Font, textBlock.TextToDisplay, ref drawCommand);

            if (textBlock.Font.FontType == SpriteFontType.SDF)
            {
                Batch.End();

                Batch.BeginCustom(context.GraphicsContext, 0);
            }
        }
    }
}
