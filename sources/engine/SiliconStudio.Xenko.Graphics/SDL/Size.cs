// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_UI_SDL
namespace SiliconStudio.Xenko.Graphics.SDL
{
    /// <summary>
    /// Representation of a size by its width and height.
    /// </summary>
    public struct Size
    {
#region Initialization
        /// <summary>
        /// Initialize current with <paramref name="width"/> and <paramref name="height."/>
        /// </summary>
        /// <param name="width">The width for the new size.</param>
        /// <param name="height">The height for the new size.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
#endregion

#region Access
        /// <summary>
        /// Width of current.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of current.
        /// </summary>
        public int Height { get; }
        
        /// <inheritDoc/>
        public override int GetHashCode()
        {
            return Width * 31 + Height;
        }

#endregion

#region Status report
        public bool IsEmpty
        {
            get { return Width == 0 && Height == 0; }
        }
#endregion

#region Comparison
        /// <summary>
        /// The == operator to compare 2 Size instances using <see cref="Equals(Size)"/>.
        /// </summary>
        public static bool operator ==(Size r1, Size r2)
        {
            return r1.Equals(r2);
        }

        /// <summary>
        /// The != operator to compare 2 Size instances using <see cref="Equals(Size)"/>.
        /// </summary>
        public static bool operator !=(Size r1, Size r2)
        {
            return !r1.Equals(r2);
        }

        /// <summary>
        /// Optimized version of <see cref="Equals(object)"/> for Size instances.
        /// </summary>
        /// <param name="o">Other Size instance to compare against.</param>
        /// <returns></returns>
        public bool Equals(Size o)
        {
            return (Width == o.Width) && (Height == o.Height);
        }

        /// <inheritDoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Size))
            {
                return false;
            }
            else
            {
                return Equals((Size) obj);
            }
        }
#endregion
    }
}
#endif
