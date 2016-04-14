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
    /// <summary>
    /// The <see cref="ParticleModule"/> is a base class for all plugins (initializers and updaters) used by the emitter
    /// Each plugin operates over one or several <see cref="ParticleFields"/> updating or setting up the particle state
    /// Additionally, each plugin can inherit some properties from the parent particle system, which are usually passed by the user.
    /// </summary>
    [DataContract("PaticleModule")]
    public abstract class ParticleModule : ParticleTransform
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
        internal virtual void ResetSimulation() { }

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
            rotation = Quaternion.Identity;
            return false;
        }

        /// <summary>
        /// A list of fields required by the module to operate properly.
        /// Please fill it during construction time.
        /// </summary>
        [DataMemberIgnore]
        public List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

        /// <summary>
        /// Sets the parent (particle system's) translation, rotation and scale (uniform)
        /// The module can choose to inherit, use or ignore any of the elements
        /// </summary>
        /// <param name="transform"><see cref="ParticleSystem"/>'s transform (from the Transform component) or identity if local space is used</param>
        /// <param name="parent">The parent <see cref="ParticleSystem"/></param>
        public virtual void SetParentTRS(ParticleTransform transform, ParticleSystem parent)
        {
            SetParentTransform(transform);
        }

        /// <summary>
        /// Invalidates relation of this emitter to any other emitters that might be referenced
        /// </summary>
        public virtual void InvalidateRelations()
        {
            
        }
    }
}
