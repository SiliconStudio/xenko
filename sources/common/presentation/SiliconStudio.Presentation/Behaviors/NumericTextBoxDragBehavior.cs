// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Controls;
using SiliconStudio.Presentation.Extensions;
using SiliconStudio.Presentation.Interop;

namespace SiliconStudio.Presentation.Behaviors
{
    public sealed class NumericTextBoxDragBehavior : MouseMoveCaptureBehaviorBase<NumericTextBox>
    {
        private enum DragState
        {
            None,
            Starting,
            Dragging,
        }

        private DragDirectionAdorner adorner;
        private Orientation dragOrientation;
        private DragState dragState;
        private Point mouseDownPosition;
        private double mouseMoveDelta;

        /// <inheritdoc />
        protected override void CancelOverride()
        {
            Mouse.OverrideCursor = null;
            dragState = DragState.None;

            var root = AssociatedObject.FindVisualRoot() as UIElement;
            if (root != null)
                root.IsKeyboardFocusWithinChanged -= RootParentIsKeyboardFocusWithinChanged;
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!AssociatedObject.IsContentHostPart(e.OriginalSource))
                return;

            if (!AssociatedObject.AllowMouseDrag || AssociatedObject.IsReadOnly || AssociatedObject.IsFocused)
                return;

            e.Handled = true;
            CaptureMouse();

            dragState = DragState.Starting;
            Mouse.OverrideCursor = Cursors.None;
            mouseDownPosition = e.GetPosition(AssociatedObject);

            if (adorner == null)
            {
                adorner = new DragDirectionAdorner(AssociatedObject, AssociatedObject.contentHost.ActualWidth);
                var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                adornerLayer?.Add(adorner);
            }
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(AssociatedObject);
            if (AssociatedObject.AllowMouseDrag && dragState == DragState.Starting && e.LeftButton == MouseButtonState.Pressed)
            {
                var dx = Math.Abs(position.X - mouseDownPosition.X);
                var dy = Math.Abs(position.Y - mouseDownPosition.Y);
                dragOrientation = dx >= dy ? Orientation.Horizontal : Orientation.Vertical;

                if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
                {
                    var root = AssociatedObject.FindVisualRoot() as UIElement;
                    if (root != null)
                        root.IsKeyboardFocusWithinChanged += RootParentIsKeyboardFocusWithinChanged;

                    mouseDownPosition = position;
                    mouseMoveDelta = 0;
                    dragState = DragState.Dragging;
                    
                    AssociatedObject.SelectAll();
                    adorner?.SetOrientation(dragOrientation);
                }
            }

            if (dragState == DragState.Dragging)
            {
                if (dragOrientation == Orientation.Horizontal)
                    mouseMoveDelta += position.X - mouseDownPosition.X;
                else
                    mouseMoveDelta += mouseDownPosition.Y - position.Y;

                var deltaUsed = Math.Floor(mouseMoveDelta / NumericTextBox.DragSpeed);
                mouseMoveDelta -= deltaUsed;
                var newValue = AssociatedObject.Value + deltaUsed * AssociatedObject.SmallChange;

                AssociatedObject.SetCurrentValue(NumericTextBox.ValueProperty, newValue);

                if (AssociatedObject.MouseValidationTrigger == MouseValidationTrigger.OnMouseMove)
                {
                    AssociatedObject.Validate();
                }
                NativeHelper.SetCursorPos(AssociatedObject.PointToScreen(mouseDownPosition));
            }
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (dragState == DragState.Starting)
            {
                AssociatedObject.Select(0, AssociatedObject.Text.Length);
                if (!AssociatedObject.IsFocused)
                {
                    Keyboard.Focus(AssociatedObject);
                }
            }
            else if (dragState == DragState.Dragging && AssociatedObject.AllowMouseDrag)
            {
                if (adorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    if (adornerLayer != null)
                    {
                        adornerLayer.Remove(adorner);
                        adorner = null;
                    }
                }
                AssociatedObject.Validate();
            }

            e.Handled = true;
            ReleaseMouseCapture();
            Mouse.OverrideCursor = null;
            dragState = DragState.None;
        }

        private void RootParentIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue)
                return;

            // Cancel dragging in progress
            if (dragState == DragState.Dragging)
            {
                if (adorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    if (adornerLayer != null)
                    {
                        adornerLayer.Remove(adorner);
                        adorner = null;
                    }
                }
                AssociatedObject.Cancel();
            }
            Cancel();
        }

        private class DragDirectionAdorner : Adorner
        {
            private readonly double contentWidth;
            private static readonly ImageSource CursorHorizontalImageSource;
            private static readonly ImageSource CursorVerticalImageSource;

            static DragDirectionAdorner()
            {
                var asmName = Assembly.GetExecutingAssembly().GetName().Name;
                CursorHorizontalImageSource = ImageExtensions.ImageSourceFromFile($"pack://application:,,,/{asmName};component/Resources/Images/cursor_west_east.png");
                CursorVerticalImageSource = ImageExtensions.ImageSourceFromFile($"pack://application:,,,/{asmName};component/Resources/Images/cursor_north_south.png");
            }

            private Orientation dragOrientation;
            private bool ready;

            internal DragDirectionAdorner([NotNull] UIElement adornedElement, double contentWidth)
                : base(adornedElement)
            {
                this.contentWidth = contentWidth;
            }

            internal void SetOrientation(Orientation orientation)
            {
                ready = true;
                dragOrientation = orientation;
                InvalidateVisual();
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (ready == false)
                    return;

                VisualEdgeMode = EdgeMode.Aliased;
                var source = dragOrientation == Orientation.Horizontal ? CursorHorizontalImageSource : CursorVerticalImageSource;
                var left = Math.Round(contentWidth - source.Width);
                var top = Math.Round((AdornedElement.RenderSize.Height - source.Height) * 0.5);
                drawingContext.DrawImage(source, new Rect(new Point(left, top), new Size(source.Width, source.Height)));
            }
        }
    }
}
