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
    /// This processor will take care of adding/removing children of every Entity added/removed in the EntitySystem.
    /// It will also exposes a list of root entities.
    /// </summary>
    public class HierarchicalProcessor : EntityProcessor<TransformationComponent>
    {
        private readonly TrackingHashSet<Entity> rootEntities;

        public HierarchicalProcessor()
            : base(new[] { TransformationComponent.Key })
        {
            rootEntities = new TrackingHashSet<Entity>();
            rootEntities.CollectionChanged += rootEntities_CollectionChanged;
        }

        /// <summary>
        /// Gets the list of root entities (entities which have no <see cref="TransformationComponent.Parent"/>).
        /// </summary>
        public ISet<Entity> RootEntities
        {
            get { return rootEntities; }
        }

        /// <inheritdoc/>
        protected override TransformationComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Transformation;
        }

        /// <inheritdoc/>
        protected internal override void OnSystemAdd()
        {
            
        }

        protected override void OnEnabledChanged(Entity entity, bool enabled)
        {
            foreach (var child in entity.Transformation.Children)
            {
                EntitySystem.SetEnabled(child.Entity, enabled);
            }
        }

        /// <inheritdoc/>
        protected override void OnEntityAdding(Entity entity, TransformationComponent transformationComponent)
        {
            foreach (var child in transformationComponent.Children)
            {
                InternalAddEntity(child.Entity);
            }

            if (transformationComponent.Parent == null)
                rootEntities.Add(entity);

            ((TrackingCollection<TransformationComponent>)transformationComponent.Children).CollectionChanged += Children_CollectionChanged;
        }

        /// <inheritdoc/>
        protected override void OnEntityRemoved(Entity entity, TransformationComponent transformationComponent)
        {
            var entityToRemove = new List<Entity>();
            foreach (var child in transformationComponent.Children)
            {
                entityToRemove.Add(child.Entity);
            }

            foreach (var childEntity in entityToRemove)
            {
                InternalRemoveEntity(childEntity, false);
            }

            // If sub entity is removed but its parent is still there, it needs to be detached.
            // Note that this behavior is still not totally fixed yet, it might change.
            if (transformationComponent.Parent != null && EntitySystem.Contains(transformationComponent.Parent.Entity))
                transformationComponent.Parent = null;

            rootEntities.Remove(entity);

            ((TrackingCollection<TransformationComponent>)transformationComponent.Children).CollectionChanged -= Children_CollectionChanged;
        }

        void rootEntities_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    EntitySystem.Add((Entity)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    EntitySystem.Remove((Entity)e.Item);
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
                    InternalAddEntity(((TransformationComponent)e.Item).Entity);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // If a child is removed, it is still kept in Entities.
                    // Entities.Remove(child) should be used to remove entities (this will detach child from its parent)
                    //InternalRemoveEntity(((TransformationComponent)e.Item).Entity);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}