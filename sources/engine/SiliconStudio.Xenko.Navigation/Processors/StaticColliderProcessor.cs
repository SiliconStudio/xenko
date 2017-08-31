// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation.Processors
{
    internal class StaticColliderProcessor : EntityProcessor<StaticColliderComponent, StaticColliderData>
    {
        public delegate void CollectionChangedEventHandler(StaticColliderComponent component, StaticColliderData data);

        public event CollectionChangedEventHandler ColliderAdded;
        public event CollectionChangedEventHandler ColliderRemoved;

        /// <inheritdoc />
        protected override StaticColliderData GenerateComponentData(Entity entity, StaticColliderComponent component)
        {
            return new StaticColliderData { Component = component };
        }

        /// <inheritdoc />
        protected override bool IsAssociatedDataValid(Entity entity, StaticColliderComponent component, StaticColliderData associatedData)
        {
            return component == associatedData.Component;
        }

        /// <inheritdoc />
        protected override void OnEntityComponentAdding(Entity entity, StaticColliderComponent component, StaticColliderData data)
        {
            ColliderAdded?.Invoke(component, data);
        }

        /// <inheritdoc />
        protected override void OnEntityComponentRemoved(Entity entity, StaticColliderComponent component, StaticColliderData data)
        {
            ColliderRemoved?.Invoke(component, data);
        }
    }
}
