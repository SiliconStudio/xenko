using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Particles.Components;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    public class ParticleSystemRenderProcessor : EntityProcessor<ParticleSystemComponent, RenderParticleSystem>
    {
        private VisibilityGroup visibilityGroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleSystemRenderProcessor"/> class.
        /// </summary>
        public ParticleSystemRenderProcessor()
            : base(typeof(TransformComponent))
        {
        }

        protected override void OnSystemAdd()
        {
            visibilityGroup = ((SceneInstance)EntityManager).VisibilityGroup;
        }

        public override void Draw(RenderContext gameTime)
        {
            foreach (var componentData in ComponentDatas)
            {
                if (componentData.Value.ParticleSystemComponent.Enabled)
                {
                    // Update render objects
                }
            }
        }

        protected override void OnEntityComponentAdding(Entity entity, ParticleSystemComponent particleSystemComponent, RenderParticleSystem renderParticleSystem)
        {
            // TODO GRAPHICS REFACTOR: Handle enabled emitters (in visibility system)

            var emitters = particleSystemComponent.ParticleSystem.Emitters;
            var emitterCount = emitters.Count;
            var renderEmitters = new RenderParticleEmitter[emitterCount];

            for (int index = 0; index < emitterCount; index++)
            {
                visibilityGroup.RenderObjects.Add(new RenderParticleEmitter
                {
                    ParticleEmitter = emitters[index],
                    RenderParticleSystem = renderParticleSystem,
                });
            }

            renderParticleSystem.Emitters = renderEmitters;
        }

        protected override void OnEntityComponentRemoved(Entity entity, ParticleSystemComponent particleSystemComponent, RenderParticleSystem renderParticleSystem)
        {
            foreach (var emitter in renderParticleSystem.Emitters)
            {
                visibilityGroup.RenderObjects.Remove(emitter);
            }
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