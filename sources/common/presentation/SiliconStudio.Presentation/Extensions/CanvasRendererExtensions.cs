using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Extensions
{
    public static class CanvasRendererExtensions
    {
        /// <summary>
        /// Draws a circle in the canvas.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="point"></param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="fillColor">The color of the shape's interior.</param>
        /// <param name="strokeColor">The color of the shape's outline.</param>
        /// <param name="thickness">The wifdth of the shape's outline.</param>
        /// <param name="lineJoin">The type of join that is used at the vertices of the shape.</param>
        /// <param name="dashArray">The pattern of dashes and gaps that is used to outline the shape.</param>
        /// <param name="dashOffset">The distance within the dash pattern where a dash begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawCircle(this CanvasRenderer renderer, Point point, double radius, Color fillColor, Color strokeColor,
            double thickness = 1.0, PenLineJoin lineJoin = PenLineJoin.Miter, ICollection<double> dashArray = null, double dashOffset = 0)
        {
            renderer.DrawEllipse(point, new Size(radius, radius), fillColor, strokeColor, thickness, lineJoin, dashArray, dashOffset);
        }
    }
}
