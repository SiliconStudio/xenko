// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// This processor will take care of adding/removing children of every Entity added/removed in the SceneInstance.
    /// It will also exposes a list of root entities.
    /// </summary>
    public class HierarchicalProcessor : EntityProcessor<TransformComponent>
    {
        private readonly TrackingHashSet<Entity> rootEntities;

        public HierarchicalProcessor()
        {
            rootEntities = new TrackingHashSet<Entity>();
            rootEntities.CollectionChanged += rootEntities_CollectionChanged;
            Order = -1000;
        }

        /// <summary>
        /// Gets the list of root entities (entities which have no <see cref="TransformComponent.Parent"/>).
        /// </summary>
        public ISet<Entity> RootEntities => rootEntities;

        /// <inheritdoc/>
        protected override TransformComponent GenerateComponentData(Entity entity, TransformComponent component)
        {
            return component;
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentAdding(Entity entity, TransformComponent component, TransformComponent data)
        {
            foreach (var child in data.Children)
            {
                InternalAddEntity(child.Entity);
            }

            if (data.Parent == null)
                rootEntities.Add(entity);

            ((TrackingCollection<TransformComponent>)data.Children).CollectionChanged += Children_CollectionChanged;
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentRemoved(Entity entity, TransformComponent component, TransformComponent data)
        {
            var entityToRemove = new List<Entity>();
            foreach (var child in data.Children)
            {
                entityToRemove.Add(child.Entity);
            }

            foreach (var childEntity in entityToRemove)
            {
                InternalRemoveEntity(childEntity, false);
            }

            rootEntities.Remove(entity);

            ((TrackingCollection<TransformComponent>)data.Children).CollectionChanged -= Children_CollectionChanged;
        }

        void rootEntities_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    EntityManager.Add((Entity)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    EntityManager.Remove((Entity)e.Item);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void Children_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            // Added/removed children of entities in the entity manager have to be added/removed of the entity manager.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InternalAddEntity(((TransformComponent)e.Item).Entity);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    InternalRemoveEntity(((TransformComponent)e.Item).Entity, false);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}