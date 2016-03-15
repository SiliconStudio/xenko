// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Particles.Components
{
    class ParticleSystemSimulationProcessor : EntityProcessor<ParticleSystemComponent, ParticleSystemSimulationProcessor.ParticleSystemComponentState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleSystemSimulationProcessor"/> class.
        /// </summary>
        public ParticleSystemSimulationProcessor()
            : base(typeof(TransformComponent))  //  Only list the additional required components
        {
            ParticleSystems = new List<ParticleSystemComponentState>();
        }

        /// <inheritdoc/>
        protected override void OnSystemAdd()
        {
            // Create or reference systems
            //var game = Services.GetSafeServiceAs<IGame>();
            //game.GameSystems.Add(particleEngine);
            //var graphicsDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>()?.GraphicsDevice;
            //var sceneSystem = Services.GetSafeServiceAs<SceneSystem>();
        }

        /// <inheritdoc />
        protected override void OnSystemRemove()
        {
            // Clean-up particleEngine
        }

        /// <inheritdoc />
        protected override void OnEntityComponentAdding(Entity entity, ParticleSystemComponent component, ParticleSystemComponentState data)
        {
            // Do some particle system initialization
        }

        /// <inheritdoc />
        protected override void OnEntityComponentRemoved(Entity entity, ParticleSystemComponent component, ParticleSystemComponentState data)
        {
            // TODO: Reset low-level data only. This method also gets called when moving the entity in the hierarchy!
            // component.ParticleSystem.Dispose();
        }

        public List<ParticleSystemComponentState> ParticleSystems { get; private set; }

        internal void UpdateParticleSystem(ParticleSystemComponentState state, float deltaTime)
        {
            var speed = state.ParticleSystemComponent.Speed;
            var transformComponent = state.TransformComponent;
            var particleSystem = state.ParticleSystemComponent.ParticleSystem;

            // We must update the TRS location of the particle system prior to updating the system itself.
            // Particles only handle uniform scale.

            if (transformComponent.Parent == null)
            {
                // The transform doesn't have a parent. Local transform IS world transform

                particleSystem.Translation = transformComponent.Position;   // This is the local position!

                particleSystem.UniformScale = transformComponent.Scale.X;   // This is the local scale!

                particleSystem.Rotation = transformComponent.Rotation;      // This is the local rotation!
            }
            else
            {
                // TODO: Check transformComponent.WorldMatrix.Decompose(out scale, out rot, out pos);

                // The transform has a parent - do not use local transform values, instead check the world matrix

                // Position
                particleSystem.Translation = new Vector3(transformComponent.WorldMatrix.M41, transformComponent.WorldMatrix.M42, transformComponent.WorldMatrix.M43);

                // Scale
                var uniformScaleX = new Vector3(transformComponent.WorldMatrix.M11, transformComponent.WorldMatrix.M12, transformComponent.WorldMatrix.M13);
                particleSystem.UniformScale = uniformScaleX.Length();

                // Rotation
                // TODO Maybe implement Quaternion.RotationMatrix which also handles cases when the matrix has scaling?
                var invScl = (particleSystem.UniformScale > 0) ? 1f / particleSystem.UniformScale : 1f;
                var rotMatrix = new Matrix(
                    transformComponent.WorldMatrix.M11 * invScl, transformComponent.WorldMatrix.M12 * invScl, transformComponent.WorldMatrix.M13 * invScl, 0,
                    transformComponent.WorldMatrix.M21 * invScl, transformComponent.WorldMatrix.M22 * invScl, transformComponent.WorldMatrix.M23 * invScl, 0,
                    transformComponent.WorldMatrix.M31 * invScl, transformComponent.WorldMatrix.M32 * invScl, transformComponent.WorldMatrix.M33 * invScl, 0,
                    0, 0, 0, 1);
                Quaternion.RotationMatrix(ref rotMatrix, out particleSystem.Rotation);
            }

            particleSystem.Update(deltaTime * speed);
        }

        /// <inheritdoc />
        public override void Update(GameTime time)
        {
            base.Update(time);

            float deltaTime = (float) time.Elapsed.TotalSeconds;

            ParticleSystems.Clear();
            foreach (var particleSystemStateKeyPair in ComponentDatas)
            {
                if (particleSystemStateKeyPair.Value.ParticleSystemComponent.Enabled)
                {
                    // Exposed variables
                   
                    if (!particleSystemStateKeyPair.Value.ParticleSystemComponent.ParticleSystem.Enabled)
                        continue;

                    ParticleSystems.Add(particleSystemStateKeyPair.Value);
                }
            }

            if (ParticleSystems.Count > 8)
            {
                TaskList.Dispatch(
                    ParticleSystems,
                    8,
                    8,
                    (i, particleSystemComponentState) =>
                    {
                        UpdateParticleSystem(particleSystemComponentState, deltaTime);
                    }
                    );
            }
            else
            {
                foreach (var particleSystemComponentState in ParticleSystems)
                {
                    UpdateParticleSystem(particleSystemComponentState, deltaTime);
                }
            }
        }

        /// <inheritdoc />
        protected override ParticleSystemComponentState GenerateComponentData(Entity entity, ParticleSystemComponent component)
        {
            return new ParticleSystemComponentState
            {
                ParticleSystemComponent = component,
                TransformComponent = entity.Transform,
            };
        }

        /// <inheritdoc />
        protected override bool IsAssociatedDataValid(Entity entity, ParticleSystemComponent component, ParticleSystemComponentState associatedData)
        {
            return
                component == associatedData.ParticleSystemComponent &&
                entity.Transform == associatedData.TransformComponent;
        }

        /// <summary>
        /// Base component state for this processor. Every particle system requires a locator, so the <see cref="TransformComponent"/> is mandatory
        /// </summary>
        public class ParticleSystemComponentState
        {
            public ParticleSystemComponent ParticleSystemComponent;

            public TransformComponent TransformComponent;
        }
    }
}
