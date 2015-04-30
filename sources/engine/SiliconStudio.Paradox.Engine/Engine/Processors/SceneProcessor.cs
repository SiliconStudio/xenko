// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// The scene processor to handle a scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a scene component. If an scene component is found, it will
    /// create a sub-<see cref="EntityManager"/> dedicated to handle the entities inside the scene.
    /// </remarks>
    internal sealed class SceneProcessor : EntityProcessor<SceneInstance>
    {
        private readonly SceneInstance sceneInstance;
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor"/> class.
        /// </summary>
        public SceneProcessor() : base(SceneComponent.Key)
        {
            Order = -10000;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor" /> class.
        /// </summary>
        /// <param name="sceneInstance">The scene instance.</param>
        /// <exception cref="System.ArgumentNullException">sceneEntityRoot</exception>
        public SceneProcessor(SceneInstance sceneInstance) : this()
        {
            if (sceneInstance == null) throw new ArgumentNullException("sceneInstance");
            this.sceneInstance = sceneInstance;
        }

        public SceneInstance Current
        {
            get
            {
                return sceneInstance;
            }
        }

        protected override SceneInstance GenerateAssociatedData(Entity entity)
        {
            if (entity != sceneInstance.Scene)
            {
                throw new InvalidOperationException("Cannot nest a Scene inside another scene");
            }

            return sceneInstance;
        }
    }
}