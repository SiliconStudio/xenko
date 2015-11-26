using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Initializers;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("TestInitializer")]
    public class TestInitializer : InitializerBase
    {
        // TODO Change the RNG to a deterministic generator
        readonly Random randomNumberGenerator = new Random();

        public TestInitializer()
        {
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.Velocity);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.Velocity))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);

            var startPos = new Vector3(0, 0, 0) + WorldPosition;
            var startVel = new Vector3(0, 0, 0);

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);

                (*((Vector3*)particle[posField])) = startPos;

                startVel.X = ((float)randomNumberGenerator.NextDouble() * 4 - 2);
                startVel.Y = ((float)randomNumberGenerator.NextDouble() * 2 + 2);
                startVel.Z = ((float)randomNumberGenerator.NextDouble() * 4 - 2);
                (*((Vector3*)particle[velField])) = startVel;

                i = (i + 1) % maxCapacity;
            }
        }

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

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
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
