// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;
using SiliconStudio.Xenko.Particles.Updaters.FieldShapes;

namespace SiliconStudio.Xenko.Particles
{
    [Flags]
    public enum InheritLocation
    {
        Position = 1,
        Rotation = 2,
        Scale = 4,
    }

    /// <summary>
    /// The <see cref="PaticleModuleBase"/> is a base class for all plugins (initializers and updaters) used by the emitter
    /// Each plugin operates over one or several <see cref="ParticleFields"/> updating or setting up the particle state
    /// Additionally, each plugin can inherit some properties from the parent particle system, which are usually passed by the user.
    /// </summary>
    [DataContract("PaticleModule")]
    public abstract class ParticleModule
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ParticleModule"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember(-10)]
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Should this Particle Module's bounds be displayed as a debug draw
        /// </summary>
        /// <userdoc>
        /// Display the Particle Module's bounds as a wireframe debug shape. Temporary feature (will be removed later)!
        /// </userdoc>
        [DataMember(-1)]
        [DefaultValue(false)]
        public bool DebugDraw { get; set; } = false;

        public virtual bool TryGetDebugDrawShape(ref DebugDrawShape debugDrawShape, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            return false;
        }

        /// <summary>
        /// A list of fields required by the module to operate properly.
        /// Please fill it during construction time.
        /// </summary>
        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(1)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Position | InheritLocation.Rotation | InheritLocation.Scale;

        [DataMember(5)]
        [Display("Offset")]
        public ParticleLocator ParticleLocator { get; set; } = new ParticleLocator();

        [DataMemberIgnore]
        public Vector3 WorldPosition { get; private set; } = new Vector3(0, 0, 0);
        [DataMemberIgnore]
        public Quaternion WorldRotation { get; private set; } = new Quaternion(0, 0, 0, 1);
        [DataMemberIgnore]
        public Vector3 WorldScale { get; private set; } = new Vector3(1, 1, 1);

        /// <summary>
        /// Sets the parent (particle system's) translation, rotation and scale (uniform)
        /// The module can choose to inherit, use or ignore any of the elements
        /// </summary>
        /// <param name="Translation">Particle System's translation (from the Transform component)</param>
        /// <param name="Rotation">Particle System's quaternion rotation (from the Transform component)</param>
        /// <param name="Scale">Particle System's uniform scale (from the Transform component)</param>
        public virtual void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasPos = InheritLocation.HasFlag(InheritLocation.Position);
            var hasRot = InheritLocation.HasFlag(InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            WorldScale = (hasScl) ? ParticleLocator.Scale * Scale : ParticleLocator.Scale;

            WorldRotation = (hasRot) ? ParticleLocator.Rotation * Rotation : ParticleLocator.Rotation;

            var offsetTranslation = ParticleLocator.Translation * WorldScale;
            WorldRotation.Rotate(ref offsetTranslation);
            WorldPosition = (hasPos) ? Translation + offsetTranslation : offsetTranslation;
        }
    }
}
