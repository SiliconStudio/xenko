// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SiliconStudio.Presentation.Extensions;

// source: http://www.codeproject.com/Articles/209560/ListBox-drag-selection
// license: The Code Project Open License (CPOL) 1.02

namespace SiliconStudio.Presentation.Behaviors
{
    public class ListBoxRectangleSelectionBehavior : DeferredBehaviorBase<ListBox>
    {
        private ListBox listBox;
        private FrameworkElement itemsPresenter;

        private SelectionAdorner selectionRect;
        private AutoScroller autoScroller;
        private ItemsControlSelector selector;

        private bool mouseCaptured;
        private Point start;
        private Point end;

        protected override void OnAttachedOverride()
        {
            listBox = AssociatedObject;

            // If we're enabling selection by a rectangle we can assume
            // this means we want to be able to select more than one item.
            if (listBox.SelectionMode == SelectionMode.Single)
                listBox.SelectionMode = SelectionMode.Extended;

            Register();
        }

        protected override void OnDetachingOverride()
        {
            Unregister();
        }

        private void Register()
        {
            itemsPresenter = listBox.FindVisualChildOfType<ItemsPresenter>();

            if (itemsPresenter == null)
                return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(itemsPresenter);
            if (adornerLayer == null)
                return;

            selectionRect = new SelectionAdorner(itemsPresenter);
            adornerLayer.Add(selectionRect);

            selector = new ItemsControlSelector(listBox);

            autoScroller = new AutoScroller(listBox);
            autoScroller.OffsetChanged += OnOffsetChanged;

            // The ListBox intercepts the regular MouseLeftButtonDown event
            // to do its selection processing, so we need to handle the
            // PreviewMouseLeftButtonDown. The scroll content won't receive
            // the message if we click on a blank area so use the ListBox.
            listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            listBox.MouseLeftButtonUp += OnMouseLeftButtonUp;
            listBox.MouseMove += OnMouseMove;
        }

        private void Unregister()
        {
            if (selectionRect != null && autoScroller != null)
            {
                StopSelection();
            }

            // Remove all the event handlers so this instance can be reclaimed by the GC.
            listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            listBox.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            listBox.MouseMove -= OnMouseMove;

            if (autoScroller != null)
                autoScroller.Unregister();
        }

        private Point positionAtMouseDown;
        private bool isDragging;
        private bool isSelectedAtMouseDown;

        private readonly Type[] knownPresenterTypes = new[]
        {
            typeof(ContentPresenter),
            typeof(GridViewRowPresenterBase)
        };

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            positionAtMouseDown = e.GetPosition(itemsPresenter);
            isDragging = false;
            mouseCaptured = false;

            var item = GetItemAt(listBox, e.GetPosition) as ListBoxItem;

            isSelectedAtMouseDown = false;

