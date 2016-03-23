// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    public class GearConstraint : Constraint
    {
        internal BulletSharp.GearConstraint InternalGearConstraint;

        /// <summary>
        /// Gets or sets the axis a.
        /// </summary>
        /// <value>
        /// The axis a.
        /// </value>
        public Vector3 AxisA
        {
            get { return InternalGearConstraint.AxisA; }
            set { InternalGearConstraint.AxisA = value; }
        }

        /// <summary>
        /// Gets or sets the axis b.
        /// </summary>
        /// <value>
        /// The axis b.
        /// </value>
        public Vector3 AxisB
        {
            get { return InternalGearConstraint.AxisB; }
            set { InternalGearConstraint.AxisB = value; }
        }

        /// <summary>
        /// Gets or sets the ratio.
        /// </summary>
        /// <value>
        /// The ratio.
        /// </value>
        public float Ratio
        {
            get { return InternalGearConstraint.Ratio; }
            set { InternalGearConstraint.Ratio = value; }
        }
    }
}
