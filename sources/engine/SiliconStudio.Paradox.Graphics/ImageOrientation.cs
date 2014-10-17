namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Defines the possible rotations to apply on image regions.
    /// </summary>
    public enum ImageOrientation
    {
        /// <summary>
        /// The image region is taken as is.
        /// </summary>
        AsIs = 0,

        /// <summary>
        /// The image is rotated of the 90 degrees (clockwise) in the source texture.
        /// </summary>
        Rotated90 = 1,

        /// <summary>
        /// The image is rotated of the 180 degrees in the source texture.
        /// </summary>
        Rotated180 = 2,

        /// <summary>
        /// The image is rotated of the 90 degrees (counter-clockwise) in the source texture.
        /// </summary>
        Rotated90C = 3,
    }
}