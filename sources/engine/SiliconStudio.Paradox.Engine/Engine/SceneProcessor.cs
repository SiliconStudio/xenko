// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The scene processor to handle a scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a scene component. If an scene component is found, it will
    /// create a sub-<see cref="EntitySystem"/> dedicated to handle the entities inside the scene.
    /// </remarks>
    public sealed class SceneProcessor : EntityProcessor<SceneProcessor.SceneState>
    {
        private readonly Entity sceneEntityRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor"/> class.
        /// </summary>
        public SceneProcessor() : base(new []{ SceneComponent.Key })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor"/> class.
        /// </summary>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <exception cref="System.ArgumentNullException">sceneEntityRoot</exception>
        public SceneProcessor(Entity sceneEntityRoot)
            : this()
        {
            if (sceneEntityRoot == null) throw new ArgumentNullException("sceneEntityRoot");
            this.sceneEntityRoot = sceneEntityRoot;
        }

        protected override SceneState GenerateAssociatedData(Entity entity)
        {
            return new SceneState(this.EntitySystem.Services, entity);
        }

        internal override bool ShouldStopProcessorChain(Entity entity)
        {
            // If the entity being added is not the scene entity root, don't run other processors, as this is handled 
            // by a nested EntitySystem
            return !ReferenceEquals(entity, sceneEntityRoot);
        }

        public class SceneState
        {
            public SceneState(IServiceRegistry services, Entity sceneEntityRoot)
            {
                if (services == null) throw new ArgumentNullException("services");

                // When a scene root is used for an entity system, 
                EntitySystem = new EntitySystem(services) { AutoRegisterDefaultProcessors = true };
                EntitySystem.Processors.Add(new SceneProcessor(sceneEntityRoot));
                EntitySystem.Add(sceneEntityRoot);
            }

            /// <summary>
            /// Entity System dedicated to this scene.
            /// </summary>
            public EntitySystem EntitySystem { get; private set; }
        }
    }
}