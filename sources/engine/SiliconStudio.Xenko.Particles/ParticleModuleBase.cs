// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles
{
    [Flags]
    public enum InheritLocation
    {
        Position = 1,
        Rotation = 2,
        Scale = 4,
    }

    [DataContract("PaticleModuleBase")]
    public abstract class ParticleModuleBase
    {
        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);


        [DataMemberIgnore]
        public Vector3 WorldPosition { get; private set; } = new Vector3(0, 0, 0);
        [DataMemberIgnore]
        public Quaternion WorldRotation { get; private set; } = new Quaternion(0, 0, 0, 1);
        [DataMemberIgnore]
        public float WorldScale { get; private set; } = 1f;

        /// <summary>
        /// The translation relative to the parent transformation.
        /// </summary>
        /// <userdoc>The translation of the entity with regard to its parent</userdoc>
        [DataMember(10)]
        public Vector3 Position = new Vector3(0, 0, 0);

        /// <summary>
        /// The rotation relative to the parent transformation.
        /// </summary>
        /// <userdoc>The rotation of the entity with regard to its parent</userdoc>
        [DataMember(12)]
        public Quaternion Rotation = new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// The scaling relative to the parent transformation.
        /// </summary>
        /// <userdoc>The scale of the entity with regard to its parent</userdoc>
        [DataMember(14)]
        public float Scale = 1f;

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(5)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Position | InheritLocation.Rotation | InheritLocation.Scale;

        public void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasPos = InheritLocation.HasFlag(InheritLocation.Position);
            var hasRot = InheritLocation.HasFlag(InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            WorldScale = (hasScl) ? this.Scale * Scale : this.Scale;

            // TODO I think this is correct, but it needs testing. q1 -> q2 rotation should be q' = q2 x q1
            WorldRotation = (hasRot) ? this.Rotation * Rotation : this.Rotation;

            // The position is the most difficult to calculate, because it involves rotation and scaling as well

            // Confirm Inherit Scale flag doesn't matter here. Position should always be scaled.
            var ownPosition = this.Position * Scale;

            Rotation.Rotate(ref ownPosition);

//            // Confirm Inherit Rotation flag doesn't matter here. Position should always be rotated.
//            var pureQuaternion = new Quaternion(ownPosition, 0);
//            // TODO I think this is correct, but it needs testing. v' = q x v x q*, but we write the operations backwards (see the note above):
//            pureQuaternion = Quaternion.Conjugate(Rotation) * pureQuaternion * Rotation;
//            ownPosition = new Vector3(pureQuaternion.X, pureQuaternion.Y, pureQuaternion.Z);

            WorldPosition = (hasPos) ? Translation + ownPosition : ownPosition;
        }
    }
}
