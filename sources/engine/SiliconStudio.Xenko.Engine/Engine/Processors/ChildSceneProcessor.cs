// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// The scene child processor to handle a child scene. See remarks.
    /// </summary>
    public sealed class ChildSceneProcessor : EntityProcessor<ChildSceneComponent>
    {
        protected override ChildSceneComponent GenerateComponentData(Entity entity, ChildSceneComponent component)
        {
            return component;
        }

        protected override void OnEntityComponentAdding(Entity entity, ChildSceneComponent component, ChildSceneComponent data)
        {
            component.UpdateScene();
        }

        protected override void OnEntityComponentRemoved(Entity entity, ChildSceneComponent component, ChildSceneComponent data)
        {
            component.UpdateScene();
        }
    }
}