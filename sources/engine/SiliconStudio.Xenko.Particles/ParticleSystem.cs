// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleSystem")]
    public class ParticleSystem
    {
        private const int DefaultMaxEmitters = 16;

        private SafeList<ParticleEmitter> emitters;
            /// <summary>
        /// Gets the color transforms.
        /// </summary>
        /// <value>The transforms.</value>
        [DataMember(10)]
        [Category]
        [Display("Emitters", Expand = ExpandRule.Always)]
        // [NotNullItems] // Can't create non-derived classes if this attribute is set
        [MemberCollection(CanReorderItems = true)]
        public SafeList<ParticleEmitter> Emitters
        {
            get
            {
                return emitters;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ParticleSystem()
        {
            emitters = new SafeList<ParticleEmitter>();

            /*
            var emitter = new ParticleEmitter();

            emitter.AddModule(new GravityUpdater());
            emitter.AddModule(new SampleInitializer());

            emitters.Add(emitter);
            */
        }

        /// <summary>
        /// Updates the particles
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {
            
            foreach (var particleEmitter in Emitters)
            {
                particleEmitter.Update(dt, this);
            }
            
        }

        /// <summary>
        /// Draws the particles
        /// </summary>
        /// <param name="gtxContext"></param>
        public void Draw(object gtxContext)
        {
            
        }
    }
}
