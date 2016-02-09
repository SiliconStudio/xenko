// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#region Copyright and license
// Some parts of this file were inspired by OxyPlot (https://github.com/oxyplot/oxyplot)
/*
The MIT license (MTI)
https://opensource.org/licenses/MIT

Copyright (c) 2014 OxyPlot contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SiliconStudio.Presentation.Controls
{
    [TemplatePart(Name = GridPartName, Type = typeof(Grid))]
    public sealed class CanvasView : Control
    {
        /// <summary>
        /// The name of the part for the <see cref="Canvas"/>.
        /// </summary>
        private const string GridPartName = "PART_Grid";

        /// <summary>
        /// Identifies the <see cref="CanvasBounds"/> dependency property key.
        /// </summary>
        public static readonly DependencyPropertyKey CanvasBoundsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CanvasBounds), typeof(Rect), typeof(CanvasView), new PropertyMetadata(new Rect()));
        /// <summary>
        /// Identifies the <see cref="CanvasBounds"/> dependency property.
        /// </summary>
        private static readonly DependencyProperty CanvasBoundsProperty = CanvasBoundsPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the <see cref="IsCanvasValid"/> dependency property key.
        /// </summary>
        public static readonly DependencyPropertyKey IsCanvasValidPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsCanvasValid), typeof(bool), typeof(CanvasView), new PropertyMetadata(true));
        /// <summary>
        /// Identifies the <see cref="IsCanvasValid"/> dependency property.
        /// </summary>
        private static readonly DependencyProperty IsCanvasValidProperty = IsCanvasValidPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the <see cref="Model"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(nameof(Model), typeof(ICanvasViewItem), typeof(CanvasView), new PropertyMetadata(null, OnModelPropertyChanged));

        /// <summary>
        /// The grid.
        /// </summary>
        private Grid grid;
        /// <summary>
        /// The renderer.
        /// </summary>
        private CanvasRenderer renderer;
        
        static CanvasView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasView), new FrameworkPropertyMetadata(typeof(CanvasView)));
        }

        public CanvasView()
        {
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        }

        public Rect CanvasBounds { get { return (Rect)GetValue(CanvasBoundsProperty); } private set { SetValue(CanvasBoundsPropertyKey, value); } }

        /// <summary>
        /// Returns True if the current rendering is valid. False otherwise.
        /// </summary>
        /// <remarks>When the value is False, it means that the canvas will be redrawn at the end of this frame.</remarks>
        public bool IsCanvasValid { get { return (bool)GetValue(IsCanvasValidProperty); } private set { SetValue(IsCanvasValidPropertyKey, value); } }

        public ICanvasViewItem Model { get { return (ICanvasViewItem)GetValue(ModelProperty); } set { SetValue(ModelProperty, value); } }

        private static void OnModelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var view = (CanvasView)sender;

            var model = e.OldValue as ICanvasViewItem;
            model?.Detach(view);

            model = e.NewValue as ICanvasViewItem;
            model?.Attach(view);

            view.InvalidateCanvas();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.grid = GetTemplateChild(GridPartName) as Grid;
            if (this.grid == null)
                throw new InvalidOperationException($"A part named '{GridPartName}' must be present in the ControlTemplate, and must be of type '{typeof(Grid).FullName}'.");

            var canvas = new Canvas();
            this.grid.Children.Add(canvas);
            canvas.UpdateLayout();
            this.renderer = new CanvasRenderer(canvas);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (this.ActualWidth > 0 && this.ActualHeight > 0)
            {
                if (!this.IsCanvasValid)
                {
                    UpdateVisuals();
                }
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    var realBounds = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
        //    realBounds.Union(CanvasBounds);
        //    return new Size(realBounds.Width, realBounds.Height);
        //}

        /// <summary>
        /// Invalidates the canvas. The <see cref="Model"/> will render it only once, after all non-idle operations are completed
        /// (<see cref="DispatcherPriority.Background"/> priority). Thus it is safe to call it every time the canvas should be redraw
        /// even when other operations are coming.
        /// </summary>
        public void InvalidateCanvas()
        {
            if (this.renderer == null || this.Model == null || !this.IsCanvasValid)
                return;

            this.IsCanvasValid = false;
            // Invalidate the arrange state for the element.
            // After the invalidation, the element will have its layout updated,
            // which will occur asynchronously unless subsequently forced by UpdateLayout.
            Dispatcher.InvokeAsync(() =>
            {
                InvalidateArrange();
                Dispatcher.InvokeAsync(() =>
                {
                    CanvasBounds = VisualTreeHelper.GetDescendantBounds(this.renderer.Canvas);
                    IsCanvasValid = true;
                    //InvalidateMeasure();
                    // We must wait after the canvas is rendered to get correct values
                }, DispatcherPriority.Loaded);
            }, DispatcherPriority.Background);
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Make sure InvalidateArrange is called when the canvas is invalidated
            this.IsCanvasValid = false;
            InvalidateCanvas();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0 && e.NewSize.Width > 0)
            {
                InvalidateCanvas();
            }
        }

        private void UpdateVisuals()
        {
            if (this.renderer == null)
                return;

            // Clear the canvas
            this.renderer.Clear();
            // Render the model
            this.Model?.Render(renderer, this.ActualHeight, this.ActualWidth);
        }
    }
}
