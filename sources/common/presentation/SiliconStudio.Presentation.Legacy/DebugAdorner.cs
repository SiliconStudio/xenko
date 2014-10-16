// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Legacy
{
    public class DebugAdorner : Adorner
    {
        private readonly VisualContainerElement visuals = new VisualContainerElement();

        public static DebugAdorner GetDebugAdorner(Visual visual = null)
        {
            UIElement adornable = null;

            if (visual == null)
                visual = Application.Current.MainWindow;

            adornable = visual.FindAdornableOfType<UIElement>();

            if (adornable == null)
                throw new InvalidOperationException("Cannot retrieve adorner layer.");

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(adornable);
            if (layer == null)
                throw new InvalidOperationException("Cannot retrieve adorner layer.");

            var debugAdorner = new DebugAdorner(adornable);
            layer.Add(debugAdorner);

            return debugAdorner;
        }

        public DebugAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        public void FillRectangle(Brush brush, Rect rect)
        {
            visuals.AddVisual(CreateRectangle(brush, rect));
            InvalidateVisual();
        }

        public void Clear()
        {
            visuals.Clear();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawVisual(visuals);
        }

        private Visual CreateRectangle(Brush brush, Rect rect)
        {
            var visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(brush, null, rect);
            }

            return visual;
        }

        private Visual CreateLine(Pen pen, Point p1, Point p2)
        {
            var visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawLine(pen, p1, p2);
            }

            return visual;
        }

        private Visual CreateText(string text, float fontsize, double x, double y)
        {
            var visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), fontsize, Brushes.Black);
                context.DrawText(ft, new Point(x, y));
            }

            return visual;
        }
    }
}
