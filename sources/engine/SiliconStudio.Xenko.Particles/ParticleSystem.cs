// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleSystem")]
    public class ParticleSystem
    {
        private const int DefaultMaxEmitters = 16;

        private readonly SafeList<ParticleEmitter> emitters;
            /// <summary>
        /// Gets the color transforms.
        /// </summary>
        /// <value>The transforms.</value>
        [DataMember(10)]
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

        /// <summary>
        /// Build particle vertices to the given mapped vertex buffer.
        /// </summary>
        /// <param name="vertexBuffer"></param>
        /// <param name="invViewX"></param>
        /// <param name="invViewY"></param>
        /// <param name="remainingCapacity"></param>
        /// <returns>Total number of quads drawn. 1 quad = 2 triangles = 4 vertices.</returns>
        public int BuildVertexBuffer(MappedResource vertexBuffer, Vector3 invViewX, Vector3 invViewY, ref int remainingCapacity)
        {
            var totalParticlesDrawn = 0;
            foreach (var particleEmitter in Emitters)
            {
                totalParticlesDrawn += particleEmitter.BuildVertexBuffer(vertexBuffer, invViewX, invViewY, ref remainingCapacity);
            }

            return totalParticlesDrawn;
        }
    }
}
