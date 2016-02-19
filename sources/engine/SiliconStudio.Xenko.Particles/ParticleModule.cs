// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

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
    /// The <see cref="ParticleModule"/> is a base class for all plugins (initializers and updaters) used by the emitter
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
        /// Resets the current state to the module's initial state
        /// </summary>
        internal virtual void RestartSimulation() { }

        /// <summary>
        /// Should this Particle Module's bounds be displayed as a debug draw
        /// </summary>
        /// <userdoc>
        /// Display the Particle Module's bounds as a wireframe debug shape. Temporary feature (will be removed later)!
        /// </userdoc>
        [DataMember(-1)]
        [DefaultValue(false)]
        public bool DebugDraw { get; set; } = false;

        /// <summary>
        /// Attepmts to get a debug shape (shape type and location matrix) for the current module in order to display its boundaries better
        /// </summary>
        /// <param name="debugDrawShape">Type of the debug draw shape</param>
        /// <param name="translation">Translation of the shape</param>
        /// <param name="rotation">Rotation of the shape</param>
        /// <param name="scale">Scaling of the shape</param>
        /// <returns></returns>
        public virtual bool TryGetDebugDrawShape(out DebugDrawShape debugDrawShape, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            debugDrawShape = DebugDrawShape.None;
            scale = new Vector3(1, 1, 1);
            translation = new Vector3(0, 0, 0);
            rotation = new Quaternion(0, 0, 0, 1);
            return false;
        }

        /// <summary>
        /// A list of fields required by the module to operate properly.
        /// Please fill it during construction time.
        /// </summary>
        [DataMemberIgnore]
        public List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTrs is called. 
        /// This is intentional and reduces overhead, because SetParentTrs is called exactly once/turn.
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
        /// <param name="translation">Particle System's translation (from the Transform component)</param>
        /// <param name="rotation">Particle System's quaternion rotation (from the Transform component)</param>
        /// <param name="scale">Particle System's uniform scale (from the Transform component)</param>
        public virtual void SetParentTrs(ref Vector3 translation, ref Quaternion rotation, float scale)
        {
            var hasPos = InheritLocation.HasFlag(InheritLocation.Position);
            var hasRot = InheritLocation.HasFlag(InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            WorldScale = (hasScl) ? ParticleLocator.Scale * scale : ParticleLocator.Scale;

            WorldRotation = (hasRot) ? ParticleLocator.Rotation * rotation : ParticleLocator.Rotation;

            var offsetTranslation = ParticleLocator.Translation * WorldScale;
            WorldRotation.Rotate(ref offsetTranslation);
            WorldPosition = (hasPos) ? translation + offsetTranslation : offsetTranslation;
        }
    }
}
