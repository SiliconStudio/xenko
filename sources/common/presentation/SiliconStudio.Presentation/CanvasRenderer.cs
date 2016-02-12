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
        public double BalancedLineDrawingThicknessLimit { get; set; }

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
            var ellipse = Create<Ellipse>();

            ellipse.Fill = GetBrush(fillColor);
            SetStroke(ellipse, strokeColor, thickness, lineJoin, dashArray, dashOffset);

            point.Offset(-size.Width / 2, -size.Height / 2);
            var rect = new Rect(point, size);
            ellipse.Height = rect.Height;
            ellipse.Width = rect.Width;
            Canvas.SetLeft(ellipse, rect.Left);
            Canvas.SetTop(ellipse, rect.Top);
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
        public void DrawLine(Point p1, Point p2, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            var line = Create<Line>();
            SetStroke(line, strokeColor, thickness, lineJoin, dashArray, dashOffset);
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
        public void DrawLineSegments(IList<Point> points, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count < 2)
                return;

            if (UseStreamGeometry)
            {
                DrawLineSegmentsByStreamGeometry(points, strokeColor, thickness, lineJoin, dashArray, dashOffset);
                return;
            }

            var pathGeometry = new PathGeometry();
            for (var i = 0; i < points.Count - 1; i += 2)
            {
                var figure = new PathFigure
                {
                    IsClosed = false,
                    StartPoint = points[i],
                };
                var segment = new LineSegment
                {
                    IsSmoothJoin = false,
                    IsStroked = true,
                    Point = points[i + 1],
                };
                figure.Segments.Add(segment);
                pathGeometry.Figures.Add(figure);
            }

            var path = Create<Path>();
            SetStroke(path, strokeColor, thickness, lineJoin, dashArray, dashOffset);
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
        public void DrawPolygon(ICollection<Point> points, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            var polygon = Create<Polygon>();

            polygon.Fill = GetBrush(fillColor);
            SetStroke(polygon, strokeColor, thickness, lineJoin, dashArray, dashOffset);

            polygon.Points = new PointCollection(points);
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
        public void DrawPolyline(IList<Point> points, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            if (thickness < BalancedLineDrawingThicknessLimit)
            {
                DrawPolylineBalanced(points, strokeColor, thickness, lineJoin, dashArray);
            }

            var polyline = Create<Polyline>();
            SetStroke(polyline, strokeColor, thickness, lineJoin, dashArray, dashOffset);
            polyline.Points = new PointCollection(points);
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
            var rectangle = Create<Rectangle>();

            rectangle.Fill = GetBrush(fillColor);
            SetStroke(rectangle, strokeColor, thickness, lineJoin, dashArray, dashOffset);

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
        public void DrawText(Point point, Color color, string text, FontFamily fontFamily, double fontSize, FontWeight fontWeight)
        {
            var textBlock = Create<TextBlock>();
            textBlock.Foreground = GetBrush(color);
            textBlock.FontFamily = fontFamily;
            textBlock.FontSize = fontSize;
            textBlock.FontWeight = fontWeight;
            textBlock.Text = text;
            textBlock.RenderTransform = new TranslateTransform(point.X, point.Y);
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

            var textBlock = new TextBlock
            {
                FontFamily = fontFamily,
                FontSize = fontSize,
                FontWeight = fontWeight,
                Text = text,
            };
            textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
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
        /// <returns></returns>
        private TElement Create<TElement>()
            where TElement : UIElement, new()
        {
            var element = new TElement();
            if (clip.HasValue && !clip.Value.IsEmpty)
            {
                element.Clip = new RectangleGeometry(clip.Value);
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
        /// <remarks>Using stream geometry seems to be slightly faster than using path geometry.</remarks>
        private void DrawLineSegmentsByStreamGeometry(IList<Point> points, Color strokeColor,
            double thickness, PenLineJoin lineJoin, ICollection<double> dashArray, double dashOffset)
        {
            var streamGeometry = new StreamGeometry();

            var streamGeometryContext = streamGeometry.Open();
            for (var i = 0; i < points.Count - 1; i += 2)
            {
                streamGeometryContext.BeginFigure(points[i], false, false);
                streamGeometryContext.LineTo(points[i + 1], true, false);
            }
            streamGeometryContext.Close();

            var path = Create<Path>();
            SetStroke(path, strokeColor, thickness, lineJoin, dashArray, dashOffset);
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
        /// <remarks>See <a href="https://oxyplot.codeplex.com/discussions/456679">discussion</a>.</remarks>
        private void DrawPolylineBalanced(IList<Point> points, Color strokeColor, double thickness, PenLineJoin lineJoin, ICollection<double> dashArray)
        {
            // balance the number of points per polyline and the number of polylines
            var numPointsPerPolyline = Math.Max(points.Count / MaxPolylinesPerLine, MinPointsPerPolyline);

            var polyline = Create<Polyline>();
            SetStroke(polyline, strokeColor, thickness, lineJoin, dashArray, 0);
            var pointCollection = new PointCollection(numPointsPerPolyline);

            var pointCount = points.Count;
            double lineLength = 0;
            var dashPatternLength = dashArray?.Sum() ?? 0;
            var last = new Point();
            for (var i = 0; i < pointCount; i++)
            {
                var current = points[i];
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
                        SetStroke(polyline, strokeColor, thickness, lineJoin, dashArray, dashOffset);
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

        private void SetStroke(Shape shape, Color color, double thickness, PenLineJoin lineJoin, ICollection<double> dashArray, double dashOffset)
        {
            shape.Stroke = GetBrush(color);
            shape.StrokeThickness = thickness;
            shape.StrokeLineJoin = lineJoin;
            if (dashArray != null)
            {
                shape.StrokeDashArray = new DoubleCollection(dashArray);
                shape.StrokeDashOffset = dashOffset;
            }
        }

    }
}
