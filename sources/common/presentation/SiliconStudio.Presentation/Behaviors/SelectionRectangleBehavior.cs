using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Behaviors
{
    public class SelectionRectangleBehavior : MouseMoveCaptureBehaviorBase<ListBox>
    {
        public static readonly DependencyProperty CanvasProperty =
            DependencyProperty.Register(nameof(Canvas), typeof(Canvas), typeof(SelectionRectangleBehavior), new PropertyMetadata(OnCanvasChanged));

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(SelectionRectangleBehavior), new PropertyMetadata(true));

        public static readonly DependencyProperty SelectionRectangleStyleProperty;

        private Point originPoint;
        private Panel itemsPanel;
        private Rectangle selectionRectangle;
        
        static SelectionRectangleBehavior()
        {
            SelectionRectangleDefaultStyle = new Style(typeof(Rectangle));
            SelectionRectangleDefaultStyle.Setters.Add(new Setter(Shape.FillProperty, new SolidColorBrush(Colors.LightBlue)));
            SelectionRectangleDefaultStyle.Setters.Add(new Setter(UIElement.OpacityProperty, 0.5));
            SelectionRectangleDefaultStyle.Setters.Add(new Setter(Shape.StrokeProperty, new SolidColorBrush(Colors.Blue)));
            SelectionRectangleDefaultStyle.Setters.Add(new Setter(Shape.StrokeLineJoinProperty, PenLineJoin.Round));
            SelectionRectangleDefaultStyle.Setters.Add(new Setter(Shape.StrokeThicknessProperty, 1.0));

            SelectionRectangleStyleProperty =
                DependencyProperty.Register(nameof(SelectionRectangleStyle), typeof(Style), typeof(SelectionRectangleBehavior), new PropertyMetadata(SelectionRectangleDefaultStyle));
        }

        public static Style SelectionRectangleDefaultStyle { get; }

        public Canvas Canvas { get { return (Canvas)GetValue(CanvasProperty); } set { SetValue(CanvasProperty, value); } }

        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value); } }

        public Style SelectionRectangleStyle { get { return (Style)GetValue(SelectionRectangleStyleProperty); } set { SetValue(SelectionRectangleStyleProperty, value); } }

        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        public double DragThreshold { get; set; } = 5;

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
            if (!IsEnabled || e.ChangedButton != MouseButton.Left)
                return;

            e.Handled = true;
            AssociatedObject.Focus();
            AssociatedObject.CaptureMouse();
            IsInProgress = true;
            
            originPoint = e.GetPosition(AssociatedObject);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!IsEnabled || e.MouseDevice.LeftButton != MouseButtonState.Pressed)
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
                var dragDistance = Math.Abs(dragDelta.LengthSquared);
                if (dragDistance > DragThreshold*DragThreshold)
                {
                    IsDragging = true;
                    // clear selection immediately when starting drag selection.
                    AssociatedObject.SelectedItems.Clear();
                    InitDragSelectionRect(originPoint, curMouseDownPoint);
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (!IsEnabled || e.ChangedButton != MouseButton.Left)
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
            var binding = new Binding
            {
                Path = new PropertyPath(nameof(SelectionRectangleStyle)),
                Source = this,
            };
            selectionRectangle = new Rectangle();
            selectionRectangle.SetBinding(FrameworkElement.StyleProperty, binding);
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
            
            // Clear the current selection.
            AssociatedObject.SelectedItems.Clear();
            
            // Find and select all the list box items.
            foreach (var item in AssociatedObject.Items)
            {
                var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                if (container == null)
                    continue;

                var bounds = GetBounds(container);
                if (dragRect.IntersectsWith(bounds))
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
    }
}
