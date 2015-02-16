// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The scene child processor to handle a child scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a <see cref="SceneChildComponent"/>. If an scene component is found, it will
    /// create a sub-<see cref="EntityManager"/> dedicated to handle the entities inside the child scene.
    /// </remarks>
    public sealed class SceneChildProcessor : EntityProcessor<SceneInstance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildProcessor"/> class.
        /// </summary>
        public SceneChildProcessor()
            : base(SceneChildComponent.Key)
        {
            Scenes = new List<SceneInstance>();
        }

        public List<SceneInstance> Scenes { get; private set; }

        protected override SceneInstance GenerateAssociatedData(Entity entity)
        {
            var sceneChild = entity.Get<SceneChildComponent>();
            return new SceneInstance(EntityManager.Services, sceneChild, sceneChild.Scene);
        }

        protected override void OnEntityAdding(Entity entity, SceneInstance data)
        {
            if (data != null)
            {
                Scenes.Add(data);
            }
        }

        protected override void OnEntityRemoved(Entity entity, SceneInstance data)
        {
            if (data != null)
            {
                data.Dispose();
                Scenes.Remove(data);
            }
        }

        public override void Update(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                sceneEntityAndState.Update(time);
            }
        }

        public override void Draw(GameTime time)
        {
            // Call on the scene Draw is performed by SceneInstance.Draw
        }
    }
}