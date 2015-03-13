// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;
using IServiceRegistry = SiliconStudio.Core.IServiceRegistry;
using Vector3 = SiliconStudio.Core.Mathematics.Vector3;

namespace SiliconStudio.Paradox.UI.Renderers
{
    /// <summary>
    /// The default renderer for <see cref="EditText"/>.
    /// </summary>
    internal class DefaultEditTextRenderer : ElementRenderer
    {
        public DefaultEditTextRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void RenderColor(UIElement element, UIRenderingContext context)
        {
            base.RenderColor(element, context);

            var editText = (EditText)element;

            if (editText.Font == null)
                return;
            
            // determine the image to draw in background of the edit text
            var fontScale = element.LayoutingContext.RealVirtualResolutionRatio;
            var color = editText.RenderOpacity * Color.White;
            var image = editText.ActiveImage;
            if(!editText.IsSelectionActive)
                image = editText.MouseOverState == MouseOverState.MouseOverElement? editText.MouseOverImage : editText.InactiveImage;

            if (image != null && image.Texture != null)
            {
                Batch.DrawImage(image.Texture, image.TextureAlpha, ref editText.WorldMatrixInternal, ref image.RegionInternal, ref editText.RenderSizeInternal, ref image.BordersInternal, ref color, context.DepthBias, image.Orientation);
            }
            
            // calculate the size of the text region by removing padding
            var textRegionSize = new Vector2(editText.ActualWidth - editText.Padding.Left - editText.Padding.Right,
                                                editText.ActualHeight - editText.Padding.Top - editText.Padding.Bottom);

            var font = editText.Font;
            var selectionColor = editText.RenderOpacity * editText.SelectionColor;
            var caretColor = editText.RenderOpacity * editText.CaretColor;

            var offsetTextStart = 0f;
            var offsetAlignment = 0f;
            var selectionSize = 0f;
            
            // Draw the selection
            if(editText.IsSelectionActive)
            {
                var fontSize = new Vector2(fontScale.Y * editText.TextSize);
                offsetTextStart = font.MeasureString(editText.TextToDisplay, ref fontSize, editText.SelectionStart).X;
                selectionSize = font.MeasureString(editText.TextToDisplay, ref fontSize, editText.SelectionStart + editText.SelectionLength).X - offsetTextStart;
                if (font.IsDynamic)
                {
                    offsetTextStart /= fontScale.X;
                    selectionSize /= fontScale.X;
                }

                offsetAlignment = -textRegionSize.X / 2f;
                if (editText.TextAlignment != TextAlignment.Left)
                {
                    var textWidth = font.MeasureString(editText.TextToDisplay, ref fontSize).X;
                    if (font.IsDynamic)
                        textWidth /= fontScale.X;

                    offsetAlignment = editText.TextAlignment == TextAlignment.Center ? -textWidth / 2 : -textRegionSize.X / 2f + (textRegionSize.X - textWidth);
                }

                var selectionWorldMatrix = element.WorldMatrixInternal;
                selectionWorldMatrix.M41 += offsetTextStart + selectionSize / 2 + offsetAlignment;
                var selectionScaleVector = new Vector3(selectionSize, editText.LineCount * editText.Font.GetTotalLineSpacing(editText.TextSize), 0);
                Batch.DrawRectangle(ref selectionWorldMatrix, ref selectionScaleVector, ref selectionColor, context.DepthBias + 1);
            }

            // create the text draw command
            var drawCommand = new SpriteFont.InternalUIDrawCommand
            {
                Color = editText.RenderOpacity * editText.TextColor,
                DepthBias = context.DepthBias + 2,
                FontScale = fontScale,
                FontSize = editText.TextSize,
                Batch = Batch,
                SnapText = context.ShouldSnapText && !editText.DoNotSnapText,
                WorldMatrix = editText.WorldMatrixInternal,
                Alignment = editText.TextAlignment,
                Size = textRegionSize
            };

            // Draw the text
            Batch.DrawString(font, editText.TextToDisplay, ref drawCommand);

            // Draw the cursor
            if (editText.IsCaretVisible)
            {
                var sizeCaret = editText.CaretWidth / fontScale.X;
                var caretWorldMatrix = element.WorldMatrixInternal;
                caretWorldMatrix.M41 += offsetTextStart + offsetAlignment + (editText.CaretPosition > editText.SelectionStart? selectionSize: 0);
                var caretScaleVector = new Vector3(sizeCaret, editText.LineCount * editText.Font.GetTotalLineSpacing(editText.TextSize), 0);
                Batch.DrawRectangle(ref caretWorldMatrix, ref caretScaleVector, ref caretColor, context.DepthBias + 3);
            }
        }
    }
}