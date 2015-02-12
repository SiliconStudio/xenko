// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The scene child processor to handle a child scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a <see cref="SceneChildComponent"/>. If an scene component is found, it will
    /// create a sub-<see cref="EntityManager"/> dedicated to handle the entities inside the child scene.
    /// </remarks>
    public sealed class SceneChildProcessor : SceneProcessorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildProcessor"/> class.
        /// </summary>
        public SceneChildProcessor()
            : base(SceneComponent.Key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildProcessor"/> class.
        /// </summary>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <exception cref="System.ArgumentNullException">sceneEntityRoot</exception>
        public SceneChildProcessor(Scene sceneEntityRoot)
            : base(sceneEntityRoot, SceneChildComponent.Key)
        {
        }
    }
}