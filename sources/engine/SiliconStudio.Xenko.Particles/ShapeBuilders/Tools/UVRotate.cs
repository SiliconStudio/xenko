// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders.Tools
{
    public enum UvClockwiseRotation
    {
        [Display("No rotation")]
        Degree0,

        [Display("90 degrees")]
        Degree90,

        [Display("180 degrees")]
        Degree180,

        [Display("270 degrees")]
        Degree270,
    }

    /// <summary>
    /// Struct used to rotate and/or flip texture coordinates
    /// </summary>
    [DataContract("UVRotate")]
    [Display("UV Rotate")]
    public struct UVRotate
    {
        private bool flipX;
        private bool flipY;
        private UvClockwiseRotation uvClockwise;

        private bool innerFlipX;
        private bool innerFlipY;
        private bool innerRotated;

        [Display("Flip Hor")]
        public bool FlipX { get { return flipX; } set { flipX = value; ApplyChanges(); } }

        [Display("Flip Ver")]
        public bool FlipY { get { return flipY; } set { flipY = value; ApplyChanges(); } }

        [Display("Clockwise")]
        public UvClockwiseRotation UvClockwise { get { return uvClockwise; } set { uvClockwise = value; ApplyChanges(); } }

        private void ApplyChanges()
        {
            innerFlipX = (FlipX ^ (uvClockwise == UvClockwiseRotation.Degree180 || uvClockwise == UvClockwiseRotation.Degree90));
            innerFlipY = (FlipY ^ (uvClockwise == UvClockwiseRotation.Degree180 || uvClockwise == UvClockwiseRotation.Degree270));
            innerRotated = (uvClockwise == UvClockwiseRotation.Degree90 || uvClockwise == UvClockwiseRotation.Degree270);
        }

        /// <summary>
        /// Returns the rotated texture coordinates for base input texture coordinates
        /// </summary>
        /// <param name="inVector">Base texture coordinates</param>
        /// <returns>Rotated and flipped coordinates</returns>
        public Vector2 GetCoords(Vector2 inVector)
        {
            var xPrime = (innerFlipX) ? 1f - inVector.X : inVector.X;
            var yPrime = (innerFlipY) ? 1f - inVector.Y : inVector.Y;

            return (innerRotated) ? new Vector2(yPrime, xPrime) : new Vector2(xPrime, yPrime);
        }
    }

}
