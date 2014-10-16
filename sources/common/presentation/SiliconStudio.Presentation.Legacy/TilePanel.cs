// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// This class describes a Panel similar to a <see cref="WrapPanel"/> except that items have a reserved space that is equal to the size of the largest items. Every item
    /// is aligned vertically and horizontally such as if they were in a grid.
    /// </summary>
    [Obsolete("This class is not maintained and might contain bugs. Use VirtualizingTilePanel instead.")]
    public class TilePanel : Panel
    {
        /// <summary>
        /// Size of the largest item contained in the Tile Panel. This field is computed by <see cref="MeasureOverride"/> and used by <see cref="ArrangeOverride"/>
        /// </summary>
        private Size largestItem;

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(TilePanel),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Identifies the <see cref="MinimumItemSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumItemSpacingProperty = DependencyProperty.Register(
            "MinimumItemSpacing",
            typeof(double),
            typeof(TilePanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure),
            ValidateMinMaxItemSpacing);

        /// <summary>
        /// Identifies the <see cref="MaximumItemSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumItemSpacingProperty = DependencyProperty.Register(
            "MaximumItemSpacing",
            typeof(double),
            typeof(TilePanel),
            new FrameworkPropertyMetadata(double.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure),
            ValidateMinMaxItemSpacing);

        /// <summary>
        /// Identifies the <see cref="FallbackItemSlotSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackItemSlotSizeProperty = DependencyProperty.Register(
            "FallbackItemSlotSize",
            typeof(Size),
            typeof(TilePanel),
            new FrameworkPropertyMetadata(new Size(64.0, 64.0), FrameworkPropertyMetadataOptions.AffectsMeasure),
            ValidateSize);

        /// <summary>
        /// Gets or sets the orientation of the Tile Panel.
        /// </summary>
        public Orientation Orientation { get { return (Orientation)GetValue(OrientationProperty); } set { SetValue(OrientationProperty, value); } }

        /// <summary>
        /// Gets or sets the minimum spacing allowed between items.
        /// </summary>
        public double MinimumItemSpacing { get { return (double)GetValue(MinimumItemSpacingProperty); } set { SetValue(MinimumItemSpacingProperty, value); } }

        /// <summary>
        /// Gets or sets the maximum spacing allowed between items.
        /// </summary>
        public double MaximumItemSpacing { get { return (double)GetValue(MaximumItemSpacingProperty); } set { SetValue(MaximumItemSpacingProperty, value); } }

        /// <summary>
        /// Gets or sets the item slot size to use in case all items requires infinite or empty size.
        /// </summary>
        public Size FallbackItemSlotSize { get { return (Size)GetValue(FallbackItemSlotSizeProperty); } set { SetValue(FallbackItemSlotSizeProperty, value); } }

        //private double lineLength = double.PositiveInfinity;

        private int itemsPerLine = -1;
        private int lineCount = -1;

        private static bool ValidateMinMaxItemSpacing(object value)
        {
            if ((value is double) == false)
                return false;

            var v = (double)value;

            return v >= 0.0 && double.IsInfinity(v) == false;
        }

        private static bool ValidateSize(object value)
        {
            if ((value is Size) == false)
                return false;

            var size = (Size)value;

            return size.Width >= 1.0 &&
                   size.Height >= 1.0 &&
                   double.IsInfinity(size.Width) == false &&
                   double.IsInfinity(size.Height) == false;
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            largestItem = new Size();
            var totalItemCount = Children.Count;

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                if (double.IsInfinity(child.DesiredSize.Width) == false)
                    largestItem.Width = (child.DesiredSize.Width > largestItem.Width ? child.DesiredSize : largestItem).Width;
                if (double.IsInfinity(child.DesiredSize.Height) == false)
                    largestItem.Height = (child.DesiredSize.Height > largestItem.Height ? child.DesiredSize : largestItem).Height;
            }

            if (Math.Abs(largestItem.Width) <= 0.5)
                largestItem.Width = FallbackItemSlotSize.Width;
            if (Math.Abs(largestItem.Height) <= 0.5)
                largestItem.Height = FallbackItemSlotSize.Height;

            if (Orientation == Orientation.Vertical)
            {
                if (double.IsPositiveInfinity(availableSize.Width))
                    return new Size(MinimumItemSpacing + totalItemCount * (largestItem.Width + MinimumItemSpacing), largestItem.Height + 2 * MinimumItemSpacing);

                itemsPerLine = Math.Max(1, (int)(availableSize.Width / largestItem.Width));
                lineCount = ComputeLineCount(totalItemCount);

                var height = MinimumItemSpacing + lineCount * (largestItem.Height + MinimumItemSpacing);
                return new Size(availableSize.Width, height);
            }
            else
            {
                if (double.IsPositiveInfinity(availableSize.Height))
                    return new Size(MinimumItemSpacing + totalItemCount * (largestItem.Width + MinimumItemSpacing), largestItem.Height + 2 * MinimumItemSpacing);

                itemsPerLine = Math.Max(1, (int)(availableSize.Height / largestItem.Height));
                lineCount = ComputeLineCount(totalItemCount);

                var width = MinimumItemSpacing + lineCount * (largestItem.Width + MinimumItemSpacing);
                return new Size(width, availableSize.Height);
            }
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var space = ComputeItemSpacing(finalSize);

            var lineItemIndex = 0;
            var lineOffset = MinimumItemSpacing;

            foreach (UIElement child in Children)
            {
                var location = Orientation == Orientation.Vertical
                    ? new Point(MinimumItemSpacing + lineItemIndex * (largestItem.Width + space), lineOffset)
                    : new Point(lineOffset, MinimumItemSpacing + lineItemIndex * (largestItem.Height + space));

                child.Arrange(new Rect(location, largestItem));

                lineItemIndex++;
                if (lineItemIndex >= itemsPerLine)
                {
                    lineItemIndex = 0;
                    lineOffset += Orientation == Orientation.Vertical ? largestItem.Height : largestItem.Width;
                }
            }

            return finalSize;
        }

        private double ComputeItemSpacing(Size finalSize)
        {
            double innerSpace;

            if (Orientation == Orientation.Vertical)
            {
                var totalItemWidth = itemsPerLine * largestItem.Width;
                innerSpace = Math.Max(0.0, finalSize.Width - totalItemWidth - 2 * MinimumItemSpacing);
            }
            else
            {
                var totalItemHeight = itemsPerLine * largestItem.Height;
                innerSpace = Math.Max(0.0, finalSize.Height - totalItemHeight - 2 * MinimumItemSpacing);
            }

            return Math.Max(MinimumItemSpacing, Math.Min(MaximumItemSpacing, innerSpace / Math.Max(1, itemsPerLine - 1)));
        }

        private int ComputeLineCount(int totalItemCount)
        {
            return (totalItemCount + itemsPerLine - 1) / itemsPerLine;
        }
    }
}
