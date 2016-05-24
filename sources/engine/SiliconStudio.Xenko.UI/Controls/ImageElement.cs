// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a control that displays an image.
    /// </summary>
    [DataContract(nameof(ImageElement))]
    [DebuggerDisplay("ImageElement - Name={Name}")]
    public class ImageElement : UIElement
    {
        private ISpriteProvider source;
        private Sprite sprite;
        private StretchType stretchType = StretchType.Uniform;
        private StretchDirection stretchDirection = StretchDirection.Both;

        /// <summary>
        /// Gets or sets a value that describes how an Image should be stretched to fill the destination rectangle.
        /// </summary>
        [DataMember]
        [Display(category: LayoutCategory)]
        [DefaultValue(StretchType.Uniform)]
        public StretchType StretchType
        {
            get { return stretchType; }
            set
            {
                stretchType = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates how the image is scaled.
        /// </summary>
        [DataMember]
        [Display(category: LayoutCategory)]
        [DefaultValue(StretchDirection.Both)]
        public StretchDirection StretchDirection
        {
            get { return stretchDirection; }
            set
            {
                stretchDirection = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ISpriteProvider"/> for the image.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider Source
        {
            get { return source;} 
            set
            {
                if (source == value)
                    return;

                source = value;
                OnSpriteChanged(source?.GetSprite());
            }
        }

        /// <summary>
        /// Gets or set the color used to tint the image. Default value is white.
        /// </summary>
        /// <remarks>The initial image color is multiplied by this color.</remarks>
        [DataMember]
        [Display(category: AppearanceCategory)]
        public Color Color { get; set; } = Color.White;

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            return ImageSizeHelper.CalculateImageSizeFromAvailable(sprite, finalSizeWithoutMargins, StretchType, StretchDirection, false);
        }

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            return ImageSizeHelper.CalculateImageSizeFromAvailable(sprite, availableSizeWithoutMargins, StretchType, StretchDirection, true);
        }

        protected override void Update(GameTime time)
        {
            var currentSprite = source?.GetSprite();
            if (sprite != currentSprite)
            {
                OnSpriteChanged(currentSprite);
            }
        }

        private void InvalidateMeasure(object sender, EventArgs eventArgs)
        {
            InvalidateMeasure();
        }

        private void OnSpriteChanged(Sprite currentSprite)
        {
            if (sprite != null)
            {
                sprite.SizeChanged -= InvalidateMeasure;
                sprite.BorderChanged -= InvalidateMeasure;
            }
            sprite = currentSprite;
            InvalidateMeasure();
            if (sprite != null)
            {
                sprite.SizeChanged += InvalidateMeasure;
                sprite.BorderChanged += InvalidateMeasure;
            }
        }
    }
}
