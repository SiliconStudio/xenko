// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using System.Text;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Provides a lightweight control for displaying small amounts of text.
    /// </summary>
    [DebuggerDisplay("TextBlock - Name={Name}")]
    public class TextBlock : UIElement
    {
        /// <summary>
        /// The key to the Font dependency property.
        /// </summary>
        public readonly static PropertyKey<SpriteFont> FontPropertyKey = new PropertyKey<SpriteFont>("FontKey", typeof(TextBlock), DefaultValueMetadata.Static<SpriteFont>(null), ObjectInvalidationMetadata.New<SpriteFont>(InvalidateFont));

        /// <summary>
        /// The key to the TextColor dependency property.
        /// </summary>
        public readonly static PropertyKey<Color> TextColorPropertyKey = new PropertyKey<Color>("TextColorKey", typeof(TextBlock), DefaultValueMetadata.Static(Color.FromAbgr(0xF0F0F0FF)));

        private static void InvalidateFont(object propertyOwner, PropertyKey propertyKey, object propertyOldValue)
        {
            var element = (TextBlock)propertyOwner;
            element.InvalidateMeasure();
        }

        private string text;

        private bool wrapText;

        private string wrappedText;

        private float? textSize;

        private bool synchronousCharacterGeneration;

        /// <summary>
        /// Method triggered when the <see cref="Text"/> changes.
        /// Can be overridden in inherited class to changed the default behavior.
        /// </summary>
        protected virtual void OnTextChanged()
        {
            InvalidateMeasure();
        }

        /// <summary>
        /// Returns the text to display during the draw call.
        /// </summary>
        public virtual string TextToDisplay
        {
            get { return WrapText? wrappedText: Text; }
        }

        /// <summary>
        /// Gets or sets the font of the text block
        /// </summary>
        public SpriteFont Font
        {
            get { return DependencyProperties.Get(FontPropertyKey); }
            set { DependencyProperties.Set(FontPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the text of the text block
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnTextChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text of the text block
        /// </summary>
        public Color TextColor
        {
            get { return DependencyProperties.Get(TextColorPropertyKey); }
            set { DependencyProperties.Set(TextColorPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the size of the text in virtual pixels unit
        /// </summary>
        public float TextSize
        {
            get
            {
                if (textSize.HasValue)
                    return textSize.Value;

                if (Font != null)
                    return Font.Size;

                return 0;
            }
            set
            {
                textSize = Math.Max(0, Math.Min(float.MaxValue, value));

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the value indicating if the <see cref="Text"/> of the <see cref="TextBlock"/> 
        /// should automatically return to the beginning of the line when it is too big for the line width.
        /// </summary>
        public bool WrapText
        {
            get { return wrapText; }
            set
            {
                if(wrapText == value)
                    return;

                wrapText = value;

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the value indicating if the text block should generate <see cref="RuntimeRasterizedSpriteFont"/> characters synchronously or asynchronously.
        /// </summary>
        /// <remarks>If synchronous generation is activated, the game will be block until all the characters have finished to be generate.
        /// If asynchronous generation is activated, some characters can appears with one or two frames of delay.</remarks>
        public bool SynchronousCharacterGeneration
        {
            get { return synchronousCharacterGeneration; }
            set
            {
                if(synchronousCharacterGeneration == value)
                    return;

                synchronousCharacterGeneration = value;

                if (IsMeasureValid && synchronousCharacterGeneration)
                    CalculateTextSize();
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the text to display.
        /// </summary>
        public TextAlignment TextAlignment { get; set; }

        /// <summary>
        /// Gets or sets the value indicating if the snapping of the <see cref="Text"/> of the <see cref="TextBlock"/> to the closest screen pixel should be skipped.
        /// </summary>
        /// <remarks>When <value>true</value>, the element's text is never snapped. 
        /// When <value>false</value>, it is snapped only if the font is dynamic and the element is rendered by a SceneUIRenderer.</remarks>
        public bool DoNotSnapText { get; set; }

        /// <summary>
        /// Calculate and returns the size of the <see cref="Text"/> in virtual pixels size.
        /// </summary>
        /// <returns>The size of the Text in virtual pixels.</returns>
        public Vector2 CalculateTextSize()
        {
            return CalculateTextSize(TextToDisplay);
        }

        /// <summary>
        /// Calculate and returns the size of the provided <paramref name="textToMeasure"/>"/> in virtual pixels size.
        /// </summary>
        /// <param name="textToMeasure">The text to measure</param>
        /// <returns>The size of the text in virtual pixels</returns>
        protected Vector2 CalculateTextSize(string textToMeasure)
        {
            if (textToMeasure == null)
                return Vector2.Zero;

            return CalculateTextSize(new SpriteFont.StringProxy(textToMeasure));
        }

        private Vector2 CalculateTextSize(StringBuilder textToMeasure)
        {
            return CalculateTextSize(new SpriteFont.StringProxy(textToMeasure));
        }

        private Vector2 CalculateTextSize(SpriteFont.StringProxy textToMeasure)
        {
            if (Font == null)
                return Vector2.Zero;

            var sizeRatio = LayoutingContext.RealVirtualResolutionRatio;
            var measureFontSize = new Vector2(sizeRatio.Y * TextSize); // we don't want letters non-uniform ratio
            var realSize = Font.MeasureString(ref textToMeasure, ref measureFontSize);

            // force pre-generation if synchronous generation is required
            if(SynchronousCharacterGeneration)
                Font.PreGenerateGlyphs(ref textToMeasure, ref measureFontSize);

            if (Font.FontType == SpriteFontType.Dynamic)
            {
                // rescale the real size to the virtual size
                realSize.X /= sizeRatio.X;
                realSize.Y /= sizeRatio.Y;
            }

            if (Font.FontType == SpriteFontType.SDF)
            {
                var scaleRatio = TextSize / Font.Size;
                realSize.X *= scaleRatio;
                realSize.Y *= scaleRatio;
            }

            return realSize;
        } 

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            if (WrapText)
                UpdateWrappedText(availableSizeWithoutMargins);

            return new Vector3(CalculateTextSize(), 0);
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            if (WrapText)
                UpdateWrappedText(finalSizeWithoutMargins);

            return base.ArrangeOverride(finalSizeWithoutMargins);
        }

        private void UpdateWrappedText(Vector3 availableSpace)
        {
            var availableWidth = availableSpace.X;
            var currentLine = new StringBuilder(text.Length);
            var currentText = new StringBuilder(2 * text.Length);

            var indexOfNewLine = 0;
            while (true)
            {
                float lineCurrentSize;
                var indexNextCharacter = 0;
                var indexOfLastSpace = -1;
                
                while (true)
                {
                    lineCurrentSize = CalculateTextSize(currentLine).X;

                    if (lineCurrentSize > availableWidth || indexOfNewLine + indexNextCharacter >= text.Length)
                        break;

                    var currentCharacter = text[indexOfNewLine + indexNextCharacter];

                    if (currentCharacter == '\n')
                    {
                        indexOfNewLine += indexNextCharacter + 1;
                        goto AppendLine;
                    }

                    currentLine.Append(currentCharacter);

                    if (char.IsWhiteSpace(currentCharacter))
                        indexOfLastSpace = indexNextCharacter;

                    ++indexNextCharacter;

                }

                if (lineCurrentSize <= availableWidth) // we reached the end of the text.
                {
                    // append the final part of the text and quit the main loop
                    currentText.Append(currentLine);
                    break;
                }

                // we reached the end of the line.
                if (indexOfLastSpace < 0) // no space in the line
                {
                    // remove last extra character
                    currentLine.Remove(currentLine.Length - 1, 1);
                    indexOfNewLine += indexNextCharacter - 1;
                }
                else // at least one white space in the line
                {
                    // remove all extra characters until last space (included)
                    if(indexNextCharacter > indexOfLastSpace)
                        currentLine.Remove(indexOfLastSpace, indexNextCharacter - indexOfLastSpace);
                    indexOfNewLine += indexOfLastSpace + 1;
                }

            AppendLine:

                // add the next line to the current text
                currentLine.Append('\n');
                currentText.Append(currentLine);

                // reset current line
                currentLine.Clear();
            }

            wrappedText = currentText.ToString();
        }
    }
}
