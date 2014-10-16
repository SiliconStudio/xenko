// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// An enum describing when the related <see cref="SliderTextBox"/> should be validated, when the user uses the mouse to change its value.
    /// </summary>
    public enum MouseValidationTrigger
    {
        /// <summary>
        /// The validation occurs every time the mouse moves.
        /// </summary>
        OnMouseMove,
        /// <summary>
        /// The validation occurs when the mouse button is released.
        /// </summary>
        OnMouseUp,
    }

    /// <summary>
    /// A specialization of the <see cref="NumericTextBox"/> class that allows to uses the mouse to drag the value in the range defined by the
    /// <see cref="NumericTextBox.Minimum"/> and the <see cref="NumericTextBox.Maximum"/> properties, just like a <see cref="Slider"/>.
    /// </summary>
    public class SliderTextBox : NumericTextBox
    {
        private enum DragState
        {
            None,
            Starting,
            Dragging,
        }

        private DragState dragState;
        private Point mouseDownPosition;
        private DragDirectionAdorner adorner;
        private Orientation dragOrientation;

        static SliderTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderTextBox), new FrameworkPropertyMetadata(typeof(SliderTextBox)));
        }

        /// <summary>
        /// Identifies the <see cref="DisplayRangeIndicator"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayRangeIndicatorProperty = DependencyProperty.Register("DisplayRangeIndicator", typeof(bool), typeof(SliderTextBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="RangeIndicatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeIndicatorBrushProperty = DependencyProperty.Register("RangeIndicatorBrush", typeof(Brush), typeof(SliderTextBox), new FrameworkPropertyMetadata(Brushes.CornflowerBlue));

        /// <summary>
        /// Identifies the <see cref="IsMouseChangeEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMouseChangeEnabledProperty = DependencyProperty.Register("IsMouseChangeEnabled", typeof(bool), typeof(SliderTextBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="MouseValidationTrigger"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseValidationTriggerProperty = DependencyProperty.Register("MouseValidationTrigger", typeof(MouseValidationTrigger), typeof(SliderTextBox), new PropertyMetadata(MouseValidationTrigger.OnMouseUp));
        
        /// <summary>
        /// Gets or sets whether to display the range indicator at the bottom of the <see cref="SliderTextBox"/>.
        /// </summary>
        public bool DisplayRangeIndicator { get { return (bool)GetValue(DisplayRangeIndicatorProperty); } set { SetValue(DisplayRangeIndicatorProperty, value); } }

        /// <summary>
        /// Gets or sets the brush to use for the range indicator.
        /// </summary>
        public Brush RangeIndicatorBrush { get { return (Brush)GetValue(RangeIndicatorBrushProperty); } set { SetValue(RangeIndicatorBrushProperty, value); } }

        /// <summary>
        /// Gets or sets whether dragging the value of the <see cref="SliderTextBox"/> is enabled.
        /// </summary>
        public bool IsMouseChangeEnabled { get { return (bool)GetValue(IsMouseChangeEnabledProperty); } set { SetValue(IsMouseChangeEnabledProperty, value); } }

        /// <summary>
        /// Gets or sets when the <see cref="SliderTextBox"/> should be validated when the user uses the mouse to change its value.
        /// </summary>
        public MouseValidationTrigger MouseValidationTrigger { get { return (MouseValidationTrigger)GetValue(MouseValidationTriggerProperty); } set { SetValue(MouseValidationTriggerProperty, value); } }

        /// <inheritdoc/>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (IsMouseChangeEnabled && IsReadOnly == false)
            {
                dragState = DragState.Starting;

                if (adorner == null)
                {
                    adorner = new DragDirectionAdorner(this);
                    var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                    if (adornerLayer != null)
                        adornerLayer.Add(adorner);
                }

                mouseDownPosition = e.GetPosition(this);

                Mouse.OverrideCursor = Cursors.None;
            }
        }

        /// <inheritdoc/>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            Point position = e.GetPosition(this);

            if (IsMouseChangeEnabled && dragState == DragState.Starting && e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = Math.Abs(position.X - mouseDownPosition.X);
                double dy = Math.Abs(position.Y - mouseDownPosition.Y);
                dragOrientation = dx >= dy ? Orientation.Horizontal : Orientation.Vertical;

                if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
                {
                    e.MouseDevice.Capture(this);
                    dragState = DragState.Dragging;
                    SelectAll();
                    if (adorner != null)
                        adorner.SetOrientation(dragOrientation);
                }
            }

            if (dragState == DragState.Dragging)
            {
                double delta;

                if (dragOrientation == Orientation.Horizontal)
                    delta = position.X - mouseDownPosition.X;
                else
                    delta = mouseDownPosition.Y - position.Y;

                var newValue = Value + delta * SmallChange;

                SetCurrentValue(ValueProperty, newValue);

                if (MouseValidationTrigger == MouseValidationTrigger.OnMouseMove)
                {
                    Validate();
                }
                NativeHelper.SetCursorPos(PointToScreen(mouseDownPosition));
            }
        }

        /// <inheritdoc/>
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            if (ReferenceEquals(e.MouseDevice.Captured, this))
                e.MouseDevice.Capture(null);

            if (dragState == DragState.Starting)
            {
                Select(0, Text.Length);
            }
            else if (dragState == DragState.Dragging && IsMouseChangeEnabled)
            {
                if (adorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                    if (adornerLayer != null)
                    {
                        adornerLayer.Remove(adorner);
                        adorner = null;
                    }
                }
                Validate();
            }

            Mouse.OverrideCursor = null;
            dragState = DragState.None;
        }

       
        private class DragDirectionAdorner : Adorner
        {
            private static readonly ImageSource CursorHorizontalImageSource;
            private static readonly ImageSource CursorVerticalImageSource;

            static DragDirectionAdorner()
            {
                var asmName = Assembly.GetExecutingAssembly().GetName().Name;
                CursorHorizontalImageSource = ImageExtensions.ImageSourceFromFile(string.Format("pack://application:,,,/{0};component/Resources/Images/cursor_west_east.png", asmName));
                CursorVerticalImageSource = ImageExtensions.ImageSourceFromFile(string.Format("pack://application:,,,/{0};component/Resources/Images/cursor_north_south.png", asmName));
            }

            private Orientation dragOrientation;
            private bool ready;

            internal DragDirectionAdorner(UIElement adornedElement)
                : base(adornedElement)
            {
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

                if (dragOrientation == Orientation.Horizontal)
                {
                    var source = CursorHorizontalImageSource;
                    drawingContext.DrawImage(source, new Rect(new Point(AdornedElement.RenderSize.Width - source.Width, 0.0), new Size(source.Width, source.Height)));
                }
                else
                {
                    var source = CursorVerticalImageSource;
                    drawingContext.DrawImage(source, new Rect(new Point(AdornedElement.RenderSize.Width - source.Width, 0.0), new Size(source.Width, source.Height)));
                }
            }
        }
    }
}
