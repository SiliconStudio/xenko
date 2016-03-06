// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SiliconStudio.Presentation
{
    using Color = SiliconStudio.Core.Mathematics.Color;

    public interface IDrawingContext
    {
        /// <summary>
        /// Clears the drawing.
        /// </summary>
        void Clear();

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="fillColor">The color of the shape's interior.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        void DrawEllipse(Point point, Size size, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0);

        /// <summary>
        /// Draws a collection of ellipses, where all have the same visual appearance (stroke, fill, etc.).
        /// </summary>
        /// <remarks>
        /// This performs better than calling <see cref="CanvasRenderer.DrawEllipse"/> multiple times.
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
        void DrawEllipses(IList<Point> points, double radiusX, double radiusY, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0);

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
        void DrawLine(Point p1, Point p2, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false);

        /// <summary>
        /// Draws line segments defined by points (0,1) (2,3) (4,5) etc.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        void DrawLineSegments(IList<Point> points, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false);

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="fillColor"></param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        void DrawPolygon(IList<Point> points, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false);

        /// <summary>
        /// Draws a polyline.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        /// <param name="aliased"></param>
        void DrawPolyline(IList<Point> points, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0, bool aliased = false);

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="fillColor">The color of the shape's interior.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        void DrawRectangle(Rect rect, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0);

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="color">The color of the text.</param>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="hAlign">The horizontal alignment.</param>
        /// <param name="vAlign">The vertical alignment.</param>
        void DrawText(Point point, Color color, string text, FontFamily fontFamily, double fontSize, FontWeight fontWeight,
            HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top);

        /// <summary>
        /// Draws a collection of texts where all have the same visual appearance (color, font, alignment).
        /// </summary>
        /// <remarks>
        /// This performs better than calling <see cref="CanvasRenderer.DrawText"/> multiple times.
        /// </remarks>
        /// <param name="points"></param>
        /// <param name="color"></param>
        /// <param name="texts"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontWeight"></param>
        /// <param name="hAlign"></param>
        /// <param name="vAlign"></param>
        void DrawTexts(IList<Point> points, Color color, IList<string> texts, FontFamily fontFamily, double fontSize, FontWeight fontWeight,
            HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top);

        /// <summary>
        /// Measures the size of the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="measurementMethod"></param>
        /// <returns>
        /// The size of the text (in device independent units, 1/96 inch).
        /// </returns>
        Size MeasureText(string text, FontFamily fontFamily, double fontSize, FontWeight fontWeight,
            TextMeasurementMethod measurementMethod = TextMeasurementMethod.GlyphTypeface);

        /// <summary>
        /// Measures the size of the specified texts where all have the same visual appearance (color, font, alignment) and returns the maximum.
        /// </summary>
        /// <remarks>
        /// This performs better than calling <see cref="CanvasRenderer.MeasureText"/> multiple times.
        /// </remarks>
        /// <param name="texts">The texts.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="measurementMethod"></param>
        /// <returns>
        /// The maximum size of the texts (in device independent units, 1/96 inch).
        /// </returns>
        Size MeasureTexts(IList<string> texts, FontFamily fontFamily, double fontSize, FontWeight fontWeight,
            TextMeasurementMethod measurementMethod = TextMeasurementMethod.GlyphTypeface);

        /// <summary>
        /// Resets the clip rectangle.
        /// </summary>
        void ResetClip();

        /// <summary>
        /// Sets the clipping rectangle.
        /// </summary>
        /// <param name="clippingRect">The clipping rectangle.</param>
        void SetClip(Rect clippingRect);
    }
}
