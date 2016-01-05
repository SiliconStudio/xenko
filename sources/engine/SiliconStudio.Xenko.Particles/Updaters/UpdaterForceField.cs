// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;
using SiliconStudio.Xenko.Particles.Updaters.FieldShapes;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("UpdaterForceField")]
    [Display("ForceField")]
    public class UpdaterForceField : ParticleUpdater
    {
        public UpdaterForceField()
        {
            // A force field operates over the particle's position and velocity, updating them as required
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.Velocity);

            // Test purposes only
            RequiredFields.Add(ParticleFields.Color);
        }

        public override unsafe void Update(float dt, ParticlePool pool)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.Velocity))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);
            var colField = pool.GetField(ParticleFields.Color);

            var deltaVel = new Vector3(10, 10, 10) * dt;
            var deltaPos = deltaVel * (dt * 0.5f);

            foreach (var particle in pool)
            {
                var alongAxis  = new Vector3(0, 1, 0);
                var aroundAxis = new Vector3(0, 0, 1);
                var towardAxis = new Vector3(1, 0, 0);

                var particlePos = (*((Vector3*)particle[posField]));
                var particleVel = (*((Vector3*)particle[velField]));

                FieldShape?.PreUpdateField(WorldPosition, WorldRotation, new Vector3(1, 1, 1) * WorldScale);

                var forceStrength = FieldShape?.GetFieldStrength(particlePos, particleVel, out alongAxis, out aroundAxis, out towardAxis) ?? 1;

                // TODO Max and min magnitude + threshold
                forceStrength = 1f - forceStrength;

                forceStrength *= dt * WorldScale;

                // Apply the field's force
                //(*((Color4*)particle[colField])) = new Color(new Vector3(forceStrength, forceStrength, forceStrength), 1);

                // Vortex
                var forceVortex = 2f;
                var forceRepulse = -0.3f;
                var forceAlong = 4f;

                (*((Vector3*)particle[posField])) += aroundAxis * (forceStrength * forceVortex);

                (*((Vector3*)particle[posField])) += towardAxis * (forceStrength * forceRepulse);

                (*((Vector3*)particle[posField])) += alongAxis  * (forceStrength * forceAlong);

                //   (*((Vector3*)particle[velField])) += deltaVel;
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
        public float WorldScale { get; private set; } = 1f;

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasPos = InheritLocation.HasFlag(InheritLocation.Position);
            var hasRot = InheritLocation.HasFlag(InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            WorldScale = (hasScl) ? Scale : 1f;

            WorldRotation = (hasRot) ? Rotation : new Quaternion(0, 0, 0, 1);

            WorldPosition = (hasPos) ? Translation : new Vector3(0, 0, 0);
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
