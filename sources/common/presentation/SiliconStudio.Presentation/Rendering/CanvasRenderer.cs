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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SiliconStudio.Presentation
{
    public class CanvasRenderer
    {
        private readonly Dictionary<Color, Brush> cachedBrushes = new Dictionary<Color, Brush>();
        private const int MaxPolylinesPerLine = 64;
        private const int MinPointsPerPolyline = 16;

        /// <summary>
        /// The clip rectangle.
        /// </summary>
        private Rect? clip;

        public CanvasRenderer(Canvas canvas)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            Canvas = canvas;
            UseStreamGeometry = true;
        }

        /// <summary>
        /// Gets or sets the thickness limit for "balanced" line drawing.
        /// </summary>
        public double BalancedLineDrawingThicknessLimit { get; set; } = 3.5;

        public Canvas Canvas { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to use stream geometry for lines and polygons rendering.
        /// </summary>
        /// <value><c>true</c> if stream geometry should be used; otherwise, <c>false</c> .</value>
        /// <remarks>Using stream geometry seems to be slightly faster than using path geometry.</remarks>
        public bool UseStreamGeometry { get; set; }

        /// <summary>
        /// Clears the canvas.
        /// </summary>
        public void Clear()
        {
            Canvas.Children.Clear();
        }

        /// <summary>
        /// Draws an ellipse in the canvas.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="fillColor">The color of the shape's interior.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        public void DrawEllipse(Point point, Size size, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            point.Offset(-size.Width / 2, -size.Height / 2);
            var rect = new Rect(point, size);

            var ellipse = Create<Ellipse>(rect.Left, rect.Top);

            ellipse.Fill = GetBrush(fillColor);
            SetStroke(ellipse, strokeColor, thickness, lineJoin, dashArray, dashOffset, false);

            ellipse.Height = rect.Height;
            ellipse.Width = rect.Width;
            Canvas.SetLeft(ellipse, rect.Left);
            Canvas.SetTop(ellipse, rect.Top);
        }

        /// <summary>
        /// Draws a collection of ellipses, where all have the same visual appearance (stroke, fill, etc.).
        /// </summary>
        /// <remarks>
        /// This performs better than calling <see cref="DrawEllipse"/> multiple times.
        /// </remarks>
        /// <param name="points"></param>
        /// <param name="radiusX">The horizontal radius of the ellipse.</param>
        /// <param name="radiusY">The vertical radius of the ellipse.</param>
        /// <param name="fillColor">The color of the shape's interior.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        public void DrawEllipses(IList<Point> points, double radiusX, double radiusY, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count == 0)
                return;

            var geometry = new GeometryGroup { FillRule = FillRule.Nonzero };
            foreach (var point in points)
            {
                geometry.Children.Add(new EllipseGeometry(point, radiusX, radiusY));
            }
            var path = Create<Path>();
            path.Fill = GetBrush(fillColor);
            SetStroke(path, strokeColor, thickness, lineJoin, dashArray, dashOffset, false);
            path.Data = geometry;
        }

        /// <summary>
        /// Draws a straight line between <paramref name="p1"/> and <paramref name="p2"/>.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        public void DrawLine(Point p1, Point p2, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false)
        {
            var line = Create<Line>();
            SetStroke(line, strokeColor, thickness, lineJoin, dashArray, dashOffset, aliased);
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;
        }

        /// <summary>
        /// Draws line segments defined by points (0,1) (2,3) (4,5) etc in the canvas.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        public void DrawLineSegments(IList<Point> points, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count < 2)
                return;

            if (UseStreamGeometry)
            {
                DrawLineSegmentsByStreamGeometry(points, strokeColor, thickness, lineJoin, dashArray, dashOffset, aliased);
                return;
            }

            var pathGeometry = new PathGeometry();
            for (var i = 0; i < points.Count - 1; i += 2)
            {
                var figure = new PathFigure
                {
                    IsClosed = false,
                    StartPoint = aliased ? ToPixelAlignedPoint(points[i]) : points[i],
                };
                var segment = new LineSegment
                {
                    IsSmoothJoin = false,
                    IsStroked = true,
                    Point = aliased ? ToPixelAlignedPoint(points[i + 1]) : points[i + 1],
                };
                figure.Segments.Add(segment);
                pathGeometry.Figures.Add(figure);
            }

            var path = Create<Path>();
            SetStroke(path, strokeColor, thickness, lineJoin, dashArray, dashOffset, aliased);
            path.Data = pathGeometry;
        }

        /// <summary>
        /// Draws a polygon in the canvas.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="fillColor"></param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        public void DrawPolygon(IList<Point> points, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false)
        {
            var polygon = Create<Polygon>();

            polygon.Fill = GetBrush(fillColor);
            SetStroke(polygon, strokeColor, thickness, lineJoin, dashArray, dashOffset, false);

            polygon.Points = ToPointCollection(points, aliased);
        }

        /// <summary>
        /// Draws a polyline in the canvas.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        public void DrawPolyline(IList<Point> points, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false)
        {
            if (thickness < BalancedLineDrawingThicknessLimit)
            {
                DrawPolylineBalanced(points, strokeColor, thickness, lineJoin, dashArray, aliased);
            }

            var polyline = Create<Polyline>();
            SetStroke(polyline, strokeColor, thickness, lineJoin, dashArray, dashOffset, aliased);
            polyline.Points = ToPointCollection(points, aliased);
        }

        /// <summary>
        /// Draws a rectangle in the canvas.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="fillColor">The color of the shape's interior.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        public void DrawRectangle(Rect rect, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            var rectangle = Create<Rectangle>(rect.Left, rect.Top);

            rectangle.Fill = GetBrush(fillColor);
            SetStroke(rectangle, strokeColor, thickness, lineJoin, dashArray, dashOffset, false);

            rectangle.Height = rect.Height;
            rectangle.Width = rect.Width;
            Canvas.SetLeft(rectangle, rect.Left);
            Canvas.SetTop(rectangle, rect.Top);
        }

        /// <summary>
        /// Draws text in the canvas.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="color">The color of the text.</param>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="hAlign">The horizontal alignment.</param>
        /// <param name="vAlign">The vertical alignment.</param>
        public void DrawText(Point point, Color color, string text, FontFamily fontFamily, double fontSize, FontWeight fontWeight,
            HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top)
        {
            var textBlock = Create<TextBlock>();
            textBlock.Foreground = GetBrush(color);
            textBlock.FontFamily = fontFamily;
            textBlock.FontSize = fontSize;
            textBlock.FontWeight = fontWeight;
            textBlock.Text = text;

            var dx = 0.0;
            var dy = 0.0;

            if (hAlign != HorizontalAlignment.Left || vAlign != VerticalAlignment.Top)
            {
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var size = textBlock.DesiredSize;
                if (hAlign == HorizontalAlignment.Center)
                    dx = -size.Width / 2;

                if (hAlign == HorizontalAlignment.Right)
                    dx = -size.Width;

                if (vAlign == VerticalAlignment.Center)
                    dy = -size.Height / 2;

                if (vAlign == VerticalAlignment.Bottom)
                    dy = -size.Height;
            }

            textBlock.RenderTransform = new TranslateTransform(point.X + dx, point.Y + dy);
            textBlock.SetValue(RenderOptions.ClearTypeHintProperty, ClearTypeHint.Enabled);
        }

        /// <summary>
        /// Draws a collection of texts where all have the same visual appearance (color, font, alignment).
        /// </summary>
        /// <remarks>
        /// This performs better than calling <see cref="DrawText"/> multiple times.
        /// </remarks>
        /// <param name="points"></param>
        /// <param name="color"></param>
        /// <param name="texts"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontWeight"></param>
        /// <param name="hAlign"></param>
        /// <param name="vAlign"></param>
        public void DrawTexts(IList<Point> points, Color color, IList<string> texts, FontFamily fontFamily, double fontSize, FontWeight fontWeight,
            HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (texts == null) throw new ArgumentNullException(nameof(texts));

            if (points.Count != texts.Count) throw new ArgumentException($"{nameof(points)} and {nameof(texts)} must have the same number of elements.");

            var brush = GetBrush(color);
            var typeFace = new Typeface(fontFamily, FontStyles.Normal, fontWeight, FontStretches.Normal);

            var visual = new DrawingVisual();
            var context = visual.RenderOpen();
            for (var i = 0; i < points.Count; ++i)
            {
                var text = texts[i];
                var point = points[i];
                var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeFace, fontSize, brush);
                var dx = 0.0;
                var dy = 0.0;
                if (hAlign != HorizontalAlignment.Left || vAlign != VerticalAlignment.Top)
                {
                    var size = new Size(formatted.Width, formatted.Height);
                    if (hAlign == HorizontalAlignment.Center)
                        dx = -size.Width / 2;

                    if (hAlign == HorizontalAlignment.Right)
                        dx = -size.Width;

                    if (vAlign == VerticalAlignment.Center)
                        dy = -size.Height / 2;

                    if (vAlign == VerticalAlignment.Bottom)
                        dy = -size.Height;
                }
                point.Offset(dx, dy);
                context.DrawText(formatted, point);
            }
            context.Close();

            var host = Create<VisualHost>();
            host.AddChild(visual);
        }

        /// <summary>
        /// Measures the size of the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <returns>
        /// The size of the text (in device independent units, 1/96 inch).
        /// </returns>
        public Size MeasureText(string text, FontFamily fontFamily, double fontSize, FontWeight fontWeight)
        {
            if (string.IsNullOrEmpty(text))
                return Size.Empty;

            // FIXME (performance): find another way to mesure without creating a control
            var textBlock = new TextBlock
            {
                FontFamily = fontFamily,
                FontSize = fontSize,
                FontWeight = fontWeight,
                Text = text,
            };
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return new Size(textBlock.DesiredSize.Width, textBlock.DesiredSize.Height);
        }

        /// <summary>
        /// Resets the clip rectangle.
        /// </summary>
        public void ResetClip()
        {
            clip = null;
        }

        /// <summary>
        /// Sets the clipping rectangle.
        /// </summary>
        /// <param name="clippingRect">The clipping rectangle.</param>
        public void SetClip(Rect clippingRect)
        {
            clip = clippingRect;
        }

        /// <summary>
        /// Creates an element and adds it to the canvas.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="clipOffsetX"></param>
        /// <param name="clipOffsetY"></param>
        /// <returns></returns>
        private TElement Create<TElement>(double clipOffsetX = 0, double clipOffsetY = 0)
            where TElement : UIElement, new()
        {
            var element = new TElement();
            if (clip.HasValue && !clip.Value.IsEmpty)
            {
                element.Clip = new RectangleGeometry(
                    new Rect(
                        clip.Value.X - clipOffsetX,
                        clip.Value.Y - clipOffsetY,
                        clip.Value.Width,
                        clip.Value.Height));
            }
            Canvas.Children.Add(element);
            return element;
        }

        /// <summary>
        /// Draws the line segments by stream geometry.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="strokeColor">The stroke color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="lineJoin">The line join.</param>
        /// <param name="dashArray">The dash array. Use <c>null</c> to get a solid line.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        /// <remarks>Using stream geometry seems to be slightly faster than using path geometry.</remarks>
        private void DrawLineSegmentsByStreamGeometry(IList<Point> points, Color strokeColor,
            double thickness, PenLineJoin lineJoin, ICollection<double> dashArray, double dashOffset, bool aliased)
        {
            var streamGeometry = new StreamGeometry();

            var streamGeometryContext = streamGeometry.Open();
            for (var i = 0; i < points.Count - 1; i += 2)
            {
                streamGeometryContext.BeginFigure(aliased ? ToPixelAlignedPoint(points[i]) : points[i], false, false);
                streamGeometryContext.LineTo(aliased ? ToPixelAlignedPoint(points[i + 1]) : points[i + 1], true, false);
            }
            streamGeometryContext.Close();

            var path = Create<Path>();
            SetStroke(path, strokeColor, thickness, lineJoin, dashArray, dashOffset, aliased);
            path.Data = streamGeometry;
        }

        /// <summary>
        /// Draws the line using the MaxPolylinesPerLine and MinPointsPerPolyline properties.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="strokeColor">The stroke color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="lineJoin">The line join.</param>
        /// <param name="dashArray">The dash array. Use <c>null</c> to get a solid line.</param>
        /// <param name="aliased"></param>
        /// <remarks>See <a href="https://oxyplot.codeplex.com/discussions/456679">discussion</a>.</remarks>
        private void DrawPolylineBalanced(IList<Point> points, Color strokeColor, double thickness, PenLineJoin lineJoin, ICollection<double> dashArray, bool aliased)
        {
            // balance the number of points per polyline and the number of polylines
            var numPointsPerPolyline = Math.Max(points.Count / MaxPolylinesPerLine, MinPointsPerPolyline);

            var polyline = Create<Polyline>();
            SetStroke(polyline, strokeColor, thickness, lineJoin, dashArray, 0, aliased);
            var pointCollection = new PointCollection(numPointsPerPolyline);

            var pointCount = points.Count;
            double lineLength = 0;
            var dashPatternLength = dashArray?.Sum() ?? 0;
            var last = new Point();
            for (var i = 0; i < pointCount; i++)
            {
                var current = aliased ? ToPixelAlignedPoint(points[i]) : points[i];
                pointCollection.Add(current);

                // get line length
                if (dashArray != null)
                {
                    if (i > 0)
                    {
                        var delta = current - last;
                        var dist = Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
                        lineLength += dist;
                    }

                    last = current;
                }

                // use multiple polylines with limited number of points to improve WPF performance
                if (pointCollection.Count >= numPointsPerPolyline)
                {
                    polyline.Points = pointCollection;

                    if (i < pointCount - 1)
                    {
                        // start a new polyline at last point so there is no gap (it is not necessary to use the % operator)
                        var dashOffset = dashPatternLength > 0 ? lineLength / thickness : 0;
                        polyline = Create<Polyline>();
                        SetStroke(polyline, strokeColor, thickness, lineJoin, dashArray, dashOffset, aliased);
                        pointCollection = new PointCollection(numPointsPerPolyline) { pointCollection.Last() };
                    }
                }
            }

            if (pointCollection.Count > 1 || pointCount == 1)
            {
                polyline.Points = pointCollection;
            }
        }

        /// <summary>
        /// Gets a brush for the given <paramref name="color"/>.
        /// </summary>
        /// <remarks>Brushes are cached and frozen to improve performance.</remarks>
        /// <seealso cref="Freezable.Freeze"/>
        /// <param name="color"></param>
        /// <returns></returns>
        private Brush GetBrush(Color color)
        {
            if (color.A == 0)
            {
                // If color is fully transparent, no need for a brush
                return null;
            }

            Brush brush;
            if (!cachedBrushes.TryGetValue(color, out brush))
            {
                brush = new SolidColorBrush(color);
                if (brush.CanFreeze)
                    brush.Freeze(); // Should improve rendering performance
                cachedBrushes.Add(color, brush);
            }

            return brush;
        }

        private void SetStroke(Shape shape, Color color, double thickness, PenLineJoin lineJoin, ICollection<double> dashArray, double dashOffset, bool aliased)
        {
            shape.Stroke = GetBrush(color);
            shape.StrokeThickness = thickness;
            shape.StrokeLineJoin = lineJoin;
            if (dashArray != null)
            {
                shape.StrokeDashArray = new DoubleCollection(dashArray);
                shape.StrokeDashOffset = dashOffset;
            }

            if (aliased)
            {
                shape.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                shape.SnapsToDevicePixels = true;
            }
        }

        /// <summary>
        /// Converts a <see cref="Point" /> to a pixel aligned<see cref="Point" />.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>A pixel aligned <see cref="Point" />.</returns>
        private static Point ToPixelAlignedPoint(Point point)
        {
            // adding 0.5 to get pixel boundary alignment, seems to work
            // http://weblogs.asp.net/mschwarz/archive/2008/01/04/silverlight-rectangles-paths-and-line-comparison.aspx
            return new Point(0.5 + (int)point.X, 0.5 + (int)point.Y);
        }

        /// <summary>
        /// Creates a point collection from the specified points.
        /// </summary>
        /// <param name="points">The points to convert.</param>
        /// <param name="aliased">Convert to pixel aligned points if set to <c>true</c>.</param>
        /// <returns>The point collection.</returns>
        private static PointCollection ToPointCollection(IEnumerable<Point> points, bool aliased)
        {
            return new PointCollection(aliased ? points.Select(ToPixelAlignedPoint) : points);
        }
    }
}
