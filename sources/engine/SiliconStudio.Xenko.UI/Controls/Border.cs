// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// A border element adds an uniform color border around its content.
    /// </summary>
    [DataContract]
    public class Border : ContentControl
    {
        internal Color BorderColorInternal = Color.Black;
        private Thickness borderThickness = Thickness.UniformCuboid(0);

        /// <summary>
        /// Gets or sets the padding inside a control.
        /// </summary>
        public Thickness BorderThickness
        {
            get { return borderThickness; }
            set
            {
                borderThickness = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the color of the borders.
        /// </summary>
        public Color BorderColor
        {
            get { return BorderColorInternal; }
            set { BorderColorInternal = value; }
        }

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            var availableLessBorders = CalculateSizeWithoutThickness(ref availableSizeWithoutMargins, ref borderThickness);

            var neededSize = base.MeasureOverride(availableLessBorders);

            return CalculateSizeWithThickness(ref neededSize, ref borderThickness);
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            // arrange the content
            if (VisualContent != null)
            {
                // calculate the remaining space for the child after having removed the padding and border space.
                var availableLessBorders = CalculateSizeWithoutThickness(ref finalSizeWithoutMargins, ref borderThickness);
                var childSizeWithoutPadding = CalculateSizeWithoutThickness(ref availableLessBorders, ref padding);

                // arrange the child
                VisualContent.Arrange(childSizeWithoutPadding, IsCollapsed);

                // compute the rendering offsets of the child element wrt the parent origin (0,0,0)
                var childOffsets = new Vector3(padding.Left + borderThickness.Left, padding.Top + borderThickness.Top, padding.Front + borderThickness.Front) - finalSizeWithoutMargins / 2;

                // set the arrange matrix of the child.
                VisualContent.DependencyProperties.Set(ContentArrangeMatrixPropertyKey, Matrix.Translation(childOffsets));
            }

            return finalSizeWithoutMargins;
        }
    }
}
