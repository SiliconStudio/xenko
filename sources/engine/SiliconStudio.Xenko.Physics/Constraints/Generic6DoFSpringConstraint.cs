// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Physics
{
    public class Generic6DoFSpringConstraint : Generic6DoFConstraint
    {
        internal BulletSharp.Generic6DofSpringConstraint InternalGeneric6DofSpringConstraint;

        /// <summary>
        /// Enables the spring.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="onOff">if set to <c>true</c> [on off].</param>
        public void EnableSpring(int index, bool onOff)
        {
            InternalGeneric6DofSpringConstraint.EnableSpring(index, onOff);
        }

        /// <summary>
        /// Sets the damping.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="damping">The damping.</param>
        public void SetDamping(int index, float damping)
        {
            InternalGeneric6DofSpringConstraint.SetDamping(index, damping);
        }

        /// <summary>
        /// Sets the equilibrium point.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="val">The value.</param>
        public void SetEquilibriumPoint(int index, float val)
        {
            InternalGeneric6DofSpringConstraint.SetEquilibriumPoint(index, val);
        }

        /// <summary>
        /// Sets the stiffness.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="stiffness">The stiffness.</param>
        public void SetStiffness(int index, float stiffness)
        {
            InternalGeneric6DofSpringConstraint.SetStiffness(index, stiffness);
        }
    }
}
