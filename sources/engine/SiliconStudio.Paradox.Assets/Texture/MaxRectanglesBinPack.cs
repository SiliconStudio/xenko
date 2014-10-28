using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Assets.Texture
{
    /// <summary>
    /// Implementation of texture packer using MaxRects algorithm
    /// </summary>
    public partial class MaxRectanglesBinPack
    {
        public enum FreeRectangleChoiceHeuristic
        {
            RectangleBestShortSideFit,
            RectangleBestLongSideFit,
            RectangleBestAreaFit,
            RectangleBottomLeftRule,
            RectangleContactPointRule
        }

        private bool useRotation;
        private int binWidth;
        private int binHeight;

        public List<RotatableRectangle> UsedRectangles { get { return usedRectangles; } }

        private readonly List<RotatableRectangle> usedRectangles = new List<RotatableRectangle>();
        private readonly List<Rectangle> freeRectangles = new List<Rectangle>();

        public MaxRectanglesBinPack()
        {  
        }

        public MaxRectanglesBinPack(int width, int height, bool useRotation)
        {
            Initialize(width, height, useRotation);
        }

        public void Initialize(int width, int height, bool useRotation)
        {
            
            binWidth = width;
            binHeight = height;

            this.useRotation = useRotation;

            usedRectangles.Clear();
            freeRectangles.Clear();
            
            freeRectangles.Add(new Rectangle(0, 0, binWidth, binHeight));
        }

        /// <summary>
        /// Packs input rectangles with MaxRects algorithm.
        /// Note that, rectangles is modified when any rectangle could be packed, it will be removed from the collection.
        /// </summary>
        /// <param name="rectangles">a list of rectangles to be packed</param>
        /// <param name="method">MaxRects heuristic method which default value is RectangleBestShortSideFit</param>
        public void PackRectangles(List<RotatableRectangle> rectangles, FreeRectangleChoiceHeuristic method = FreeRectangleChoiceHeuristic.RectangleBestShortSideFit)
        {
            var bestNode = new RotatableRectangle();

            while (rectangles.Count > 0)
            {
                var bestScore1 = int.MaxValue;
                var bestScore2 = int.MaxValue;

                var bestRectangleIndex = -1;

                for (var i = 0; i < rectangles.Count; ++i)
                {
                    int score1;
                    int score2;
                    var pickedNode = ChooseTargetPosition(rectangles[i], method, out score1, out score2);

                    if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
                    {
                        bestScore1 = score1;
                        bestScore2 = score2;
                        bestRectangleIndex = i;
                        bestNode = pickedNode;
                    }
                }

                if (bestRectangleIndex == -1) break;

                PlaceRectangle(bestNode);
                rectangles.RemoveAt(bestRectangleIndex);
            }
        }

        private void PlaceRectangle(RotatableRectangle node)
        {
            var numberRectanglesToProcess = freeRectangles.Count;
            for (var i = 0; i < numberRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(freeRectangles[i], node))
                {
                    freeRectangles.RemoveAt(i);
                    --i;
                    --numberRectanglesToProcess;
                }
            }

            PruneFreeList();

            usedRectangles.Add(node);
        }

        private void PruneFreeList()
        {
            // Go through each pair and remove any rectangle that is redundant.
            for (var i = 0; i < freeRectangles.Count; ++i)
                for (var j = i + 1; j < freeRectangles.Count; ++j)
                {
                    if (freeRectangles[j].Contains(freeRectangles[i]))
                    {
                        freeRectangles.RemoveAt(i);
                        --i;
                        break;
                    }
                    if (freeRectangles[i].Contains(freeRectangles[j]))
                    {
                        freeRectangles.RemoveAt(j);
                        --j;
                    }
                }
        }

        private bool SplitFreeNode(Rectangle freeNode, RotatableRectangle usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (usedNode.Value.X >= freeNode.X + freeNode.Width || usedNode.Value.X + usedNode.Value.Width <= freeNode.X ||
                usedNode.Value.Y >= freeNode.Y + freeNode.Height || usedNode.Value.Y + usedNode.Value.Height <= freeNode.Y)
                return false;

            if (usedNode.Value.X < freeNode.X + freeNode.Width && usedNode.Value.X + usedNode.Value.Width > freeNode.X)
            {
                // New node at the top side of the used node.
                if (usedNode.Value.Y > freeNode.Y && usedNode.Value.Y < freeNode.Y + freeNode.Height)
                {
                    var newNode = freeNode;
                    newNode.Height = usedNode.Value.Y - newNode.Y;
                    freeRectangles.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.Value.Y + usedNode.Value.Height < freeNode.Y + freeNode.Height)
                {
                    var newNode = freeNode;
                    newNode.Y = usedNode.Value.Y + usedNode.Value.Height;
                    newNode.Height = freeNode.Y + freeNode.Height - (usedNode.Value.Y + usedNode.Value.Height);
                    freeRectangles.Add(newNode);
                }
            }

            if (usedNode.Value.Y < freeNode.Y + freeNode.Height && usedNode.Value.Y + usedNode.Value.Height > freeNode.Y)
            {
                // New node at the left side of the used node.
                if (usedNode.Value.X > freeNode.X && usedNode.Value.X < freeNode.X + freeNode.Width)
                {
                    var newNode = freeNode;
                    newNode.Width = usedNode.Value.X - newNode.X;
                    freeRectangles.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.Value.X + usedNode.Value.Width < freeNode.X + freeNode.Width)
                {
                    var newNode = freeNode;
                    newNode.X = usedNode.Value.X + usedNode.Value.Width;
                    newNode.Width = freeNode.X + freeNode.Width - (usedNode.Value.X + usedNode.Value.Width);
                    freeRectangles.Add(newNode);
                }
            }

            return true;
        }

        private RotatableRectangle ChooseTargetPosition(RotatableRectangle rectangle, FreeRectangleChoiceHeuristic method, out int score1, out int score2)
        {
            score1 = int.MaxValue;
            score2 = int.MaxValue;

            RotatableRectangle bestNode;

            switch (method)
            {
                case FreeRectangleChoiceHeuristic.RectangleBestShortSideFit:
                    bestNode = FindPositionForNewNodeBestShortSideFit(rectangle, out score1, ref score2);
                    break;
                case FreeRectangleChoiceHeuristic.RectangleBottomLeftRule:
                    bestNode = FindPositionForNewNodeBottomLeft(rectangle, out score1, ref score2);
                    break;
                case FreeRectangleChoiceHeuristic.RectangleContactPointRule:
                    bestNode = FindPositionForNewNodeContactPoint(rectangle, out score1);
                    score1 *= -1;
                    break;
                case FreeRectangleChoiceHeuristic.RectangleBestLongSideFit:
                    bestNode = FindPositionForNewNodeBestLongSideFit(rectangle, ref score2, out score1);
                    break;
                case FreeRectangleChoiceHeuristic.RectangleBestAreaFit:
                    bestNode = FindPositionForNewNodeBestAreaFit(rectangle, out score1, ref score2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("method");
            }

            if (bestNode.Value.Height == 0)
            {
                score1 = int.MaxValue;
                score2 = int.MaxValue;
            }

            return bestNode;
        }

        private RotatableRectangle FindPositionForNewNodeBestShortSideFit(RotatableRectangle rectangle, out int bestShortSideFit, ref int bestLongSideFit)
        {
            var bestNode = rectangle;

            var width = rectangle.Value.Width;
            var height = rectangle.Value.Height;

            bestShortSideFit = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // non-flip
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    var leftoverHoriz = Math.Abs(freeRectangles[i].Width - width);
                    var leftoverVert = Math.Abs(freeRectangles[i].Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;

                        bestNode.Value.Width = width;
                        bestNode.Value.Height = height;

                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;

                        bestNode.IsRotated = false;
                    }
                }

                if (!useRotation) continue;

                if (freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    var flippedLeftoverHoriz = Math.Abs(freeRectangles[i].Width - height);
                    var flippedLeftoverVert = Math.Abs(freeRectangles[i].Height - width);
                    var flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                    var flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                    if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;

                        bestNode.Value.Width = height;
                        bestNode.Value.Height = width;

                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                        bestNode.IsRotated = true;
                    }
                }
            }
            return bestNode;
        }

        /// <summary>
        /// The heuristic rule used by this algorithm is to Orient and place each-
        /// rectangle to the position where the y-coordinate of the top side of the rectangle
        /// is the smallest and if there are several such valid positions, pick the
        /// one that has the smallest x-coordinate value
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="bestY"></param>
        /// <param name="bestX"></param>
        /// <returns></returns>
        private RotatableRectangle FindPositionForNewNodeBottomLeft(RotatableRectangle rectangle, out int bestY, ref int bestX)
        {
            var bestNode = rectangle;
            var width = rectangle.Value.Width;
            var height = rectangle.Value.Height;

            bestY = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    var topSideY = freeRectangles[i].Y + height;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].X < bestX))
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = width;
                        bestNode.Value.Height = height;
                        bestY = topSideY;
                        bestX = freeRectangles[i].X;
                        bestNode.IsRotated = false;
                    }
                }

                if (!useRotation) continue;

                if (freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    var topSideY = freeRectangles[i].Y + width;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].X < bestX))
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = height;
                        bestNode.Value.Height = width;
                        bestY = topSideY;
                        bestX = freeRectangles[i].X;
                        bestNode.IsRotated = true;
                    }
                }
            }

            return bestNode;
        }

        private RotatableRectangle FindPositionForNewNodeContactPoint(RotatableRectangle rectangle, out int bestContactScore)
        {
            var bestNode = rectangle;

            var width = rectangle.Value.Width;
            var height = rectangle.Value.Height;

            bestContactScore = -1;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].X, freeRectangles[i].Y, width, height);
                    if (score > bestContactScore)
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = width;
                        bestNode.Value.Height = height;
                        bestContactScore = score;
                        bestNode.IsRotated = false;
                    }
                }

                if (!useRotation) continue;

                // Flip
                if (freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].X, freeRectangles[i].Y, width, height);
                    if (score > bestContactScore)
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = height;
                        bestNode.Value.Height = width;
                        bestContactScore = score;
                        bestNode.IsRotated = true;
                    }
                }
            }

            return bestNode;
        }

        private int ContactPointScoreNode(int x, int y, int width, int height)
        {
            var score = 0;

            if (x == 0 || x + width == binWidth)
                score += height;
            if (y == 0 || y + height == binHeight)
                score += width;

            for (var i = 0; i < usedRectangles.Count; ++i)
            {
                if (usedRectangles[i].Value.X == x + width || usedRectangles[i].Value.X + usedRectangles[i].Value.Width == x)
                    score += CommonIntervalLength(usedRectangles[i].Value.Y, usedRectangles[i].Value.Y + usedRectangles[i].Value.Height, y, y + height);
                if (usedRectangles[i].Value.Y == y + height || usedRectangles[i].Value.Y + usedRectangles[i].Value.Height == y)
                    score += CommonIntervalLength(usedRectangles[i].Value.X, usedRectangles[i].Value.X + usedRectangles[i].Value.Width, x, x + width);
            }

            return score;
        }

        private int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
        {
            if (i1end < i2start || i2end < i1start)
                return 0;
            return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
        }

        private RotatableRectangle FindPositionForNewNodeBestLongSideFit(RotatableRectangle rectangle, ref int bestShortSideFit, out int bestLongSideFit)
        {
            var bestNode = rectangle;

            var width = rectangle.Value.Width;
            var height = rectangle.Value.Height;

            bestLongSideFit = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    var leftoverHoriz = Math.Abs(freeRectangles[i].Width - width);
                    var leftoverVert = Math.Abs(freeRectangles[i].Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = width;
                        bestNode.Value.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                        bestNode.IsRotated = false;
                    }
                }

                if (!useRotation) continue;

                // Flip
                if (freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    var leftoverHoriz = Math.Abs(freeRectangles[i].Width - height);
                    var leftoverVert = Math.Abs(freeRectangles[i].Height - width);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);
                
                	if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                	{
                		bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = height;
                        bestNode.Value.Height = width;
                		bestShortSideFit = shortSideFit;
                		bestLongSideFit = longSideFit;
                	    bestNode.IsRotated = true;
                	}
                }
            }

            return bestNode;
        }

        private RotatableRectangle FindPositionForNewNodeBestAreaFit(RotatableRectangle rectangle, out int bestAreaFit, ref int bestShortSideFit)
        {
            var bestNode = rectangle;

            var width = rectangle.Value.Width;
            var height = rectangle.Value.Height;

            bestAreaFit = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                var areaFit = freeRectangles[i].Width * freeRectangles[i].Height - width * height;

                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    var leftoverHoriz = Math.Abs(freeRectangles[i].Width - width);
                    var leftoverVert = Math.Abs(freeRectangles[i].Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = width;
                        bestNode.Value.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                        bestNode.IsRotated = false;
                    }
                }

                if (!useRotation) continue;

                // Flip
                if (freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                	var leftoverHoriz = Math.Abs(freeRectangles[i].Width - height);
                    var leftoverVert = Math.Abs(freeRectangles[i].Height - width);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                
                	if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                	{
                		bestNode.Value.X = freeRectangles[i].X;
                        bestNode.Value.Y = freeRectangles[i].Y;
                        bestNode.Value.Width = height;
                        bestNode.Value.Height = width;
                		bestShortSideFit = shortSideFit;
                		bestAreaFit = areaFit;
                	    bestNode.IsRotated = true;
                	}
                }
            }

            return bestNode;
        }
    }
}
