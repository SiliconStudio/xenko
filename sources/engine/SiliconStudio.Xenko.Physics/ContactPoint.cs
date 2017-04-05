// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ContactPoint
    {
        internal readonly IntPtr ColliderA;
        internal readonly IntPtr ColliderB;

        public readonly float Distance;
        public readonly Vector3 Normal;
        public readonly Vector3 PositionOnA;
        public readonly Vector3 PositionOnB;
    }

    public class ContactPointEqualityComparer : EqualityComparer<ContactPoint>
    {
        /// <summary>
        /// Gets the default.
        /// </summary>
        public new static readonly ContactPointEqualityComparer Default = new ContactPointEqualityComparer();

        /// <inheritdoc/>
        public override bool Equals(ContactPoint x, ContactPoint y)
        {
            return (x.ColliderA == y.ColliderA && x.ColliderB == y.ColliderB) || (x.ColliderA == y.ColliderB && x.ColliderB == y.ColliderA);
        }

        /// <inheritdoc/>
        public override int GetHashCode(ContactPoint obj)
        {
            return 397 * obj.ColliderA.GetHashCode() * obj.ColliderB.GetHashCode();
        }
    }
}
