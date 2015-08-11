
namespace SiliconStudio.Paradox.Assets.Textures.Packing
{
    /// <summary>
    /// RotableRectangle adds a rotating status to Rectangle struct type indicating that this rectangle is rotated by 90 degree and that width and height is swapped.
    /// </summary>
    public struct RotableRectangle
    {
        /// <summary>
        /// The starting position of the rectangle along X.
        /// </summary>
        public int X;

        /// <summary>
        /// The starting position of the rectangle along Y.
        /// </summary>
        public int Y;

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the rectangle
        /// </summary>
        public int Height;

        /// <summary>
        /// Gets or sets a rotation flag to indicate that this rectangle is rotated by 90 degree
        /// </summary>
        public bool IsRotated;

        /// <summary>
        /// Initializes a new instance of RotableRectangle with top-left position: x, y, width and height of the rectangle with an optional key 
        /// </summary>
        /// <param name="x">Left value in X axis</param>
        /// <param name="y">Top value in Y axis</param>
        /// <param name="width">Width of a rectangle</param>
        /// <param name="height">Height of a rectangle</param>
        /// <param name="isRotated">Indicate if the rectangle is rotated or not</param>
        public RotableRectangle(int x, int y, int width, int height, bool isRotated = false)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsRotated = isRotated;
        }
    }
}