            if (item != null)
            {
                if (item.IsSelected)
                    isSelectedAtMouseDown = true;
                else
                {
                    var result = VisualTreeHelper.HitTest(item, e.GetPosition(item));
                    if (result != null && result.VisualHit != null)
                    {
                        var presenter = result.VisualHit.FindVisualParentOfType<FrameworkElement>();

                        if (presenter != null)
                        {
                            var presenterType = presenter.GetType();
                            if (Array.Exists(knownPresenterTypes, t => t.IsAssignableFrom(presenterType)))
                                isSelectedAtMouseDown = true;
                        }
                    }
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (isDragging == false)
            {
                var currentMousePosition = e.GetPosition(itemsPresenter);

                var dx = currentMousePosition.X - positionAtMouseDown.X;
                var dy = currentMousePosition.Y - positionAtMouseDown.Y;

                if (Math.Abs(dx) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(dy) > SystemParameters.MinimumVerticalDragDistance)
                {
                    isDragging = true;

                    if (isSelectedAtMouseDown == false)
                    {
                        if (positionAtMouseDown.X >= 0 && positionAtMouseDown.X < itemsPresenter.ActualWidth &&
                            positionAtMouseDown.Y >= 0 && positionAtMouseDown.Y < itemsPresenter.ActualHeight)
                        {
                            mouseCaptured = TryCaptureMouse(e);
                            if (mouseCaptured)
                            {
                                StartSelection(positionAtMouseDown);
                                e.Handled = true;
                            }
                        }
                    }
                }
            }

            if (mouseCaptured)
            {
                // Get the position relative to the content of the ScrollViewer.
                end = e.GetPosition(itemsPresenter);
                autoScroller.Update(end);
                UpdateSelection();
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging == false)
            {
                var item = GetItemAt(listBox, e.GetPosition) as ListBoxItem;
                if (item != null)
                {
                    var isControlPressed = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

                    if (isControlPressed)
                    {
                        // toggle current item selection
                        //item.IsSelected = !isSelectedAtMouseDown;

                        // seems to be already supported by [SelectionMode = SelectionMode.Extended]
                        // doing it again revert selection to bad state, but wasn't the case before 0_o
                    }
                    else
                    {
                        // select current item
                        listBox.SelectedItems.Clear();
                        item.IsSelected = true;
                    }
                }
                else
                {
                    listBox.SelectedItems.Clear();
                }
            }

            isDragging = false;

            if (mouseCaptured)
            {
                mouseCaptured = false;
                itemsPresenter.ReleaseMouseCapture();
                StopSelection();
            }
        }

        private void OnOffsetChanged(object sender, OffsetChangedEventArgs e)
        {
            selector.Scroll(e.HorizontalChange, e.VerticalChange);
            UpdateSelection();
        }

        private void StartSelection(Point location)
        {
            // We've stolen the MouseLeftButtonDown event from the ListBox
            // so we need to manually give it focus.
            listBox.Focus();

            start = location;
            end = location;

            // Do we need to start a new selection?
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0 &&
                (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                // Neither the shift key or control key is pressed, so
                // clear the selection.
                listBox.SelectedItems.Clear();
            }

            selector.Reset();
            UpdateSelection();

            selectionRect.IsEnabled = true;
            autoScroller.IsEnabled = true;
        }

        private void UpdateSelection()
        {
            // Offset the start point based on the scroll offset.
            var transStart = autoScroller.TranslatePoint(start);

            // Draw the selecion rectangle.
            // Rect can't have a negative width/height...
            var x = Math.Min(transStart.X, end.X);
            var y = Math.Min(transStart.Y, end.Y);
            var width = Math.Abs(end.X - transStart.X);
            var height = Math.Abs(end.Y - transStart.Y);

            var area = new Rect(x, y, width, height);
            selectionRect.SelectionArea = area;

            // Select the items.
            // Transform the points to be relative to the ListBox.
            var topLeft = itemsPresenter.TranslatePoint(area.TopLeft, listBox);
            var bottomRight = itemsPresenter.TranslatePoint(area.BottomRight, listBox);

            // And select the items.
            selector.UpdateSelection(new Rect(topLeft, bottomRight));
        }

        private void StopSelection()
        {
            // Hide the selection rectangle and stop the auto scrolling.
            selectionRect.IsEnabled = false;
            autoScroller.IsEnabled = false;
        }

        private FrameworkElement GetItemAt(ItemsControl itemsControl, Func<IInputElement, Point> getPosition)
        {
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                var item = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (item != null)
                {
                    var bounds = VisualTreeHelper.GetDescendantBounds(item);
                    if (bounds.Contains(getPosition(item)))
                        return item;
                }
            }

            return null;
        }

        private bool TryCaptureMouse(MouseEventArgs e)
        {
            var position = e.GetPosition(itemsPresenter);

            // Check if there is anything under the mouse.
            var element = itemsPresenter.InputHitTest(position) as UIElement;
            if (element != null)
            {
                // Simulate a mouse click by sending it the MouseButtonDown
                // event based on the data we received.
                var args = new MouseButtonEventArgs(
                    e.MouseDevice,
                    e.Timestamp,
                    MouseButton.Left,
                    e.StylusDevice) { RoutedEvent = Mouse.MouseDownEvent, Source = e.Source };

                element.RaiseEvent(args);

                // The ListBox will try to capture the mouse unless something
                // else captures it.
                if (!ReferenceEquals(Mouse.Captured, listBox))
                    return false; // Something else wanted the mouse, let it keep it.
            }

            // Either there's nothing under the mouse or the element doesn't want the mouse.
            return itemsPresenter.CaptureMouse();
        }

        // *** Helper types *****************************************************************************

        /// <summary>
        /// Automatically scrolls an ItemsControl when the mouse is dragged outside
        /// of the control.
        /// </summary>
        private sealed class AutoScroller
        {
            private readonly DispatcherTimer autoScroll = new DispatcherTimer();
            private readonly ItemsControl itemsControl;
            private readonly ScrollViewer scrollViewer;
            private readonly ScrollContentPresenter scrollContent;
            private bool isEnabled;
            private Point offset;
            private Point mouse;

            /// <summary>
            /// Initializes a new instance of the AutoScroller class.
            /// </summary>
            /// <param name="itemsControl">The ItemsControl that is scrolled.</param>
            /// <exception cref="ArgumentNullException">itemsControl is null.</exception>
            public AutoScroller(ItemsControl itemsControl)
            {
                if (itemsControl == null)
                {
                    throw new ArgumentNullException("itemsControl");
                }

                this.itemsControl = itemsControl;
                scrollViewer = itemsControl.FindVisualChildOfType<ScrollViewer>();
                scrollViewer.ScrollChanged += OnScrollChanged;
                scrollContent = scrollViewer.FindVisualChildOfType<ScrollContentPresenter>();

                autoScroll.Tick += delegate { PreformScroll(); };
                autoScroll.Interval = TimeSpan.FromMilliseconds(GetRepeatRate());
            }

