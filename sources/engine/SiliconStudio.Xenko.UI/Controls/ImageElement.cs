// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents a control that displays an image.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("ImageElement - Name={Name}")]
    public class ImageElement : UIElement
    {
        private Sprite source;
        private StretchType stretchType = StretchType.Uniform;
        private StretchDirection stretchDirection = StretchDirection.Both;

        /// <summary>
        /// Gets or sets a value that describes how an Image should be stretched to fill the destination rectangle.
        /// </summary>
        [DataMember]
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
        /// Gets or sets the <see cref="Sprite"/> for the image.
        /// </summary>
        [DataMember]
        public Sprite Source
        {
            get { return source;} 
            set
            {
                if (source != null)
                {
                    source.SizeChanged -= InvalidateMeasure;
                    source.BorderChanged -= InvalidateMeasure;
                }

                source = value;
                InvalidateMeasure();

                if (source != null)
                {
                    source.SizeChanged += InvalidateMeasure;
                    source.BorderChanged += InvalidateMeasure;
                }
            }
        }

        /// <summary>
        /// Gets or set the color used to tint the image. Default value is white.
        /// </summary>
        /// <remarks>The initial image color is multiplied by this color.</remarks>
        [DataMember]
        public Color Color { get; set; } = Color.White;

        private void InvalidateMeasure(object sender, EventArgs eventArgs)
        {
            InvalidateMeasure();
        }

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            var desiredSize = CalculateImageSizeFromAvailable(availableSizeWithoutMargins, true);

            if (source == null || !source.HasBorders)
                return desiredSize;

            var borderSum = new Vector2(source.BordersInternal.X + source.BordersInternal.Z, source.BordersInternal.Y + source.BordersInternal.W);
            if(source.Orientation == ImageOrientation.Rotated90)
                Utilities.Swap(ref borderSum.X, ref borderSum.Y);

            return new Vector3(Math.Max(desiredSize.X, borderSum.X), Math.Max(desiredSize.Y, borderSum.Y), desiredSize.Z);
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            return CalculateImageSizeFromAvailable(finalSizeWithoutMargins, false);
        }

        private Vector3 CalculateImageSizeFromAvailable(Vector3 availableSizeWithoutMargins, bool isMeasuring)
        {
            if (Source == null) // no associated image -> no region needed
                return Vector3.Zero;

            var idealSize = source.SizeInPixels;
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
            
            // adjust the scales depending on the type of stretch to apply
            switch (StretchType)
            {
                case StretchType.None:
                    desiredScale = Vector2.One;
                    break;
                case StretchType.Uniform:
                    desiredScale.X = desiredScale.Y = Math.Min(desiredScale.X, desiredScale.Y);
                    break;
                case StretchType.UniformToFill:
                    desiredScale.X = desiredScale.Y = Math.Max(desiredScale.X, desiredScale.Y);
                    break;
                case StretchType.FillOnStretch:
                    // if we are only measuring we prefer keeping the image resolution than using all the available space.
                    if (isMeasuring)
                        desiredScale.X = desiredScale.Y = Math.Min(desiredScale.X, desiredScale.Y); 
                    break;
                case StretchType.Fill:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // adjust the scales depending on the stretch directions
            switch (StretchDirection)
            {
                case StretchDirection.Both:
                    break;
                case StretchDirection.DownOnly:
                    desiredScale.X = Math.Min(desiredScale.X, 1);
                    desiredScale.Y = Math.Min(desiredScale.Y, 1);
                    break;
                case StretchDirection.UpOnly:
                    desiredScale.X = Math.Max(1, desiredScale.X);
                    desiredScale.Y = Math.Max(1, desiredScale.Y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // update the desired size based on the desired scales
            desiredSize = new Vector3(idealSize.X * desiredScale.X, idealSize.Y * desiredScale.Y, 0f);

            return desiredSize;
        }
    }
}
