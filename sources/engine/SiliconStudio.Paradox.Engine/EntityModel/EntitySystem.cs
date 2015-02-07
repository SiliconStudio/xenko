// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.ReferenceCounting;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// Manage a collection of entities.
    /// </summary>
    public class EntitySystem : GameSystemBase, IReadOnlySet<Entity>
    {
        // TODO: Make this class threadsafe (current locks aren't sufficients)

        // List of all entities, with their respective processors
        private readonly TrackingDictionary<Entity, List<EntityProcessor>> entities;

        // Enabled entities
        private readonly TrackingHashSet<Entity> enabledEntities;

        private readonly TrackingCollection<EntityProcessor> processors;

        private readonly HashSet<Type> autoRegisteredComponentTypes;
        private readonly HashSet<Type> autoRegisteredProcessorTypes;

        private bool isAutoRegisteringProcessors;

        public EntitySystem(IServiceRegistry registry)
            : base(registry)
        {
            Services.AddService(typeof(EntitySystem), this);
            Enabled = true;
            Visible = true;

            entities = new TrackingDictionary<Entity, List<EntityProcessor>>();
            enabledEntities = new TrackingHashSet<Entity>();

            processors = new TrackingCollection<EntityProcessor>();
            processors.CollectionChanged += new EventHandler<TrackingCollectionChangedEventArgs>(systems_CollectionChanged);

            autoRegisteredComponentTypes = new HashSet<Type>();
            autoRegisteredProcessorTypes = new HashSet<Type>();
            newAutoRegisteredProcessors = new List<EntityProcessor>();
        }

        /// <summary>
        /// Gets the entity Processors.
        /// </summary>
        public FastCollection<EntityProcessor> Processors
        {
            get { return processors; }
        }

        internal bool AutoRegisterDefaultProcessors { get; set; }

        public override void Update(GameTime gameTime)
        {
            foreach (var processor in processors)
            {
                if (processor.Enabled)
                {
                    using (Profiler.Begin(processor.UpdateProfilingKey))
                    {
                        processor.Update(gameTime);
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var processor in processors)
            {
                if (processor.Enabled)
                {
                    using (Profiler.Begin(processor.DrawProfilingKey))
                    {
                        processor.Draw(gameTime);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the entity.
        /// If the <see cref="Entity" /> has a parent, its parent should be added (or <see cref="TransformationComponent.Children" />) should be used.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <exception cref="System.ArgumentException">Entity shouldn't have a parent.;entity</exception>
        public void Add(Entity entity)
        {
            // Entity can't be a root because it already has a parent?
            if (entity.Transform != null && entity.Transform.Parent != null)
                throw new ArgumentException("Entity shouldn't have a parent.", "entity");

            InternalAddEntity(entity);
        }

        /// <summary>
        /// Adds a collection of entities to this system.
        /// </summary>
        /// <param name="entitiesToAdd">The entities to add.</param>
        public void Add(params Entity[] entitiesToAdd)
        {
            foreach (var entity in entitiesToAdd)
            {
                Add(entity);
            }
        }

        /// <summary>
        /// Adds a collection of entities to this system.
        /// </summary>
        /// <param name="entitiesToAdd">The entities to add.</param>
        public void AddRange(IEnumerable<Entity> entitiesToAdd)
        {
            foreach (var entity in entitiesToAdd)
            {
                Add(entity);
            }
        }

        /// <summary>
        /// Sets the enable state of this entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="enabled">if set to <c>true</c>, entity is [enabled].</param>
        /// <exception cref="System.InvalidOperationException">Entity is not part of this EntityManager.</exception>
        public void SetEnabled(Entity entity, bool enabled = true)
        {
            List<EntityProcessor> entityProcessors;
            if (!entities.TryGetValue(entity, out entityProcessors))
                throw new InvalidOperationException("Entity is not part of this EntityManager.");

            bool wasEnabled = enabledEntities.Contains(entity);

            if (enabled != wasEnabled)
            {
                if (enabled)
                {
                    enabledEntities.Add(entity);
                }
                else
                {
                    enabledEntities.Remove(entity);
                }

                foreach (var component in entityProcessors)
                {
                    component.SetEnabled(entity, enabled);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified entity is enabled.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the specified entity is enabled; otherwise, <c>false</c>.</returns>
        /// <inheritdoc />
        public bool IsEnabled(Entity entity)
        {
            return enabledEntities.Contains(entity);
        }

        /// <summary>
        /// Enables the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <inheritdoc />
        public void Enable(Entity entity)
        {
            SetEnabled(entity, true);
        }

        /// <summary>
        /// Disables the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <inheritdoc />
        public void Disable(Entity entity)
        {
            SetEnabled(entity, false);
        }

        /// <summary>
        /// Removes the entity from the <see cref="EntitySystem" />.
        /// It works weither entity has a parent or not.
        /// In conjonction with <see cref="HierarchicalSystem" />, it will remove children entities as well.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Remove(Entity entity)
        {
            InternalRemoveEntity(entity, true);
        }

        /// <summary>
        /// Removes all entities from the <see cref="EntitySystem"/>.
        /// </summary>
        public void Clear()
        {
            foreach (var entity in entities.Keys.ToList())
            {
                InternalRemoveEntity(entity, true);
            }
        }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T GetProcessor<T>() where T : EntityProcessor
        {
            foreach (var system in processors)
            {
                if (system is T)
                    return (T)system;
            }

            return null;
        }
        
        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        internal void InternalAddEntity(Entity entity)
        {
            // Already added?
            if (entities.ContainsKey(entity))
                return;

            var entityProcessors = new List<EntityProcessor>();
            entities.Add(entity, entityProcessors);

            enabledEntities.Add(entity);

            entity.AddReferenceInternal();

            entity.Components.PropertyUpdated += EntityPropertyUpdated;

            // Check which processor want this entity
            CheckEntityWithProcessors(entity, entityProcessors, false);

            // Auto-register default processors
            if (AutoRegisterDefaultProcessors)
            {
                AutoRegisterProcessors(entity, entityProcessors);
            }
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <param name="removeParent">Indicate if entity should be removed from its parent</param>
        internal void InternalRemoveEntity(Entity entity, bool removeParent)
        {
            // Entity wasn't already added
            List<EntityProcessor> entityProcessors;
            if (!entities.TryGetValue(entity, out entityProcessors))
                return;

            entities.Remove(entity);
            enabledEntities.Remove(entity);

            if (removeParent)
            {
                // Force parent to be null, so that it is removed even if it is not a root node
                entity.Transform.Parent = null;
            }

            // Notify Processors thie entity has been removed
            CheckEntityWithProcessors(entity, entityProcessors, true);

            entity.Components.PropertyUpdated -= EntityPropertyUpdated;

            entity.ReleaseInternal();
        }

        private readonly List<EntityProcessor> newAutoRegisteredProcessors;

        private void AutoRegisterProcessors(Entity entity, List<EntityProcessor> entityProcessors)
        {
            newAutoRegisteredProcessors.Clear();

            // TODO: Access to Components use a yield behind. Change this
            foreach (var componentKeyPair in entity.Components)
            {
                var component = componentKeyPair.Value as EntityComponent;
                if (component != null)
                {
                    RegisterComponentType(component, component.GetType());
                }
            }

            isAutoRegisteringProcessors = true;
            foreach (var processor in newAutoRegisteredProcessors)
            {
                processors.Add(processor);
                processor.EntityCheck(entity, entityProcessors);
            }
            isAutoRegisteringProcessors = false;

            newAutoRegisteredProcessors.Clear();
        }

        private void RegisterComponentType(EntityComponent entityComponent, Type componentType)
        {
            if (componentType == null) throw new ArgumentNullException("componentType");

            if (autoRegisteredComponentTypes.Contains(componentType))
            {
                return;
            }

            autoRegisteredComponentTypes.Add(componentType);

            if (entityComponent == null)
            {
                entityComponent = (EntityComponent)Activator.CreateInstance(componentType);
            }

            foreach (var processorType in entityComponent.GetDefaultProcessors())
            {
                // TODO: Log an error?
                if (!typeof(EntityProcessor).IsAssignableFrom(processorType))
                {
                    continue;
                }

                if (!autoRegisteredProcessorTypes.Contains(processorType))
                {
                    var processor = (EntityProcessor)Activator.CreateInstance(processorType);

                    foreach (var key in processor.RequiredKeys)
                    {
                        RegisterComponentType(null, key.OwnerType);
                    }

                    if (!autoRegisteredProcessorTypes.Contains(processorType))
                    {
                        InitializeProcessor(processor);
                        newAutoRegisteredProcessors.Add(processor);
                    }
                }
            }
        }

        private void InitializeProcessor(EntityProcessor processor)
        {
            processor.EntitySystem = this;
            processor.Services = Services;
            processor.OnSystemAdd();
        }

        private void AddSystem(EntityProcessor processor)
        {
            InitializeProcessor(processor);

            // In case of auto-register, we don't have to iterate on existing entities as it is only valid for the entity being added
            if (!isAutoRegisteringProcessors)
            {
                foreach (var entity in entities)
                {
                    processor.EntityCheck(entity.Key, entity.Value);
                }
            }
        }

        private void RemoveSystem(EntityProcessor processor)
        {
            processor.OnSystemRemove();
            processor.Services = null;
            processor.EntitySystem = null;
        }

        private void EntityPropertyUpdated(ref PropertyContainer propertyContainer, PropertyKey propertyKey, object newValue, object oldValue)
        {
            // Only process EntityComponent properties
            if (!typeof(EntityComponent).GetTypeInfo().IsAssignableFrom(propertyKey.PropertyType.GetTypeInfo()))
                return;

            // No real update   
            if (oldValue == newValue)
                return;

            var entity = (Entity)propertyContainer.Owner;
            var entityProcessors = entities[entity];
            CheckEntityWithProcessors(entity, entityProcessors, false);
        }

        private void CheckEntityWithProcessors(Entity entity, List<EntityProcessor> entityProcessors, bool forceRemove)
        {
            foreach (EntityProcessor system in processors)
            {
                if (system.ShouldStopProcessorChain(entity))
                {
                    return;
                }
                system.EntityCheck(entity, entityProcessors, forceRemove);
            }
        }

        //private void entities_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        //{
        //    var entity = (Entity)e.Item;
        //    switch (e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Add:
        //            InternalAddEntity(entity);
        //            break;
        //        case NotifyCollectionChangedAction.Remove:
        //            InternalRemoveEntity(entity);
        //            break;
        //    }
        //}

        private void systems_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddSystem((EntityProcessor)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveSystem((EntityProcessor)e.Item);
                    break;
            }
        }

        /// <summary>
        /// Determines whether this instance contains the specified entity.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance contains the specified entity; otherwise, <c>false</c>.</returns>
        public bool Contains(Entity item)
        {
            return entities.ContainsKey(item);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return entities.Count;
            }
        }
    }
}