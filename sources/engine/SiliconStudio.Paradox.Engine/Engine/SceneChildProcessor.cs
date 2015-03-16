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
    public sealed class SceneChildProcessor : EntityProcessor<SceneChildComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildProcessor"/> class.
        /// </summary>
        public SceneChildProcessor()
            : base(SceneChildComponent.Key)
        {
        }

        public SceneInstance GetSceneInstance(SceneChildComponent component)
        {
            return component.SceneInstance;
        }

        protected override SceneChildComponent GenerateAssociatedData(Entity entity)
        {
            var sceneChild = entity.Get<SceneChildComponent>();
            sceneChild.SceneInstance = new SceneInstance(EntityManager.Services, sceneChild.Scene, EntityManager.GetProcessor<ScriptProcessor>() != null);
            return sceneChild;
        }

        protected override void OnEntityAdding(Entity entity, SceneChildComponent component)
        {

        }

        protected override void OnEntityRemoved(Entity entity, SceneChildComponent component)
        {
            if (component != null)
            {
                component.SceneInstance.Dispose();
                component.SceneInstance = null;
            }
        }

        public override void Update(GameTime time)
        {
            foreach (var entity in enabledEntities)
            {
                var childComponent = entity.Value;

                // Copy back the scene from the component to the instance
                childComponent.SceneInstance.Scene = childComponent.Scene;
                if (childComponent.Enabled)
                {
                    childComponent.SceneInstance.Update(time);
                }
            }
        }

        public override void Draw(RenderContext context)
        {
            foreach (var entity in enabledEntities)
            {
                var childComponent = entity.Value;

                // Copy back the scene from the component to the instance
                childComponent.SceneInstance.Scene = childComponent.Scene;
                if (childComponent.Enabled)
                {
                    childComponent.SceneInstance.Draw(context);
                }
            }
        }
    }
}