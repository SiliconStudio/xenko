// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a Windows button control, which reacts to the Click event.
    /// </summary>
    [DataContract(nameof(Button))]
    [DebuggerDisplay("Button - Name={Name}")]
    public class Button : ButtonBase
    {
        private ISpriteProvider pressedImage;
        private ISpriteProvider notPressedImage;
        private ISpriteProvider mouseOverImage;
        private bool sizeToContent = true;

        public Button()
        {
            DrawLayerNumber += 1; // (button design image)
            Padding = new Thickness(10, 5, 10, 7);

            MouseOverStateChanged += (sender, args) => InvalidateButtonImage();
        }

        /// <inheritdoc/>
        public override bool IsPressed
        {
            get { return base.IsPressed; }
            protected set
            {
                if (value == IsPressed)
                    return;

                base.IsPressed = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when pressed.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider PressedImage
        {
            get { return pressedImage; }
            set
            {
                if (pressedImage == value)
                    return;

                pressedImage = value;
                OnAspectImageInvalidated();
            }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when not pressed.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider NotPressedImage
        {
            get { return notPressedImage; }
            set
            {
                if (notPressedImage == value)
                    return;

                notPressedImage = value;
                OnAspectImageInvalidated();
            }
        }

        /// <summary>
        /// Gets or sets the image that the button displays when the mouse is over it.
        /// </summary>
        [DataMember]
        [Display(category: AppearanceCategory)]
        [DefaultValue(null)]
        public ISpriteProvider MouseOverImage
        {
            get { return mouseOverImage; }
            set
            {
                if (mouseOverImage == value)
                    return;

                mouseOverImage = value;
                OnAspectImageInvalidated();
            }
        }

        /// <summary>
        /// Gets or sets whether the size depends on the Content. The default is <c>true</c>.
        /// </summary>
        /// <userdoc>True if this button's size depends of its content, false otherwise.</userdoc>
        [DataMember]
        [Display(category: LayoutCategory)]
        [DefaultValue(true)]
        public bool SizeToContent
        {
            get { return sizeToContent; }
            set
            {
                if (sizeToContent == value)
                    return;

                sizeToContent = value;
                InvalidateMeasure();
            }
        }

        internal Sprite ButtonImage => (IsPressed ? PressedImage : MouseOverState == MouseOverState.MouseOverElement ? MouseOverImage : NotPressedImage)?.GetSprite();

        /// <inheritdoc/>
        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            return sizeToContent
                ? base.ArrangeOverride(finalSizeWithoutMargins)
                : CalculateImageSizeFromAvailable(finalSizeWithoutMargins);
        }

        /// <inheritdoc/>
        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            if (sizeToContent)
            {
                return base.MeasureOverride(availableSizeWithoutMargins);
            }

            // Note: copied from ImageElement
            var desiredSize = CalculateImageSizeFromAvailable(availableSizeWithoutMargins);

            var sprite = ButtonImage;
            if (sprite == null || !sprite.HasBorders)
                return desiredSize;

            var borderSum = new Vector2(sprite.BordersInternal.X + sprite.BordersInternal.Z, sprite.BordersInternal.Y + sprite.BordersInternal.W);
            if (sprite.Orientation == ImageOrientation.Rotated90)
                Utilities.Swap(ref borderSum.X, ref borderSum.Y);

            return new Vector3(Math.Max(desiredSize.X, borderSum.X), Math.Max(desiredSize.Y, borderSum.Y), desiredSize.Z);
        }

        /// <summary>
        /// Function triggered when one of the <see cref="PressedImage"/> and <see cref="NotPressedImage"/> images are invalidated.
        /// This function can be overridden in inherited classes.
        /// </summary>
        protected virtual void OnAspectImageInvalidated()
        {
            InvalidateButtonImage();
        }

        // Note: copied from ImageElement
        private Vector3 CalculateImageSizeFromAvailable(Vector3 availableSizeWithoutMargins)
        {
            var sprite = ButtonImage;
            if (sprite == null) // no associated image -> no region needed
                return Vector3.Zero;

            var idealSize = sprite.SizeInPixels;
            if (idealSize.X <= 0 || idealSize.Y <= 0) // image size null or invalid -> no region needed
                return Vector3.Zero;

            if (float.IsInfinity(availableSizeWithoutMargins.X) && float.IsInfinity(availableSizeWithoutMargins.Y)) // unconstrained available size -> take the best size for the image: the image size
                return new Vector3(idealSize, 0);

            // initialize the desired size with maximum available size
            var desiredSize = availableSizeWithoutMargins;

            // compute the desired image ratios
            var desiredScale = new Vector2(desiredSize.X / idealSize.X, desiredSize.Y / idealSize.Y);

            // when the size along a given axis is free take the same ratio as the constrained axis.
            if (float.IsInfinity(desiredScale.X))
                desiredScale.X = desiredScale.Y;
            if (float.IsInfinity(desiredScale.Y))
                desiredScale.Y = desiredScale.X;

            // adjust the scales
            desiredScale.X = desiredScale.Y = Math.Min(desiredScale.X, desiredScale.Y);

            // update the desired size based on the desired scales
            desiredSize = new Vector3(idealSize.X * desiredScale.X, idealSize.Y * desiredScale.Y, 0f);

            return desiredSize;
        }

        private void InvalidateButtonImage()
        {
            if (!sizeToContent)
                InvalidateMeasure();
        }
    }
}
