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
        [DataMember(1)]
        [Display("Offset")]
        public ParticleLocator ParticleLocator { get; set; } = new ParticleLocator();

        /// <summary>
        /// A list of fields required by the module to operate properly.
        /// Please fill it during construction time.
        /// </summary>
        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

        /// <summary>
        /// Sets the parent (particle system's) translation, rotation and scale (uniform)
        /// The module can choose to inherit, use or ignore any of the elements
        /// </summary>
        /// <param name="Translation">Particle System's translation (from the Transform component)</param>
        /// <param name="Rotation">Particle System's quaternion rotation (from the Transform component)</param>
        /// <param name="Scale">Particle System's uniform scale (from the Transform component)</param>
        public abstract void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale);

    }
}
