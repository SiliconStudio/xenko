// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_UI_SDL
using System;

namespace SiliconStudio.Xenko.Graphics.SDL
{
    /// <summary>
    /// Representation of a rectangle with top and bottom coordinates, or with top coordinates and width and height.
    /// </summary>
    public struct Rect
    {
#region Initialization
        /// <summary>
        /// Initialize current instance with a rectangle of coordinates (left, top, right, bottom).
        /// </summary>
        /// <param name="left">X coordinate of top-left corner</param>
        /// <param name="top">Y coordinate of top-left corner</param>
        /// <param name="right">X coordinate of bottom-right corner</param>
        /// <param name="bottom">Y coordinate of bottom-right corner</param>
        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
#endregion

#region Access
        /// <summary>
        /// X coordinate of the top-left corner of current.
        /// </summary>
        public int Left { get; }
        public int X { get { return Left; } }

        /// <summary>
        /// Y coordinate of the top-left corner of current.
        /// </summary>
        public int Top { get; }
        public int Y { get { return Top; } }

        /// <summary>
        /// X coordinate of the bottom-right corner of current.
        /// </summary>
        public int Right { get; }

        /// <summary>
        /// Y coordinate of the bottom-right corner of current.
        /// </summary>
        public int Bottom { get; }

        /// <summary>
        /// Height of current.
        /// </summary>
        public int Height
        {
            get { return Bottom - Top; }
        }

        /// <summary>
        /// Width of current.
        /// </summary>
        public int Width
        {
            get { return Right - Left; }
        }

        /// <summary>
        /// Top-left coordinate as a Point.
        /// </summary>
        public Point Location
        {
            get { return new Point(Left, Top); }
        }

        /// <summary>
        /// Width and Height of current as a Size.
        /// </summary>
        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        /// <summary>
        /// Optimized version of GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // Taken from http://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Rectangle.cs,17559e21008f381d
            return (int)((UInt32)Left ^
                         (((UInt32)Top << 13) | ((UInt32)Top >> 19)) ^
                         (((UInt32)Width << 26) | ((UInt32)Width >> 6)) ^
                         (((UInt32)Height << 7) | ((UInt32)Height >> 25)));
        }
#endregion

#region Status report

        public bool IsEmpty { get { return Width == 0 && Height == 0; } }

        public bool Contains(Point pt)
        {
            if (IsEmpty)
            {
                return false;
            }
            else
            {
                return (pt.X >= Left) && (pt.Y >= Top) && (pt.X - Width <= Left) && (pt.Y - Height <= Bottom);
            }
        }
#endregion

#region Comparison
        /// <summary>
        /// The == operator to compare 2 Rect instances using <see cref="Equals(Rect)"/>.
        /// </summary>
        public static bool operator ==(Rect r1, Rect r2)
        {
            return r1.Equals(r2);
        }

        /// <summary>
        /// The ~= operator to compare 2 Rect instances using <see cref="Equals(Rect)"/>.
        /// </summary>
        public static bool operator !=(Rect r1, Rect r2)
        {
            return r1.Equals(r2);
        } 
        
        /// <summary>
        /// Optimized version of <see cref="Equals(object)"/> for Rect instances.
        /// </summary>
        /// <param name="r">Other rectangle to compare against.</param>
        /// <returns></returns>
        public bool Equals(Rect r)
        {
            return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
        }

        /// <inheritDoc/>
        public override bool Equals(object obj)
        {
            if (obj is Rect)
            {
                return Equals((Rect)obj);
            }
            else
            {
                return false;
            }
        }
#endregion

#region Output
        /// <inheritDoc/>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{left={0},top={1},right={2},bottom={3}}}", Left, Top, Right, Bottom);
        }
#endregion
    }
}
#endif
