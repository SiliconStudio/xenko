// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.Mathematics
{
    /// <summary>
    /// Represents an axis-aligned bounding box in three dimensional space that store only the Center and Extent.
    /// </summary>
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BoundingBoxExt : IEquatable<BoundingBoxExt>
    {
        /// <summary>
        /// The center of this bounding box.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The extent of this bounding box.
        /// </summary>
        public Vector3 Extent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiliconStudio.Core.Mathematics.BoundingBoxExt" /> struct.
        /// </summary>
        /// <param name="box">The box.</param>
        public BoundingBoxExt(BoundingBox box)
        {
            this.Center = box.Center;
            this.Extent = box.Extent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiliconStudio.Core.Mathematics.BoundingBoxExt"/> struct.
        /// </summary>
        /// <param name="minimum">The minimum vertex of the bounding box.</param>
        /// <param name="maximum">The maximum vertex of the bounding box.</param>
        public BoundingBoxExt(Vector3 minimum, Vector3 maximum)
        {
            this.Center = (minimum + maximum) / 2;
            this.Extent = (maximum - minimum) / 2;
        }

        /// <summary>
        /// Gets the minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public Vector3 Minimum
        {
            get
            {
                return Center - Extent;
            }
        }

        /// <summary>
        /// Gets the maximum.
        /// </summary>
        /// <value>The maximum.</value>
        public Vector3 Maximum
        {
            get
            {
                return Center + Extent;
            }
        }

        /// <summary>
        /// Transform this Bounding box
        /// </summary>
        /// <param name="world"></param>
        public void Transform(Matrix world)
        {
            Transform(ref world);
        }

        /// <summary>
        /// Transform this Bounding box (the world matrix will be modified).
        /// </summary>
        /// <param name="world"></param>
        public void Transform(ref Matrix world)
        {
            // http://zeuxcg.org/2010/10/17/aabb-from-obb-with-component-wise-abs/
            // Compute transformed AABB (by world)
            var center = Center;
            var extent = Extent;

            Vector3.TransformCoordinate(ref center, ref world, out Center);

            // Update world matrix into absolute form
            unsafe
            {
                fixed (void* pMatrix = &world)
                {
                    // Perform an abs on the matrix
                    var matrixData = (float*)pMatrix;
                    for (int j = 0; j < 16; ++j)
                    {
                        //*matrixData &= 0x7FFFFFFF;
                        *matrixData = Math.Abs(*matrixData);
                        ++matrixData;
                    }
                }
            }

            Vector3.TransformNormal(ref extent, ref world, out Extent);
        }

        public bool Equals(BoundingBoxExt other)
        {
            return Center.Equals(other.Center) && Extent.Equals(other.Extent);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BoundingBoxExt && Equals((BoundingBoxExt)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Center.GetHashCode() * 397) ^ Extent.GetHashCode();
            }
        }

        /// <summary>
        /// Implements the ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(BoundingBoxExt left, BoundingBoxExt right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(BoundingBoxExt left, BoundingBoxExt right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BoundingBoxExt"/> to <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="bbExt">The bb ext.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator BoundingBox(BoundingBoxExt bbExt)
        {
            return new BoundingBox(bbExt.Minimum, bbExt.Maximum);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BoundingBox"/> to <see cref="BoundingBoxExt"/>.
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator BoundingBoxExt(BoundingBox boundingBox)
        {
            return new BoundingBoxExt(boundingBox);
        }
    }
}