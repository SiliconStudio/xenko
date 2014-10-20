using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Assets.Texture
{
    /// <summary>
    /// Implementation of texture packer using MaxRects algorithm.
    /// More information: 
    /// http://www.drdobbs.com/database/the-maximal-rectangle-problem/184410529
    /// http://www.glbasic.com/forum/index.php?topic=7896.0
    /// </summary>
    public class TexturePacker
    {
        /// <summary>
        /// Packs several rectangles into one or more sheets.
        /// Note: Current algorithm only gives one sheet in a list.
        /// </summary>
        /// <param name="configuration">Packing configuration</param>
        /// <param name="rectangles">Rectangles input</param>
        /// <returns></returns>
        public static List<Sheet> PackRectangles(PackingConfiguration configuration, List<Rectangle> rectangles)
        {
            rectangles = rectangles.OrderByDescending(rectangle => rectangle.Width * rectangle.Height).ToList();

            var sheet = new Sheet { Width = configuration.PreferredWidth, Height = configuration.PreferredHeight, IsRotate = configuration.CanRotate };

            var rectangleSheet = new Rectangle(0, 0, configuration.PreferredWidth, configuration.PreferredHeight);

            foreach (var rectangle in rectangles)
            {
                var maxSpaceRect = FindMaxRectangle(ref rectangleSheet, sheet.Rectangles);

                // Transform rect relative to maxSpaceRect
                var transRect = rectangle;
                transRect.X = maxSpaceRect.X;
                transRect.Y = maxSpaceRect.Y;

                // Rotate Transform rect 90 degree (Swap Height and Width)
                var rotatedTransRect = new Rectangle(transRect.X, transRect.Y, transRect.Height, transRect.Width);

                // Check if a rectangle could be placed in the available space; if so, add it to a sheet.
                if (maxSpaceRect.Contains(transRect))
                    sheet.Rectangles.Add(new RotatableRectangle { BaseRectangle = transRect, IsRotated = false });
                else if (configuration.CanRotate && maxSpaceRect.Contains(rotatedTransRect))
                    sheet.Rectangles.Add(new RotatableRectangle { BaseRectangle = rotatedTransRect, IsRotated = true });
            }

            return new List<Sheet> { sheet };
        }

        /// <summary>
        /// Brute-force implementation of MaxRects algorithm.
        /// Running time: T( SheetWidth^2 * SheetHeight^2 * SheetWidth * SheetHeight * rectangles.count).
        /// Given SheetWidth == SheetHeight == n; Running time: O( n^6 ).
        /// </summary>
        /// <param name="sheetRectangle"></param>
        /// <param name="rectangles"></param>
        /// <returns></returns>
        public static Rectangle FindMaxRectangle(ref Rectangle sheetRectangle, List<RotatableRectangle> rectangles)
        {
            var bestTopLeft = new Point();
            var bestBottomRight = new Point();

            var topLeftPoint = new Point();
            var bottomRightPoint = new Point();

            // O( Width^2 * Height^2 )
            for (topLeftPoint.X = 0; topLeftPoint.X < sheetRectangle.Width; ++topLeftPoint.X)
                for (topLeftPoint.Y = 0; topLeftPoint.Y < sheetRectangle.Height; ++topLeftPoint.Y)
                    for (bottomRightPoint.X = topLeftPoint.X; bottomRightPoint.X < sheetRectangle.Width; ++bottomRightPoint.X)
                        for (bottomRightPoint.Y = topLeftPoint.Y; bottomRightPoint.Y < sheetRectangle.Height; ++bottomRightPoint.Y)
                        {
                            // O(Rect.Width * Rect.Height * rectangles.count)
                            if ((bottomRightPoint.X - topLeftPoint.X) * (bottomRightPoint.Y - topLeftPoint.Y) > (bestBottomRight.X - bestTopLeft.X) * (bestBottomRight.Y - bestTopLeft.Y)
                                && IsAllEmpty(ref topLeftPoint, ref bottomRightPoint, rectangles))
                            {
                                bestTopLeft = topLeftPoint;
                                bestBottomRight = bottomRightPoint;
                            }
                        }

            return new Rectangle(bestTopLeft.X, bestTopLeft.Y, bestBottomRight.X - bestTopLeft.X + 1, bestBottomRight.Y - bestTopLeft.Y + 1);
        }

        /// <summary>
        /// Running time: O( Rect.Width * Rect.Height * rectangles.count )
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="rectangles"></param>
        /// <returns></returns>
        public static bool IsAllEmpty(ref Point topLeft, ref Point bottomRight, List<RotatableRectangle> rectangles)
        {
            for(var x = topLeft.X ; x < bottomRight.X ; ++x)
                for(var y = topLeft.Y ; y < bottomRight.Y ; ++y)
                    foreach (var rectangle in rectangles)
                    {
                        if (!rectangle.IsRotated)
                        {
                            if (rectangle.BaseRectangle.Contains(x, y)) 
                                return false;
                        }
                        else
                        {
                            if (new Rectangle(rectangle.BaseRectangle.X, rectangle.BaseRectangle.Y, rectangle.BaseRectangle.Height, rectangle.BaseRectangle.Width).Contains(x, y))
                                return false;
                        }

                    }
            return true;
        }


        /// <summary>
        /// Sheet contains its properties {Width, Height}, and packed rectangles
        /// </summary>
        public class Sheet
        {
            public int Width;

            public int Height;

            public bool IsRotate;

            public List<RotatableRectangle> Rectangles { get {  return rectangles; } }

            private readonly List<RotatableRectangle> rectangles = new List<RotatableRectangle>();
        }

        public struct PackingConfiguration
        {
            public int PreferredWidth;

            public int PreferredHeight;

            public bool CanRotate;

            // todo:nut/ handle border configuration
            public int RectangleBorderSize;
        }

        public struct RotatableRectangle
        {
            public Rectangle BaseRectangle;

            public bool IsRotated;
        }
    }
}
