// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="TextBlock"/>.
    /// </summary>
    internal class DefaultScrollingTextRenderer : ElementRenderer
    {
        private static Color blackColor;

        public DefaultScrollingTextRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);

            var scrollingText = (ScrollingText)element;

            if (scrollingText.Font == null || scrollingText.TextToDisplay == null)
                return;

            var offset = scrollingText.ScrollingOffset;
            var textWorldMatrix = element.WorldMatrix;
            textWorldMatrix.M41 += textWorldMatrix.M11 * offset;
            textWorldMatrix.M42 += textWorldMatrix.M12 * offset;
            textWorldMatrix.M43 += textWorldMatrix.M13 * offset;
            textWorldMatrix.M44 += textWorldMatrix.M14 * offset;

            // create the scrolling text draw command
            var drawCommand = new SpriteFont.InternalUIDrawCommand
            {
                Color = scrollingText.RenderOpacity * scrollingText.TextColor,
                DepthBias = context.DepthBias + 1,
                FontScale = element.LayoutingContext.RealVirtualResolutionRatio,
                FontSize = scrollingText.TextSize,
                Batch = Batch,
                SnapText = context.ShouldSnapText && !scrollingText.DoNotSnapText,
                Matrix = textWorldMatrix,
                Alignment = TextAlignment.Left,
                Size = new Vector2(scrollingText.ActualWidth, scrollingText.ActualHeight)
            };

            // flush the current content of the UI image batch
            Batch.End();

            // draw a clipping mask 
            Batch.Begin(context.GraphicsContext, ref context.ViewProjectionMatrix, BlendStates.ColorDisabled, IncreaseStencilValueState, context.StencilTestReferenceValue);
            Batch.DrawRectangle(ref element.WorldMatrixInternal, ref element.RenderSizeInternal, ref blackColor, context.DepthBias);
            Batch.End();

            // draw the element it-self with stencil test value of "Context.Value + 1"
            Batch.Begin(context.GraphicsContext, ref context.ViewProjectionMatrix, BlendStates.AlphaBlend, KeepStencilValueState, context.StencilTestReferenceValue + 1);
            Batch.DrawString(scrollingText.Font, scrollingText.TextToDisplay, ref drawCommand);
            Batch.End();

            // un-draw the clipping mask
            Batch.Begin(context.GraphicsContext, ref context.ViewProjectionMatrix, BlendStates.ColorDisabled, DecreaseStencilValueState, context.StencilTestReferenceValue + 1);
            Batch.DrawRectangle(ref element.WorldMatrixInternal, ref element.RenderSizeInternal, ref blackColor, context.DepthBias+2);
            Batch.End();

            // restart the Batch session
            Batch.Begin(context.GraphicsContext, ref context.ViewProjectionMatrix, BlendStates.AlphaBlend, KeepStencilValueState, context.StencilTestReferenceValue);
        }
    }
}