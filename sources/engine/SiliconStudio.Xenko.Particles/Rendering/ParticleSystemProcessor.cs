using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Particles.Components;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.Particles
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class NextGenSpriteProcessor : EntityProcessor<ParticleSystemComponent>
    {
        private NextGenRenderSystem renderSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextGenSpriteProcessor"/> class.
        /// </summary>
        public NextGenSpriteProcessor()
            : base(typeof(TransformComponent))
        {
            ParticleSystems = new List<RenderParticleSystem>();
        }

        public List<RenderParticleSystem> ParticleSystems { get; private set; }

        protected internal override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<NextGenRenderSystem>();
        }

        public override void Draw(RenderContext gameTime)
        {
            ParticleSystems.Clear();
            foreach (var particleSystemStateKeyPair in ComponentDatas)
            {
                if (particleSystemStateKeyPair.Value.ParticleSystemComponent.Enabled)
                {
                    ParticleSystems.Add(particleSystemStateKeyPair.Value);
                }
            }
        }

        protected override void OnEntityComponentAdding(Entity entity, ParticleSystemComponent spriteComponent, RenderParticleSystem data)
        {
            foreach (var particleEmitter in spriteComponent.ParticleSystem.Emitters)
            {
                
            }
            renderSystem.RenderObjects.Add(data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, ParticleSystemComponent spriteComponent, RenderParticleSystem data)
        {
            renderSystem.RenderObjects.Remove(data);
        }

        protected override RenderParticleSystem GenerateComponentData(Entity entity, ParticleSystemComponent particleSystemComponent)
        {
            return new RenderParticleSystem
            {
                ParticleSystemComponent = particleSystemComponent,
                TransformComponent = entity.Transform,
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, ParticleSystemComponent spriteComponent, RenderParticleSystem associatedData)
        {
            return
                spriteComponent == associatedData.ParticleSystemComponent &&
                entity.Transform == associatedData.TransformComponent;
        }
    }
}