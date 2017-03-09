using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation
{
    internal class StaticColliderProcessor : EntityProcessor<StaticColliderComponent, StaticColliderData>
    {
        public delegate void CollectionChangedEventHandler(StaticColliderComponent component, StaticColliderData data);

        public event CollectionChangedEventHandler ColliderAdded;
        public event CollectionChangedEventHandler ColliderRemoved;

        protected override StaticColliderData GenerateComponentData(Entity entity, StaticColliderComponent component)
        {
            return new StaticColliderData { Component = component };
        }

        protected override void OnEntityComponentAdding(Entity entity, StaticColliderComponent component, StaticColliderData data)
        {
            base.OnEntityComponentAdding(entity, component, data);
            ColliderAdded?.Invoke(component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, StaticColliderComponent component, StaticColliderData data)
        {
            ColliderRemoved?.Invoke(component, data);
            base.OnEntityComponentRemoved(entity, component, data);
        }
    }
}