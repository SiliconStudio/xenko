// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles
{
    /// <summary>
    /// <see cref="ParticleLocator"/> is a TRS (translation, rotation, scale) locator for objects inside the <see cref="ParticleSystem"/>
    /// </summary>
    [DataContract("ParticleLocator")]
    [Display("Locator")]
    public class ParticleLocator
    {
        /// <summary>
        /// The translation relative to the parent transformation.
        /// </summary>
        /// <userdoc>The translation of the entity with regard to its parent</userdoc>
        [DataMember(10)]
        public Vector3 Translation { get; set; } = new Vector3(0, 0, 0);


        /// <summary>
        /// The rotation relative to the parent transformation.
        /// </summary>
        /// <userdoc>The rotation of the entity with regard to its parent</userdoc>
        [DataMember(20)]
        public Quaternion Rotation { get; set; } = new Quaternion(0, 0, 0, 1);


        /// <summary>
        /// The scaling relative to the parent transformation.
        /// </summary>
        /// <userdoc>
        /// The scaling of the entity with regard to its parent. 
        /// If a child component can only use uniform scaling, it will inherit from the X factor only.
        /// </userdoc>
        [DataMember(30)]
        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);
    }
}
