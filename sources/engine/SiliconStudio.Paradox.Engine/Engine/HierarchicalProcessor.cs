// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// This processor will take care of adding/removing children of every Entity added/removed in the EntityManager.
    /// It will also exposes a list of root entities.
    /// </summary>
    public class HierarchicalProcessor : EntityProcessor<TransformComponent>
    {
        private readonly TrackingHashSet<Entity> rootEntities;

        public HierarchicalProcessor()
            : base(new[] { TransformComponent.Key })
        {
            rootEntities = new TrackingHashSet<Entity>();
            rootEntities.CollectionChanged += rootEntities_CollectionChanged;
        }

        /// <summary>
        /// Gets the list of root entities (entities which have no <see cref="TransformComponent.Parent"/>).
        /// </summary>
        public ISet<Entity> RootEntities
        {
            get { return rootEntities; }
        }

        /// <inheritdoc/>
        protected override TransformComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Transform;
        }

        /// <inheritdoc/>
        protected internal override void OnSystemAdd()
        {
            
        }

        protected override void OnEnabledChanged(Entity entity, bool enabled)
        {
            foreach (var child in entity.Transform.Children)
            {
                EntityManager.SetEnabled(child.Entity, enabled);
            }
        }

        /// <inheritdoc/>
        protected override void OnEntityAdding(Entity entity, TransformComponent data)
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
        protected override void OnEntityRemoved(Entity entity, TransformComponent data)
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

            // If sub entity is removed but its parent is still there, it needs to be detached.
            // Note that this behavior is still not totally fixed yet, it might change.
            if (data.Parent != null && EntityManager.Contains(data.Parent.Entity))
                data.Parent = null;

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
                    // If a child is removed, it is still kept in Entities.
                    // Entities.Remove(child) should be used to remove entities (this will detach child from its parent)
                    //InternalRemoveEntity(((TransformComponent)e.Item).Entity);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}