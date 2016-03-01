using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Behaviors
{
    public sealed class SelectionRectangleBehavior : MouseMoveCaptureBehaviorBase<ListBox>
    {
        public static readonly DependencyProperty CanvasProperty =
            DependencyProperty.Register(nameof(Canvas), typeof(Canvas), typeof(SelectionRectangleBehavior), new PropertyMetadata(OnCanvasChanged));

        public static readonly DependencyProperty AdditiveModifiersProperty =
            DependencyProperty.Register(nameof(AdditiveModifiers), typeof(ModifierKeys), typeof(SelectionRectangleBehavior), new PropertyMetadata(ModifierKeys.Shift));

        public static readonly DependencyProperty DefaultModifiersProperty =
            DependencyProperty.Register(nameof(DefaultModifiers), typeof(ModifierKeys), typeof(SelectionRectangleBehavior), new PropertyMetadata(ModifierKeys.None));

        public static readonly DependencyProperty SubtractiveModifiersProperty =
            DependencyProperty.Register(nameof(SubtractiveModifiers), typeof(ModifierKeys), typeof(SelectionRectangleBehavior), new PropertyMetadata(ModifierKeys.Control));

        public static readonly DependencyProperty RectangleStyleProperty
            = DependencyProperty.Register(nameof(RectangleStyle), typeof(Style), typeof(SelectionRectangleBehavior));

        private Point originPoint;
        private Panel itemsPanel;
        private Rectangle selectionRectangle;
        
        static SelectionRectangleBehavior()
        {
            AttachOnEveryLoadedEventProperty.OverrideMetadata(typeof(SelectionRectangleBehavior), new PropertyMetadata(true));
        }

        /// <summary>
        /// Resource Key for the default SelectionRectangleStyle.
        /// </summary>
        public static ResourceKey DefaultRectangleStyleKey { get; } = new ComponentResourceKey(typeof(SelectionRectangleBehavior), nameof(DefaultRectangleStyleKey));

        public Canvas Canvas { get { return (Canvas)GetValue(CanvasProperty); } set { SetValue(CanvasProperty, value); } }

        public ModifierKeys AdditiveModifiers { get { return (ModifierKeys)GetValue(AdditiveModifiersProperty); } set { SetValue(AdditiveModifiersProperty, value); } }

        public ModifierKeys DefaultModifiers { get { return (ModifierKeys)GetValue(DefaultModifiersProperty); } set { SetValue(DefaultModifiersProperty, value); } }

        public ModifierKeys SubtractiveModifiers { get { return (ModifierKeys)GetValue(SubtractiveModifiersProperty); } set { SetValue(SubtractiveModifiersProperty, value); } }

        public Style RectangleStyle { get { return (Style)GetValue(RectangleStyleProperty); } set { SetValue(RectangleStyleProperty, value); } }
        
        public bool IsDragging { get; private set; }

        private static void OnCanvasChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (SelectionRectangleBehavior)obj;
            behavior.OnCanvasChanged(e);
        }

        protected override void CancelOverride()
        {
            base.CancelOverride();
            IsDragging = false;
            Canvas.Visibility = Visibility.Collapsed;
        }

        protected override void OnAttachedOverride()
        {
            base.OnAttachedOverride();

            itemsPanel = GetItemsPanel(AssociatedObject);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!AreModifiersValid() || e.ChangedButton != MouseButton.Left)
                return;

            e.Handled = true;
            AssociatedObject.Focus();
            AssociatedObject.CaptureMouse();
            IsInProgress = true;
            
            originPoint = e.GetPosition(AssociatedObject);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!AreModifiersValid() || e.MouseDevice.LeftButton != MouseButtonState.Pressed)
            {
                Cancel();
                return;
            }

            var point = e.GetPosition(AssociatedObject);
            if (IsDragging)
            {
                UpdateDragSelectionRect(originPoint, point);
                e.Handled = true;
            }
            else
            {
                var curMouseDownPoint = e.GetPosition(AssociatedObject);
                var dragDelta = curMouseDownPoint - originPoint;
                if (Math.Abs(dragDelta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(dragDelta.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    IsDragging = true;
                    InitDragSelectionRect(originPoint, curMouseDownPoint);
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (!AreModifiersValid() || e.ChangedButton != MouseButton.Left)
                return;

            e.Handled = true;
            IsInProgress = false;
            AssociatedObject.ReleaseMouseCapture();

            if (IsDragging)
            {
                IsDragging = false;
                ApplyDragSelectionRect();
            }

        }

        private void CreateSelectionRectangle()
        {
            selectionRectangle = new Rectangle();
            if (RectangleStyle != null)
            {
                var binding = new Binding
                {
                    Path = new PropertyPath(nameof(RectangleStyle)),
                    Source = this,
                };
                selectionRectangle.SetBinding(FrameworkElement.StyleProperty, binding);
            }
            else
            {
                selectionRectangle.Style = selectionRectangle?.TryFindResource(DefaultRectangleStyleKey) as Style;
            }
        }

        private void OnCanvasChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldCanvas = e.OldValue as Canvas;
            if (oldCanvas != null && selectionRectangle != null)
            {
                oldCanvas.Children.Remove(selectionRectangle);
            }

            var newCanvas = e.NewValue as Canvas;
            if (newCanvas == null)
                return;
            newCanvas.Visibility = Visibility.Collapsed;

            if (selectionRectangle == null)
                CreateSelectionRectangle();
            if (selectionRectangle != null)
                newCanvas.Children.Add(selectionRectangle);
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);
            Canvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            //
            // Determine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle used for drag selection.
            //
            Canvas.SetLeft(selectionRectangle, x);
            Canvas.SetTop(selectionRectangle, y);
            selectionRectangle.Width = width;
            selectionRectangle.Height = height;
        }

        /// <summary>
        /// Select all nodes that are in the drag selection rectangle.
        /// </summary>
        private void ApplyDragSelectionRect()
        {
            Canvas.Visibility = Visibility.Collapsed;

            var x = Canvas.GetLeft(selectionRectangle);
            var y = Canvas.GetTop(selectionRectangle);
            var width = selectionRectangle.Width;
            var height = selectionRectangle.Height;
            var dragRect = new Rect(x, y, width, height);

            if (HasDefaultModifiers())
            {
                // Clear the current selection.
                AssociatedObject.SelectedItems.Clear(); 
            }
            
            // Find and select all the list box items.
            foreach (var item in AssociatedObject.Items)
            {
                var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                if (container == null)
                    continue;

                var bounds = GetBounds(container);
                if (!dragRect.IntersectsWith(bounds))
                    continue;

                var isItemSelected = AssociatedObject.SelectedItems.Contains(item);
                var isSubstractive = HasSubstractiveModifiers();
                if (isSubstractive && isItemSelected)
                {
                    AssociatedObject.SelectedItems.Remove(item);
                }
                else if (!isSubstractive && !isItemSelected)
                {
                    AssociatedObject.SelectedItems.Add(item);
                }
            }
        }

        private Rect GetBounds(Visual container)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(container);
            if (itemsPanel == null)
                return bounds;

            var transform = container.TransformToAncestor(itemsPanel);
            return transform.TransformBounds(bounds);
        }

        private static Panel GetItemsPanel(DependencyObject itemsControl)
        {
            var itemsPresenter = itemsControl.FindVisualChildOfType<ItemsPresenter>();
            if (itemsPresenter == null)
                return null;

            return VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AreModifiersValid()
        {
            return HasAdditiveModifiers() || HasDefaultModifiers() || HasSubstractiveModifiers();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAdditiveModifiers()
        {
            return AdditiveModifiers == ModifierKeys.None ? Keyboard.Modifiers == ModifierKeys.None : Keyboard.Modifiers.HasFlag(AdditiveModifiers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasDefaultModifiers()
        {
            return DefaultModifiers == ModifierKeys.None ? Keyboard.Modifiers == ModifierKeys.None : Keyboard.Modifiers.HasFlag(DefaultModifiers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasSubstractiveModifiers()
        {
            return SubtractiveModifiers == ModifierKeys.None ? Keyboard.Modifiers == ModifierKeys.None : Keyboard.Modifiers.HasFlag(SubtractiveModifiers);
        }
    }
}
