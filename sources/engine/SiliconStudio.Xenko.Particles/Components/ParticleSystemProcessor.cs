// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Components
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

        protected override void OnSystemAdd()
        {
        }

        public List<ParticleSystemComponentState> ParticleSystems { get; private set; }

        internal void UpdateParticleSystem(ParticleSystemComponentState state)
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

            particleSystem.Update(0.016f * speed);
        }

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
                    // Exposed variables
                   
                    if (!particleSystemStateKeyPair.Value.ParticleSystemComponent.ParticleSystem.Enabled)
                        continue;

                    ParticleSystems.Add(particleSystemStateKeyPair.Value);
                }
            }

            if (true || ParticleSystems.Count > 8)
            {
                TaskList.Dispatch(
                    ParticleSystems,
                    8,
                    8,
                    (i, particleSystemComponentState) =>
                    {
                        UpdateParticleSystem(particleSystemComponentState);
                    }
                    );
            }
            else
            {
                foreach (var particleSystemComponentState in ParticleSystems)
                {
                    UpdateParticleSystem(particleSystemComponentState);
                }
            }


            /*
                    internal static void UpdateTransformations(FastCollection<TransformComponent> transformationComponents)
        {
            // To avoid GC pressure (due to lambda), parallelize only if required
            if (transformationComponents.Count >= 1024)
            {
                TaskList.Dispatch(
                    transformationComponents,
                    8,
                    1024,
                    (i, transformation) =>
                        {
                            UpdateTransformation(transformation);

                            // Recurse
                            if (transformation.Children.Count > 0)
                                UpdateTransformations(transformation.Children);
                        }
                    );
            }
            else
            {
                foreach (var transformation in transformationComponents)
                {
                    UpdateTransformation(transformation);

                    // Recurse
                    if (transformation.Children.Count > 0)
                        UpdateTransformations(transformation.Children);
                }
            }
        }
            */
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
