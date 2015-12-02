// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_UI_SDL

namespace SiliconStudio.Xenko.Graphics.SDL
{
    /// <summary>
    /// Representation of a coordinate (x, y) as a Point.
    /// </summary>
    public struct Point
    {
#region Initialization
        public Point(int aX, int aY)
        {
            X = aX;
            Y = aY;
        }

        public static readonly Point Empty = new Point();
#endregion

#region Access
        /// <summary>
        /// X coordinate of current instance.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y coordinate of current instance.
        /// </summary>
        public int Y { get; }

        /// <inheritDoc/>
        public override int GetHashCode()
        {
            return X * 31 + Y;
        }
#endregion

#region Comparison

        /// <summary>
        /// The == operator to compare 2 Point instances using <see cref="Equals(Point)"/>.
        /// </summary>
        public static bool operator ==(Point r1, Point r2)
        {
            return r1.Equals(r2);
        }

        /// <summary>
        /// The != operator to compare 2 Point instances using <see cref="Equals(Point)"/>.
        /// </summary>
        public static bool operator !=(Point r1, Point r2)
        {
            return !r1.Equals(r2);
        }

        /// <summary>
        /// Optimized version of <see cref="Equals(object)"/> for Point instances.
        /// </summary>
        /// <param name="o">Other Point instance to compare against.</param>
        /// <returns></returns>
        public bool Equals(Point o)
        {
            return (X == o.X) && (Y == o.Y);
        }

        /// <inheritDoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }
            else
            {
                return Equals((Point)obj);
            }
        }
#endregion
    }
}
#endif
