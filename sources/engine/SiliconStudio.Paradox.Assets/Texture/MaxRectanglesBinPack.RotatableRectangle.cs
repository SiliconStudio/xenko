using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Assets.Texture
{
    public partial class MaxRectanglesBinPack
    {
        /// <summary>
        /// RotatableRectangle adds a rotating status to Rectangle struct type indicating that this rectangle is rotated by 90 degree-
        /// and that width and height is swapped.
        /// It also contains a key name.
        /// </summary>
        public struct RotatableRectangle
        {
            /// <summary>
            /// Gets or Sets a Key for this rectangle
            /// </summary>
            public string Key;

            /// <summary>
            /// Gets or Sets rectangle value whose width and height is swapped when the rectangle is rotated
            /// </summary>
            public Rectangle Value;

            /// <summary>
            /// Gets or Sets a rotation flag to indicate that this rectangle is rotated by 90 degree
            /// </summary>
            public bool IsRotated;

            /// <summary>
            /// Initializes a new instance of RotatableRectangle with top-left position: x, y, width and height of the rectangle with an optional key 
            /// </summary>
            /// <param name="x">Left value in X axis</param>
            /// <param name="y">Top value in Y axis</param>
            /// <param name="width">Width of a rectangle</param>
            /// <param name="height">Height of a rectangle</param>
            /// <param name="key">Key of a rectangle which is null by default when it is not specified</param>
            public RotatableRectangle(int x, int y, int width, int height, string key = null)
            {
                Key = key;
                Value = new Rectangle(x, y, width, height);
                IsRotated = false;
            }

            /// <summary>
            /// Initializes a new instance of RotatableRectangle with a rectangle with an optional key 
            /// </summary>
            /// <param name="value">A rectangle struct</param>
            /// <param name="key">Key of a rectangle which is null by default when it is not specified</param>
            public RotatableRectangle(Rectangle value, string key = null)
            {
                Key = key;
                IsRotated = false;
                Value = value;
            }
        }   
    }
}
