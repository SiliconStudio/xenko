// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Mathematics
{
    /// <summary>
    /// Extensions methods of the vector classes.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Return the X/Y components of the vector.
        /// </summary>
        /// <param name="vector">the input vector</param>
        public static Vector2 XY(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <summary>
        /// Return the X/Z components of the vector.
        /// </summary>
        /// <param name="vector">the input vector</param>
        public static Vector2 XZ(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }

        /// <summary>
        /// Return the Y/Z components of the vector.
        /// </summary>
        /// <param name="vector">the input vector</param>
        public static Vector2 YZ(this Vector3 vector)
        {
            return new Vector2(vector.Y, vector.Z);
        }

        /// <summary>
        /// Return the X/Y components of the vector.
        /// </summary>
        /// <param name="vector">the input vector</param>
        public static Vector2 XY(this Vector4 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <summary>
        /// Return the X/Y/Z components of the vector.
        /// </summary>
        /// <param name="vector">the input vector</param>
        public static Vector3 XYZ(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}