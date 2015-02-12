// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The scene processor to handle a scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a scene component. If an scene component is found, it will
    /// create a sub-<see cref="EntityManager"/> dedicated to handle the entities inside the scene.
    /// </remarks>
    public sealed class SceneProcessor : SceneProcessorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor"/> class.
        /// </summary>
        public SceneProcessor() : base(SceneComponent.Key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessor"/> class.
        /// </summary>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <exception cref="System.ArgumentNullException">sceneEntityRoot</exception>
        public SceneProcessor(Scene sceneEntityRoot) : base(sceneEntityRoot, SceneComponent.Key)
        {
        }
    }
}