            /// <summary>Occurs when the scroll offset has changed.</summary>
            public event EventHandler<OffsetChangedEventArgs> OffsetChanged;

            /// <summary>
            /// Gets or sets a value indicating whether the auto-scroller is enabled
            /// or not.
            /// </summary>
            public bool IsEnabled
            {
                private get
                {
                    return isEnabled;
                }
                set
                {
                    if (isEnabled != value)
                    {
                        isEnabled = value;

                        // Reset the auto-scroller and offset.
                        autoScroll.IsEnabled = false;
                        offset = new Point();
                    }
                }
            }

            /// <summary>
            /// Translates the specified point by the current scroll offset.
            /// </summary>
            /// <param name="point">The point to translate.</param>
            /// <returns>A new point offset by the current scroll amount.</returns>
            public Point TranslatePoint(Point point)
            {
                return new Point(point.X - offset.X, point.Y - offset.Y);
            }

            /// <summary>
            /// Removes all the event handlers registered on the control.
            /// </summary>
            public void Unregister()
            {
                scrollViewer.ScrollChanged -= OnScrollChanged;
            }

            /// <summary>
            /// Updates the location of the mouse and automatically scrolls if required.
            /// </summary>
            /// <param name="mousePosition">
            /// The location of the mouse, relative to the ScrollViewer's content.
            /// </param>
            public void Update(Point mousePosition)
            {
                mouse = mousePosition;

                // If scrolling isn't enabled then see if it needs to be.
                if (autoScroll.IsEnabled == false)
                {
                    PreformScroll();
                }
            }

            // Returns the default repeat rate in milliseconds.
            private static int GetRepeatRate()
            {
                // The RepeatButton uses the SystemParameters.KeyboardSpeed as the
                // default value for the Interval property. KeyboardSpeed returns
                // a value between 0 (400ms) and 31 (33ms).
                const double Ratio = (400.0 - 33.0) / 31.0;
                return 400 - (int)(SystemParameters.KeyboardSpeed * Ratio);
            }

            private double CalculateOffset(int startIndex, int endIndex)
            {
                var sum = 0.0;

                for (int i = startIndex; i != endIndex; i++)
                {
                    var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (container != null)
                    {
                        // Height = Actual height + margin
                        sum += container.ActualHeight;
                        sum += container.Margin.Top + container.Margin.Bottom;
                    }
                }

                return sum;
            }

            private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                // Do we need to update the offset?
                if (IsEnabled)
                {
                    var horizontal = e.HorizontalChange;
                    var vertical = e.VerticalChange;

                    // VerticalOffset means two seperate things based on the CanContentScroll
                    // property. If this property is true then the offset is the number of
                    // items to scroll; false then it's in Device Independant Pixels (DIPs).
                    if (scrollViewer.CanContentScroll)
                    {
                        // We need to either increase the offset or decrease it.
                        if (e.VerticalChange < 0)
                        {
                            var start = (int)e.VerticalOffset;
                            var end = (int)(e.VerticalOffset - e.VerticalChange);
                            vertical = -CalculateOffset(start, end);
                        }
                        else
                        {
                            var start = (int)(e.VerticalOffset - e.VerticalChange);
                            var end = (int)e.VerticalOffset;
                            vertical = CalculateOffset(start, end);
                        }
                    }

                    offset.X += horizontal;
                    offset.Y += vertical;

                    var callback = OffsetChanged;
                    if (callback != null)
                    {
                        callback(this, new OffsetChangedEventArgs(horizontal, vertical));
                    }
                }
            }

            private void PreformScroll()
            {
                var scrolled = false;

                var size = VisualTreeHelper.GetDescendantBounds(scrollViewer);

                if (mouse.X > size.Width /*scrollContent.ActualWidth*/)
                {
                    scrollViewer.LineRight();
                    scrolled = true;
                }
                else if (mouse.X < 0)
                {
                    scrollViewer.LineLeft();
                    scrolled = true;
                }

                if (mouse.Y > scrollContent.ActualHeight)
                {
                    scrollViewer.LineDown();
                    scrolled = true;
                }
                else if (mouse.Y < 0)
                {
                    scrollViewer.LineUp();
                    scrolled = true;
                }

                // It's important to disable scrolling if we're inside the bounds of
                // the control so that when the user does leave the bounds we can
                // re-enable scrolling and it will have the correct initial delay.
                autoScroll.IsEnabled = scrolled;
            }
        }

