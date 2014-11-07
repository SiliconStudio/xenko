// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// This class describes a Panel similar to a <see cref="WrapPanel"/> except that items have a reserved space that is equal to the size of the largest items. Every item
    /// is aligned vertically and horizontally such as if they were in a grid.
    /// </summary>
    public class VirtualizingTilePanel : VirtualizingPanel, IScrollInfo
    {
        private int itemsPerLine = -1;
        private int lineCount = -1;
        private int itemCount = -1;

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(VirtualizingTilePanel),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Identifies the <see cref="MinimumItemSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumItemSpacingProperty = DependencyProperty.Register(
            "MinimumItemSpacing",
            typeof(double),
            typeof(VirtualizingTilePanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure),
            ValidateMinMaxItemSpacing);

        /// <summary>
        /// Identifies the <see cref="MaximumItemSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumItemSpacingProperty = DependencyProperty.Register(
            "MaximumItemSpacing",
            typeof(double),
            typeof(VirtualizingTilePanel),
            new FrameworkPropertyMetadata(double.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure),
            ValidateMinMaxItemSpacing);

        /// <summary>
        /// Identifies the <see cref="ItemSlotSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemSlotSizeProperty = DependencyProperty.Register(
            "ItemSlotSize",
            typeof(Size),
            typeof(VirtualizingTilePanel),
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
        public Size ItemSlotSize { get { return (Size)GetValue(ItemSlotSizeProperty); } set { SetValue(ItemSlotSizeProperty, value); } }

        public int ItemsPerLine { get { return itemsPerLine; } }

        public int ItemCount { get { return itemCount; } }

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

        public void GetVisibilityRange(Size panelSize, out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            int firstVisibleLine;
            int lastVisibleLine;
            
            if (Orientation == Orientation.Vertical)
            {
                var itemHeightSpace = (int)Math.Ceiling(MinimumItemSpacing + ItemSlotSize.Height);
                firstVisibleLine = (int)Math.Ceiling(offset.Y) / itemHeightSpace;
                lastVisibleLine = firstVisibleLine + (int)Math.Ceiling(panelSize.Height) / itemHeightSpace + 1;
            }
            else
            {
                var itemWidthSpace = (int)Math.Ceiling(MinimumItemSpacing + ItemSlotSize.Width);
                firstVisibleLine = (int)Math.Ceiling(offset.X) / itemWidthSpace;
                lastVisibleLine = firstVisibleLine + (int)Math.Ceiling(panelSize.Width) / itemWidthSpace + 1;
            }
            
            firstVisibleItemIndex = firstVisibleLine * itemsPerLine;
            lastVisibleItemIndex = lastVisibleLine * itemsPerLine + itemsPerLine - 1;

            if (lastVisibleItemIndex >= itemCount)
                lastVisibleItemIndex = itemCount - 1;
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            // initialization side effect happens in the getter of the Children property
            // and is required for generator to be instanced properly, see below article for reference:
            // http://stackoverflow.com/questions/3289116/why-does-itemcontainergenerator-return-null
            // ReSharper disable once UnusedVariable
            var doNotRemove = Children;

            itemCount = -1;

            IItemContainerGenerator generator = ItemContainerGenerator;
            if (generator == null)
                return base.MeasureOverride(availableSize);

            var parentItemsControl = ItemsControl.GetItemsOwner(this);
            if (parentItemsControl == null)
                return base.MeasureOverride(availableSize);

            itemCount = parentItemsControl.Items.Count;
            itemsPerLine = itemCount;
            lineCount = itemsPerLine;

            Size desiredSize;
            if (Orientation == Orientation.Vertical)
            {
                if (double.IsPositiveInfinity(availableSize.Width))
                    throw new InvalidOperationException("Width must not be infinite when virtualizing vertically.");

                itemsPerLine = (int)Math.Ceiling(availableSize.Width - MinimumItemSpacing) / (int)(ItemSlotSize.Width + MinimumItemSpacing);
                itemsPerLine = Math.Max(1, Math.Min(itemsPerLine, itemCount));
                lineCount = ComputeLineCount(itemCount);
                desiredSize = new Size(availableSize.Width, lineCount * ItemSlotSize.Height);
            }
            else
            {
                if (double.IsPositiveInfinity(availableSize.Height))
                    throw new InvalidOperationException("Height must not be infinite when virtualizing horizontally.");

                itemsPerLine = (int)Math.Ceiling(availableSize.Height - MinimumItemSpacing) / (int)Math.Ceiling(ItemSlotSize.Height + MinimumItemSpacing);
                itemsPerLine = Math.Max(1, Math.Min(itemsPerLine, itemCount));
                lineCount = ComputeLineCount(itemCount);
                desiredSize = new Size(lineCount * ItemSlotSize.Width, availableSize.Height);
            }

            UpdateScrollInfo(availableSize);

            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibilityRange(availableSize, out firstVisibleItemIndex, out lastVisibleItemIndex);

            // Get the generator position of the first visible data item
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);

            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstVisibleItemIndex; itemIndex <= lastVisibleItemIndex; itemIndex++, childIndex++)
                {
                    bool newlyRealized;

                    // Get or create the child
                    var child = generator.GenerateNext(out newlyRealized) as UIElement;
                    if (child == null)
                        continue;

                    if (newlyRealized)
                    {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= itemCount)
                            AddInternalChild(child);
                        else
                            InsertInternalChild(childIndex, child);

                        generator.PrepareItemContainer(child);
                    }

                    child.Measure(ItemSlotSize);
                }
            }

            CleanUpItems(firstVisibleItemIndex, lastVisibleItemIndex);

            return desiredSize;
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // initialization side effect happens in the getter of the Children property
            // and is required for generator to be instanced properly, see bellow article for reference:
            // http://stackoverflow.com/questions/3289116/why-does-itemcontainergenerator-return-null
            // ReSharper disable once UnusedVariable
            var doNotRemove = Children;

            IItemContainerGenerator generator = ItemContainerGenerator;
            if (generator == null)
                return base.ArrangeOverride(finalSize);

            var parentItemsControl = ItemsControl.GetItemsOwner(this);
            if (parentItemsControl == null)
                return base.ArrangeOverride(finalSize);

            var space = ComputeItemSpacing(finalSize);

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];

                // Map the child offset to an item offset
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                if (Orientation == Orientation.Vertical)
                {
                    int row = itemIndex / itemsPerLine;
                    int column = itemIndex % itemsPerLine;

                    var absoluteY = MinimumItemSpacing + row * (ItemSlotSize.Height + MinimumItemSpacing);

                    child.Arrange(
                        new Rect(
                            new Point(
                                column * (ItemSlotSize.Width + space) - offset.X + MinimumItemSpacing,
                                absoluteY - offset.Y
                                ),
                            ItemSlotSize));
                }
                else
                {
                    int row = itemIndex % itemsPerLine;
                    int column = itemIndex / itemsPerLine;

                    var absoluteX = MinimumItemSpacing + column * (ItemSlotSize.Width + MinimumItemSpacing);

                    child.Arrange(
                        new Rect(
                            new Point(
                                absoluteX - offset.X,
                                row * (ItemSlotSize.Height + space) - offset.Y + MinimumItemSpacing
                                ),
                            ItemSlotSize));
                }
            }

            return finalSize;
        }

        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            UIElementCollection children = InternalChildren;
            IItemContainerGenerator generator = ItemContainerGenerator;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                var childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex >= 0 && (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated))
                {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private double ComputeItemSpacing(Size finalSize)
        {
            double itemsInnerSpace;

            if (Orientation == Orientation.Vertical)
            {
                var totalItemWidth = itemsPerLine * ItemSlotSize.Width;
                itemsInnerSpace = Math.Max(0.0, finalSize.Width - totalItemWidth - 2 * MinimumItemSpacing);
            }
            else
            {
                var totalItemHeight = itemsPerLine * ItemSlotSize.Height;
                itemsInnerSpace = Math.Max(0.0, finalSize.Height - totalItemHeight - 2 * MinimumItemSpacing);
            }

            var intervalCount = itemsPerLine - 1;
            if (intervalCount <= 0)
                return MinimumItemSpacing;

            return Math.Max(MinimumItemSpacing, Math.Min(itemsInnerSpace / intervalCount, MaximumItemSpacing));
        }

        private void UpdateScrollInfo(Size availableSize)
        {
            Size localExtent;

            if (Orientation == Orientation.Vertical)
            {
                localExtent = new Size(
                    Math.Max(availableSize.Width, 2.0 * MinimumItemSpacing + ItemSlotSize.Width),
                    MinimumItemSpacing + lineCount * (ItemSlotSize.Height + MinimumItemSpacing));
            }
            else
            {
                localExtent = new Size(
                    MinimumItemSpacing + lineCount * (ItemSlotSize.Width + MinimumItemSpacing),
                    Math.Max(availableSize.Height, 2.0 * MinimumItemSpacing + ItemSlotSize.Height));
            }

            // update extent
            if (localExtent != extent)
            {
                extent = localExtent;
                if (ScrollOwner != null)
                    ScrollOwner.InvalidateScrollInfo();

                Dispatcher.CurrentDispatcher.BeginInvoke((Action)InvalidateMeasure);

                SetHorizontalOffset(offset.X);
                SetVerticalOffset(offset.Y);
            }

            // update viewport
            if (availableSize != viewport)
            {
                viewport = availableSize;

                if (ScrollOwner != null)
                    ScrollOwner.InvalidateScrollInfo();

                SetHorizontalOffset(offset.X);
                SetVerticalOffset(offset.Y);
            }
        }

        private int ComputeLineCount(int totalItemCount)
        {
            return (totalItemCount + itemsPerLine - 1) / itemsPerLine;
        }

        public bool CanHorizontallyScroll
        {
            get;
            set;
        }

        public bool CanVerticallyScroll
        {
            get;
            set;
        }

        private Size extent;

        public double ExtentWidth
        {
            get { return extent.Width; }
        }

        public double ExtentHeight
        {
            get { return extent.Height; }
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - ItemSlotSize.Height);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + ItemSlotSize.Height);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ItemSlotSize.Width);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + ItemSlotSize.Width);
        }

        public void ScrollToIndexedItem(int index)
        {
            BringIndexIntoView(index);
        }

        protected override void BringIndexIntoView(int index)
        {
            base.BringIndexIntoView(index);

            int n = index / itemsPerLine;

            var space = ComputeItemSpacing(RenderSize);

            if (Orientation == Orientation.Vertical)
            {
                double newTop = n * (ItemSlotSize.Height + space);

                if (newTop < offset.Y)
                    SetVerticalOffset(newTop);
                else if (newTop + ItemSlotSize.Height + space > offset.Y + viewport.Height)
                    SetVerticalOffset(newTop + ItemSlotSize.Height + space - viewport.Height);
            }
            else
            {
                double newLeft = n * (ItemSlotSize.Width + space);

                if (newLeft < offset.X)
                    SetHorizontalOffset(newLeft);
                else if (newLeft + ItemSlotSize.Width + space > offset.X + viewport.Width)
                    SetHorizontalOffset(newLeft + ItemSlotSize.Width + space - viewport.Width);
            }

            InvalidateMeasure();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (visual == null)
                return new Rect();

            var parentItemsControl = ItemsControl.GetItemsOwner(this);
            if (parentItemsControl == null)
                return new Rect();

            IItemContainerGenerator generator = ItemContainerGenerator;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                if (Equals(InternalChildren[i], visual))
                {
                    int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                    int row = itemIndex / itemsPerLine;
                    int column = itemIndex % itemsPerLine;

                    return new Rect(new Point(column * ItemSlotSize.Width, row * ItemSlotSize.Height), ItemSlotSize);
                }
            }

            return new Rect();
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - ItemSlotSize.Height);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + ItemSlotSize.Height);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ItemSlotSize.Width);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + ItemSlotSize.Width);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - viewport.Height);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + viewport.Height);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - viewport.Width);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + viewport.Width);
        }

        public ScrollViewer ScrollOwner
        {
            get;
            set;
        }

        private Point offset;

        public void SetHorizontalOffset(double horizontalOffset)
        {
            if (horizontalOffset < 0.0 || viewport.Width >= extent.Width)
                horizontalOffset = 0.0;
            else
            {
                if (horizontalOffset + viewport.Width >= extent.Width)
                    horizontalOffset = extent.Width - viewport.Width;
            }

            offset.X = horizontalOffset;

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();

            InvalidateMeasure();
        }

        public void SetVerticalOffset(double verticalOffset)
        {
            if (verticalOffset < 0.0 || viewport.Height >= extent.Height)
                verticalOffset = 0.0;
            else
            {
                if (verticalOffset + viewport.Height >= extent.Height)
                    verticalOffset = extent.Height - viewport.Height;
            }

            offset.Y = verticalOffset;

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();

            InvalidateMeasure();
        }

        public double HorizontalOffset
        {
            get { return offset.X; }
        }

        public double VerticalOffset
        {
            get { return offset.Y; }
        }

        private Size viewport;

        public double ViewportHeight
        {
            get { return viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return viewport.Width; }
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                int index = args.Position.Index;
                if (args.Position.Offset > 0)
                {
                    index++;
                }
                if (index < InternalChildren.Count && args.ItemUICount > 0)
                {
                    RemoveInternalChildRange(index, args.ItemUICount);
                }
            }
            base.OnItemsChanged(sender, args);
        }
    }
}
