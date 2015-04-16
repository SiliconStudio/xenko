// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.Mathematics
{
    /// <summary>
    /// A bounding frustum.
    /// </summary>
    public struct BoundingFrustum
    {
        public Plane Plane1;
        public Plane Plane2;
        public Plane Plane3;
        public Plane Plane4;
        public Plane Plane5;
        public Plane Plane6;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingFrustum"/> struct from a matrix view-projection.
        /// </summary>
        /// <param name="matrix">The matrix view projection.</param>
        public BoundingFrustum(ref Matrix matrix)
        {
            // Left
            Plane1 = Plane.Normalize(new Plane(
                matrix.M14 + matrix.M11,
                matrix.M24 + matrix.M21,
                matrix.M34 + matrix.M31,
                matrix.M44 + matrix.M41));

            // Right
            Plane2 = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M11,
                matrix.M24 - matrix.M21,
                matrix.M34 - matrix.M31,
                matrix.M44 - matrix.M41));

            // Top
            Plane3 = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M12,
                matrix.M24 - matrix.M22,
                matrix.M34 - matrix.M32,
                matrix.M44 - matrix.M42));

            // Bottom
            Plane4 = Plane.Normalize(new Plane(
                matrix.M14 + matrix.M12,
                matrix.M24 + matrix.M22,
                matrix.M34 + matrix.M32,
                matrix.M44 + matrix.M42));

            // Near
            Plane5 = Plane.Normalize(new Plane(
                matrix.M13,
                matrix.M23,
                matrix.M33,
                matrix.M43));

            // Far
            Plane6 = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M13,
                matrix.M24 - matrix.M23,
                matrix.M34 - matrix.M33,
                matrix.M44 - matrix.M43));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ref BoundingBoxExt boundingBoxExt)
        {
            return Collision.FrustumContainsBox(ref this, ref boundingBoxExt);
        }
    }
}