        /// <summary>Enables the selection of items by a specified rectangle.</summary>
        private sealed class ItemsControlSelector
        {
            private readonly ItemsControl itemsControl;
            private Rect previousArea;

            /// <summary>
            /// Initializes a new instance of the ItemsControlSelector class.
            /// </summary>
            /// <param name="itemsControl">
            /// The control that contains the items to select.
            /// </param>
            /// <exception cref="ArgumentNullException">itemsControl is null.</exception>
            public ItemsControlSelector(ItemsControl itemsControl)
            {
                if (itemsControl == null)
                    throw new ArgumentNullException("itemsControl");

                this.itemsControl = itemsControl;
            }

            /// <summary>
            /// Resets the cached information, allowing a new selection to begin.
            /// </summary>
            public void Reset()
            {
                previousArea = new Rect();
            }

            /// <summary>
            /// Scrolls the selection area by the specified amount.
            /// </summary>
            /// <param name="x">The horizontal scroll amount.</param>
            /// <param name="y">The vertical scroll amount.</param>
            public void Scroll(double x, double y)
            {
                previousArea.Offset(-x, -y);
            }

            /// <summary>
            /// Updates the controls selection based on the specified area.
            /// </summary>
            /// <param name="area">
            /// The selection area, relative to the control passed in the contructor.
            /// </param>
            public void UpdateSelection(Rect area)
            {
                // Check eack item to see if it intersects with the area.
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {
                    var item = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (item != null)
                    {
                        // Get the bounds in the parent's co-ordinates.
                        var topLeft = item.TranslatePoint(new Point(0, 0), itemsControl);
                        var itemBounds = new Rect(topLeft.X, topLeft.Y, item.ActualWidth, item.ActualHeight);

                        // Only change the selection if it intersects with the area
                        // (or intersected i.e. we changed the value last time).
                        if (itemBounds.IntersectsWith(area))
                        {
                            Selector.SetIsSelected(item, true);
                        }
                        else if (itemBounds.IntersectsWith(previousArea))
                        {
                            // We previously changed the selection to true but it no
                            // longer intersects with the area so clear the selection.
                            Selector.SetIsSelected(item, false);
                        }
                    }
                }

                previousArea = area;
            }
        }

        /// <summary>The event data for the AutoScroller.OffsetChanged event.</summary>
        private sealed class OffsetChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the OffsetChangedEventArgs class.
            /// </summary>
            /// <param name="horizontal">The change in horizontal scroll.</param>
            /// <param name="vertical">The change in vertical scroll.</param>
            internal OffsetChangedEventArgs(double horizontal, double vertical)
            {
                HorizontalChange = horizontal;
                VerticalChange = vertical;
            }

            /// <summary>Gets the change in horizontal scroll position.</summary>
            public double HorizontalChange { get; private set; }

            /// <summary>Gets the change in vertical scroll position.</summary>
            public double VerticalChange { get; private set; }
        }

        /// <summary>Draws a selection rectangle on an AdornerLayer.</summary>
        private sealed class SelectionAdorner : Adorner
        {
            private Rect selectionRect;
            private readonly Brush fill;
            private readonly Pen pen;

            /// <summary>
            /// Initializes a new instance of the SelectionAdorner class.
            /// </summary>
            /// <param name="parent">
            /// The UIElement which this instance will overlay.
            /// </param>
            /// <exception cref="ArgumentNullException">parent is null.</exception>
            public SelectionAdorner(UIElement parent)
                : base(parent)
            {
                // Make sure the mouse doesn't see us.
                IsHitTestVisible = false;

                fill = SystemColors.HighlightBrush.Clone();
                fill.Opacity = 0.4;
                fill.Freeze();

                pen = new Pen(SystemColors.HighlightBrush, 1.0);
                pen.Freeze();

                // We only draw a rectangle when we're enabled.
                IsEnabledChanged += delegate { InvalidateVisual(); };
            }

            /// <summary>Gets or sets the area of the selection rectangle.</summary>
            public Rect SelectionArea
            {
                get
                {
                    return selectionRect;
                }
                set
                {
                    selectionRect = value;
                    InvalidateVisual();
                }
            }

            /// <summary>
            /// Participates in rendering operations that are directed by the layout system.
            /// </summary>
            /// <param name="drawingContext">The drawing instructions.</param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (IsEnabled)
                {
                    // Make the lines snap to pixels (add half the pen width [0.5])
                    double[] x = { SelectionArea.Left + 0.5, SelectionArea.Right + 0.5 };
                    double[] y = { SelectionArea.Top + 0.5, SelectionArea.Bottom + 0.5 };
                    drawingContext.PushGuidelineSet(new GuidelineSet(x, y));

                    drawingContext.DrawRectangle(fill, pen, SelectionArea);
                }
            }
        }
    }
}
