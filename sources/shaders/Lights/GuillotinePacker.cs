// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Processors
{
    /// <summary>
    /// Implementation of a "Guillotine" packer.
    /// More information at http://clb.demon.fi/files/RectangleBinPack.pdf.
    /// </summary>
    internal class GuillotinePacker
    {
        private readonly List<Rectangle> freeRectangles = new List<Rectangle>();
        private readonly List<Rectangle> tempFreeRectangles = new List<Rectangle>();

        public int Width { get; private set; }
        public int Height { get; private set; }

        public void Clear(int width, int height)
        {
            freeRectangles.Clear();
            freeRectangles.Add(new Rectangle { X = 0, Y = 0, Width = width, Height = height });

            Width = width;
            Height = height;
        }

        public void Clear()
        {
            Clear(Width, Height);
        }

        public void Free(ref Rectangle oldRectangle)
        {
            freeRectangles.Add(oldRectangle);
        }

        public bool Insert(int width, int height, ref Rectangle bestRectangle)
        {
            return Insert(width, height, freeRectangles, ref bestRectangle);
        }

        public bool TryInsert(int width, int height, int count)
        {
            var bestRectangle = new Rectangle();
            tempFreeRectangles.Clear();
            tempFreeRectangles.AddRange(freeRectangles);

            for (var i = 0; i < count; ++i)
            {
                if (!Insert(width, height, tempFreeRectangles, ref bestRectangle))
                {
                    tempFreeRectangles.Clear();
                    return false;
                }
            }

            // if the insertion went well, use the new configuration
            freeRectangles.Clear();
            freeRectangles.AddRange(tempFreeRectangles);
            tempFreeRectangles.Clear();

            return true;
        }

        private static bool Insert(int width, int height, List<Rectangle> freeRectanglesList , ref Rectangle bestRectangle)
        {
            // Info on algorithm: http://clb.demon.fi/files/RectangleBinPack.pdf
            int bestScore = int.MaxValue;
            int freeRectangleIndex = -1;

            // Find space for new rectangle
            for (int i = 0; i < freeRectanglesList.Count; ++i)
            {
                var currentFreeRectangle = freeRectanglesList[i];
                if (width == currentFreeRectangle.Width && height == currentFreeRectangle.Height)
                {
                    // Perfect fit
                    bestRectangle.X = currentFreeRectangle.X;
                    bestRectangle.Y = currentFreeRectangle.Y;
                    bestRectangle.Width = width;
                    bestRectangle.Height = height;
                    freeRectangleIndex = i;
                    break;
                }
                if (width <= currentFreeRectangle.Width && height <= currentFreeRectangle.Height)
                {
                    // Can fit inside
                    // Use "BAF" heuristic (best area fit)
                    var score = currentFreeRectangle.Width * currentFreeRectangle.Height - width * height;
                    if (score < bestScore)
                    {
                        bestRectangle.X = currentFreeRectangle.X;
                        bestRectangle.Y = currentFreeRectangle.Y;
                        bestRectangle.Width = width;
                        bestRectangle.Height = height;
                        bestScore = score;
                        freeRectangleIndex = i;
                    }
                }
            }

            // No space could be found
            if (freeRectangleIndex == -1)
                return false;

            var freeRectangle = freeRectanglesList[freeRectangleIndex];

            // Choose an axis to split (trying to minimize the smaller area "MINAS")
            int w = freeRectangle.Width - bestRectangle.Width;
            int h = freeRectangle.Height - bestRectangle.Height;
            var splitHorizontal = (bestRectangle.Width * h > w * bestRectangle.Height);

            // Form the two new rectangles.
            var bottom = new Rectangle { X = freeRectangle.X, Y = freeRectangle.Y + bestRectangle.Height, Width = splitHorizontal ? freeRectangle.Width : bestRectangle.Width, Height = h };
            var right = new Rectangle { X = freeRectangle.X + bestRectangle.Width, Y = freeRectangle.Y, Width = w, Height = splitHorizontal ? bestRectangle.Height : freeRectangle.Height };

            if (bottom.Width > 0 && bottom.Height > 0)
                freeRectanglesList.Add(bottom);
            if (right.Width > 0 && right.Height > 0)
                freeRectanglesList.Add(right);

            // Remove previously selected freeRectangle
            if (freeRectangleIndex != freeRectanglesList.Count - 1)
                freeRectanglesList[freeRectangleIndex] = freeRectanglesList[freeRectanglesList.Count - 1];
            freeRectanglesList.RemoveAt(freeRectanglesList.Count - 1);

            return true;
        }
    }
}