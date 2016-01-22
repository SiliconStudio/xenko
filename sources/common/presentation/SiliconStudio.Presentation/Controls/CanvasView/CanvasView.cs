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
using System.Windows.Threading;

namespace SiliconStudio.Presentation.Controls
{
    [TemplatePart(Name = CanvasPartName, Type = typeof(Canvas))]
    public class CanvasView : Control
    {
        /// <summary>
        /// The name of the part for the <see cref="Canvas"/>.
        /// </summary>
        private const string CanvasPartName = "PART_Canvas";
        
        /// <summary>
        /// Identifies the <see cref="Model"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(nameof(Model), typeof(ICanvasViewItem), typeof(CanvasView), new PropertyMetadata(null, OnModelPropertyChanged));

        /// <summary>
        /// The renderer.
        /// </summary>
        private CanvasRenderer renderer;

        static CanvasView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasView), new FrameworkPropertyMetadata(typeof(CanvasView)));
        }

        public ICanvasViewItem Model
        {
            get { return (ICanvasViewItem)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

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

            var canvas = GetTemplateChild(CanvasPartName) as Canvas;
            if (canvas == null)
                throw new InvalidOperationException($"A part named '{CanvasPartName}' must be present in the ControlTemplate, and must be of type '{typeof(Canvas).FullName}'.");

            canvas.UpdateLayout();
            this.renderer = new CanvasRenderer(canvas);

            InvalidateCanvas();
        }

        public void InvalidateCanvas()
        {
            if (renderer == null || Model == null)
                return;

            BeginInvoke(() =>
            {
                renderer.Clear();
                Model.Render(renderer);
            });
        }

        private void BeginInvoke(Action action)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(action, DispatcherPriority.Background);
            }
            else
            {
                action();
            }
        }
    }
}
