// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_UI_SDL2
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
#endregion

    }
}
#endif
