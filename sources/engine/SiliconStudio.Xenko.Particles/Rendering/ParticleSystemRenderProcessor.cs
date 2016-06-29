// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Particles.Components;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    public class ParticleSystemRenderProcessor : EntityProcessor<ParticleSystemComponent, RenderParticleSystem>, IEntityComponentRenderProcessor
    {
        public VisibilityGroup VisibilityGroup { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleSystemRenderProcessor"/> class.
        /// </summary>
        public ParticleSystemRenderProcessor()
            : base(typeof(TransformComponent))
        {
        }

        private void CheckEmitters(RenderParticleSystem renderParticleSystem)
        {
            if (renderParticleSystem == null)
                return;

            var emitters = renderParticleSystem.ParticleSystemComponent.ParticleSystem.Emitters;
            var emitterCount = emitters.Count;

            if (emitterCount == (renderParticleSystem.Emitters?.Length ?? 0))
                return;

            // Remove old emitters
            if (renderParticleSystem.Emitters != null)
            {
                foreach (var renderEmitter in renderParticleSystem.Emitters)
                {
                    VisibilityGroup.RenderObjects.Remove(renderEmitter);
                }
            }

            renderParticleSystem.Emitters = null;

            // Add new emitters
            var renderEmitters = new RenderParticleEmitter[emitterCount];
            for (int index = 0; index < emitterCount; index++)
            {
                var renderEmitter = new RenderParticleEmitter
                {
                    ParticleEmitter = emitters[index],
                    RenderParticleSystem = renderParticleSystem,
                };

                renderEmitters[index] = renderEmitter;
                VisibilityGroup.RenderObjects.Add(renderEmitter);
            }

            renderParticleSystem.Emitters = renderEmitters;
        }

        public override void Draw(RenderContext context)
        {
            base.Draw(context);

            foreach (var componentData in ComponentDatas)
            {
                var renderSystem = componentData.Value;

                CheckEmitters(renderSystem);


                // Update render objects
                foreach (var emitter in renderSystem.Emitters)
                {
                    if ((emitter.Enabled = renderSystem.ParticleSystemComponent.Enabled) == true)
                    {
                        var aabb = emitter.RenderParticleSystem.ParticleSystemComponent.ParticleSystem.GetAABB();
                        emitter.BoundingBox = new BoundingBoxExt(aabb.Minimum, aabb.Maximum);
                        emitter.StateSortKey = ((uint) emitter.ParticleEmitter.DrawPriority) << 16;     // Maybe include the RenderStage precision as well
                        emitter.RenderGroup = renderSystem.ParticleSystemComponent.Entity.Group;
                    }
                }
            }
        }

        protected override void OnEntityComponentAdding(Entity entity, ParticleSystemComponent particleSystemComponent, RenderParticleSystem renderParticleSystem)
        {
            var emitters = particleSystemComponent.ParticleSystem.Emitters;
            var emitterCount = emitters.Count;
            var renderEmitters = new RenderParticleEmitter[emitterCount];

            for (int index = 0; index < emitterCount; index++)
            {
                var renderEmitter = new RenderParticleEmitter
                {
                    ParticleEmitter = emitters[index],
                    RenderParticleSystem = renderParticleSystem,
                };

                renderEmitters[index] = renderEmitter;
                VisibilityGroup.RenderObjects.Add(renderEmitter);
            }

            renderParticleSystem.Emitters = renderEmitters;
        }

        protected override void OnEntityComponentRemoved(Entity entity, ParticleSystemComponent particleSystemComponent, RenderParticleSystem renderParticleSystem)
        {
            foreach (var emitter in renderParticleSystem.Emitters)
            {
                VisibilityGroup.RenderObjects.Remove(emitter);
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