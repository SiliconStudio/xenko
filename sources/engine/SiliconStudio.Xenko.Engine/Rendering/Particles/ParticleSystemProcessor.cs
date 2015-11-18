// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Rendering.Particles
{
    class ParticleSystemProcessor : EntityProcessor<ParticleSystemProcessor.ParticleSystemComponentState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleSystemProcessor"/> class.
        /// </summary>
        public ParticleSystemProcessor()
            : base(ParticleSystemComponent.Key, TransformComponent.Key)
        {
            ParticleSystems = new List<ParticleSystemComponentState>();
        }

        protected internal override void OnSystemAdd()
        {
        }

        public List<ParticleSystemComponentState> ParticleSystems { get; private set; }

        /// <summary>
        /// Update the particle system's state and particles.
        /// </summary>
        /// <param name="time"></param>
        public override void Update(GameTime time)
        {
            base.Update(time);

            ParticleSystems.Clear();
            foreach (var particleSystemStateKeyPair in enabledEntities)
            {
                if (particleSystemStateKeyPair.Value.ParticleSystemComponent.Enabled)
                {
                    // TODO Update the particle system here

                    var particleSystem = particleSystemStateKeyPair.Value.ParticleSystemComponent.ParticleSystem;

                    particleSystem.Update(0.016f);

                    ParticleSystems.Add(particleSystemStateKeyPair.Value);
                }
            }
        }

        /// <summary>
        /// Draw the particle system using the RenderContext.
        /// </summary>
        /// <param name="context"></param>
        public override void Draw(RenderContext context)
        {
            base.Draw(context);
        }

        protected override ParticleSystemComponentState GenerateAssociatedData(Entity entity)
        {
            return new ParticleSystemComponentState
            {
                ParticleSystemComponent = entity.Get(ParticleSystemComponent.Key),
                TransformComponent = entity.Get(TransformComponent.Key),
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, ParticleSystemComponentState associatedData)
        {
            return
                entity.Get(ParticleSystemComponent.Key) == associatedData.ParticleSystemComponent &&
                entity.Get(TransformComponent.Key) == associatedData.TransformComponent;
        }

        public class ParticleSystemComponentState
        {
            public ParticleSystemComponent ParticleSystemComponent;

            public TransformComponent TransformComponent;
        }
    }
}
