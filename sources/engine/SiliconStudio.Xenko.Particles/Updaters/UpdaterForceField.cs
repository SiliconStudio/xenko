// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;
using SiliconStudio.Xenko.Particles.Updaters.FieldShapes;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("UpdaterForceField")]
    [Display("ForceField")]
    public class UpdaterForceField : ParticleUpdater
    {
        /// <summary>
        /// Defines how and if the total magnitude of the force should change depending of how far away the particle is from the central axis
        /// </summary>
        /// <userdoc>
        /// Defines how and if the total magnitude of the force should change depending of how far away the particle is from the central axis
        /// </userdoc>
        [DataMember(50)]
        [Display("Falloff")]
        public FieldFalloff FieldFalloff { get; set; } = new FieldFalloff();

        /// <summary>
        /// How much of the force should be applied as conserved energy (acceleration)
        /// </summary>
        /// <userdoc>
        /// With no concervation (0) particles will cease to move when the force disappears (physically incorrect, but easier to control).
        /// With energy concervation (1) particles will retain energy and gradually accelerate, continuing to move even when the force
        /// cease to exist (physically correct, but more difficult to control).
        /// </userdoc>
        [DataMember(50)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Energy conservation")]
        public float EnergyConservation { get; set; } = 0f;

        /// <summary>
        /// The force ALONG the bounding shape's axis.
        /// </summary>
        /// <userdoc>
        /// The force ALONG the bounding shape's axis.
        /// </userdoc>
        [DataMember(60)]
        [Display("Directed force")]
        public float ForceDirected { get; set; }

        /// <summary>
        /// The force AROUND the bounding shape's axis.
        /// </summary>
        /// <userdoc>
        /// The force AROUND the bounding shape's axis.
        /// </userdoc>
        [DataMember(70)]
        [Display("Vortex force")]
        public float ForceVortex { get; set; } = 1f;

        /// <summary>
        /// The force AWAY from the bounding shape's axis.
        /// </summary>
        /// <userdoc>
        /// The force AWAY from the bounding shape's axis.
        /// </userdoc>
        [DataMember(80)]
        [Display("Repulsive force")]
        public float ForceRepulsive { get; set; } = 1f;

        public UpdaterForceField()
        {
            // A force field operates over the particle's position and velocity, updating them as required
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.Velocity);

            // Test purposes only
//            RequiredFields.Add(ParticleFields.Color);
        }

        public override unsafe void Update(float dt, ParticlePool pool)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.Velocity))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);
//            var colField = pool.GetField(ParticleFields.Color);

            var directToPosition = 1f - EnergyConservation;

            foreach (var particle in pool)
            {
                var alongAxis  = new Vector3(0, 1, 0);
                var awayAxis   = new Vector3(0, 0, 1);
                var aroundAxis = new Vector3(1, 0, 0);

                var particlePos = (*((Vector3*)particle[posField]));
                var particleVel = (*((Vector3*)particle[velField]));

                var forceMagnitude = 1f;
                if (FieldShape != null)
                {
                    FieldShape.PreUpdateField(WorldPosition, WorldRotation, WorldScale);

                    forceMagnitude = FieldShape.GetDistanceToCenter(particlePos, particleVel, out alongAxis, out aroundAxis, out awayAxis);

                    forceMagnitude = FieldFalloff.GetStrength(forceMagnitude);

                }
                forceMagnitude *= dt * parentScale;

                var totalForceVector = 
                    alongAxis  * ForceDirected +
                    aroundAxis * ForceVortex +
                    awayAxis * ForceRepulsive;

                totalForceVector *= forceMagnitude;
               
                // Force contribution to velocity - conserved energy
                var vectorContribution = totalForceVector * EnergyConservation;
                (*((Vector3*)particle[velField])) += vectorContribution;

                // Force contribution to position - lost energy
                vectorContribution = (vectorContribution * (dt * 0.5f)) + (totalForceVector * directToPosition);
                (*((Vector3*)particle[posField])) += vectorContribution;
            }
        }

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(5)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Position | InheritLocation.Rotation | InheritLocation.Scale;

        [DataMember(10)]
        [Display("Shape")]
        public FieldShape FieldShape { get; set; }

        [DataMemberIgnore]
        public Vector3 WorldPosition { get; private set; } = new Vector3(0, 0, 0);
        [DataMemberIgnore]
        public Quaternion WorldRotation { get; private set; } = new Quaternion(0, 0, 0, 1);
        [DataMemberIgnore]
        public Vector3 WorldScale { get; private set; } = new Vector3(1, 1, 1);

        [DataMemberIgnore]
        private float parentScale = 1f;

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasPos = InheritLocation.HasFlag(InheritLocation.Position);
            var hasRot = InheritLocation.HasFlag(InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            parentScale = (hasScl) ? Scale : 1f;

            WorldScale = (hasScl) ? ParticleLocator.Scale * Scale : ParticleLocator.Scale;

            WorldRotation = (hasRot) ? ParticleLocator.Rotation * Rotation : ParticleLocator.Rotation;

            var offsetTranslation = ParticleLocator.Translation * WorldScale;
            WorldRotation.Rotate(ref offsetTranslation);
            WorldPosition = (hasPos) ? Translation + offsetTranslation : offsetTranslation;
        }


        public override bool TryGetDebugDrawShape(ref DebugDrawShape debugDrawShape, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            rotation = new Quaternion(0, 0, 0, 1);
            scale = new Vector3(1, 1, 1);
            translation = new Vector3(0, 0, 0);

            debugDrawShape = FieldShape?.GetDebugDrawShape(out translation, out rotation, out scale) ?? DebugDrawShape.None;

            rotation *= WorldRotation;

            scale *= WorldScale;

            translation *= WorldScale;
            rotation.Rotate(ref translation);
            translation += WorldPosition;

            return true;
        }
    }
}
