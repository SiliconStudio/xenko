// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Paradox.Effects;
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
            Scenes = new Dictionary<SceneChildComponent, SceneInstance>();
        }

        public Dictionary<SceneChildComponent, SceneInstance> Scenes { get; private set; }

        protected override SceneInstance GenerateAssociatedData(Entity entity)
        {
            var sceneChild = entity.Get<SceneChildComponent>();
            return new SceneInstance(EntityManager.Services, sceneChild.Scene);
        }

        protected override void OnEntityAdding(Entity entity, SceneInstance data)
        {
            var childComponent = entity.Get<SceneChildComponent>();

            if (data != null)
            {
                Scenes[childComponent] = data;
            }
        }

        protected override void OnEntityRemoved(Entity entity, SceneInstance data)
        {
            var childComponent = entity.Get<SceneChildComponent>();
            if (data != null)
            {
                data.Dispose();
                Scenes.Remove(childComponent);
            }
        }

        public override void Update(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                var childComponent = sceneEntityAndState.Key;
                var sceneInstance = sceneEntityAndState.Value;

                // Copy back the scene from the component to the instance
                sceneInstance.Scene = childComponent.Scene;
                if (childComponent.Enabled)
                {
                    sceneInstance.Update(time);
                }
            }
        }

        public override void Draw(RenderContext context)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                var childComponent = sceneEntityAndState.Key;
                var sceneInstance = sceneEntityAndState.Value;

                // Copy back the scene from the component to the instance
                sceneInstance.Scene = childComponent.Scene;
                if (childComponent.Enabled)
                {
                    sceneInstance.Draw(context);
                }
            }
        }
    }
}