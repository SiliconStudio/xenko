// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleSystem")]
    public class ParticleSystem
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ParticleSystem"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember(-10)]
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

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

            emitter.AddModule(new UpdaterGravity());
            emitter.AddModule(new SampleInitializer());

            emitters.Add(emitter);
            */
        }

        /// <summary>
        /// Translation of the ParticleSystem. Usually inherited directly from the ParticleSystemComponent.
        /// </summary>
        [DataMemberIgnore]
        public Vector3 Translation = new Vector3(0, 0, 0);

        /// <summary>
        /// Rotation of the ParticleSystem, expressed as a quaternion rotation. Usually inherited directly from the ParticleSystemComponent.
        /// </summary>
        [DataMemberIgnore]
        public Quaternion Rotation = new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// Scale of the ParticleSystem. Only uniform scale is supported. Usually inherited directly from the ParticleSystemComponent.
        /// </summary>
        [DataMemberIgnore]
        public float UniformScale = 1f;

        /// <summary>
        /// Updates the particles
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {           
            foreach (var particleEmitter in Emitters)
            {
                if (particleEmitter.Enabled)
                {
                    particleEmitter.Update(dt, this);
                }
            }            
        }

        /// <summary>
        /// Draws the particles
        /// </summary>
        public void Draw(ParticleBatch particleBatch, RenderContext context, Color4 color)
        {
            // TODO Use color4 tint

            foreach (var particleEmitter in Emitters)
            {
                if (particleEmitter.Enabled)
                {
                    particleBatch.Draw(particleEmitter, context, color);
                }
            }
        }

        /// <summary>
        /// Build particle vertices to the given mapped vertex buffer.
        /// </summary>
        /// <param name="vertexBuffer"></param>
        /// <param name="invViewX"></param>
        /// <param name="invViewY"></param>
        /// <param name="remainingCapacity"></param>
        /// <returns>Total number of quads drawn. 1 quad = 2 triangles = 4 vertices.</returns>
        public int BuildVertexBuffer(GraphicsDevice device, IntPtr vertexBuffer, Vector3 invViewX, Vector3 invViewY, ref int remainingCapacity)
        {
            var totalParticlesDrawn = 0;
            foreach (var particleEmitter in Emitters)
            {
                totalParticlesDrawn += particleEmitter.BuildVertexBuffer(device, vertexBuffer, invViewX, invViewY, ref remainingCapacity);
            }

            return totalParticlesDrawn;
        }
    }
}
