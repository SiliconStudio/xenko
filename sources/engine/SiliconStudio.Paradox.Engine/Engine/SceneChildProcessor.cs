// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The scene child processor to handle a child scene. See remarks.
    /// </summary>
    /// <remarks>
    /// This processor is handling specially an entity with a <see cref="ChildSceneComponent"/>. If an scene component is found, it will
    /// create a sub-<see cref="EntityManager"/> dedicated to handle the entities inside the child scene.
    /// </remarks>
    public sealed class SceneChildProcessor : EntityProcessor<ChildSceneComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildProcessor"/> class.
        /// </summary>
        public SceneChildProcessor()
            : base(ChildSceneComponent.Key)
        {
        }

        public SceneInstance GetSceneInstance(ChildSceneComponent component)
        {
            return component.SceneInstance;
        }

        protected override ChildSceneComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get<ChildSceneComponent>();
        }

        protected override void OnEntityAdding(Entity entity, ChildSceneComponent component)
        {
            component.SceneInstance = new SceneInstance(EntityManager.Services, component.Scene, EntityManager.GetProcessor<ScriptProcessor>() != null);
        }

        protected override void OnEntityRemoved(Entity entity, ChildSceneComponent component)
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
                if (childComponent.Enabled)
                {
                    // Copy back the scene from the component to the instance
                    childComponent.SceneInstance.Scene = childComponent.Scene;
                    childComponent.SceneInstance.Update(time);
                }
            }
        }

        public override void Draw(RenderContext context)
        {
            foreach (var entity in enabledEntities)
            {
                var childComponent = entity.Value;
                if (childComponent.Enabled)
                {
                    var sceneInstance = childComponent.SceneInstance;
                    if (sceneInstance.Scene != childComponent.Scene)
                        throw new InvalidOperationException("The scene instance does not match the scene of the ChildSceneComponent. Has it been modified after Update?");
                    
                    childComponent.SceneInstance.Draw(context);
                }
            }
        }
    }
}