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
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Manage a collection of entities.
    /// </summary>
    public abstract class EntityManager : ComponentBase, IReadOnlySet<Entity>
    {
        // TODO: Make this class threadsafe (current locks aren't sufficients)

        public ExecutionMode ExecutionMode { get; protected set; } = ExecutionMode.Runtime;

        // List of all entities, with their respective processors
        private readonly TrackingDictionary<Entity, List<EntityProcessor>> entities;

        // Enabled entities
        private readonly TrackingHashSet<Entity> enabledEntities;

        private readonly FastCollection<EntityProcessor> processors;

        private readonly List<EntityProcessor> newProcessors;

        private readonly HashSet<Type> componentTypes;
        private readonly HashSet<Type> processorTypes;

        /// <summary>
        /// Occurs when an entity is added.
        /// </summary>
        public event EventHandler<Entity> EntityAdded;

        /// <summary>
        /// Occurs when an entity is removed.
        /// </summary>
        public event EventHandler<Entity> EntityRemoved;

        /// <summary>
        /// Occurs when a new component type is added.
        /// </summary>
        public event EventHandler<Type> ComponentTypeAdded;

        /// <summary>
        /// Occurs when a component changed for an entity (Added or removed)
        /// </summary>
        public event EventHandler<EntityComponentEventArgs> ComponentChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityManager"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <exception cref="System.ArgumentNullException">registry</exception>
        protected EntityManager(IServiceRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            Services = registry;

            entities = new TrackingDictionary<Entity, List<EntityProcessor>>();
            enabledEntities = new TrackingHashSet<Entity>();

            processors = new FastCollection<EntityProcessor>();
            newProcessors = new List<EntityProcessor>();

            componentTypes = new HashSet<Type>();
            processorTypes = new HashSet<Type>();
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the entity Processors.
        /// </summary>
        public IReadOnlyCollection<EntityProcessor> Processors
        {
            get { return processors; }
        }

        /// <summary>
        /// Gets the list of component types from the entities..
        /// </summary>
        /// <value>The registered component types.</value>
        public IEnumerable<Type> ComponentTypes
        {
            get
            {
                return componentTypes;
            }
        }

        /// <summary>
        /// Adds a processor to this instance.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <exception cref="System.ArgumentNullException">processor</exception>
        public void AddProcessor(EntityProcessor processor)
        {
            if (processor == null) throw new ArgumentNullException("processor");
            if (!processors.Contains(processor))
            {
                processors.Add(processor);
                processors.Sort(EntityProcessorComparer.Default);
                OnProcessorAdded(processor);
            }
        }

        /// <summary>
        /// Removes a processor from this instance.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <exception cref="System.ArgumentNullException">processor</exception>
        public void RemoveProcessor(EntityProcessor processor)
        {
            if (processor == null) throw new ArgumentNullException("processor");
            if (processors.Contains(processor))
            {
                processors.Remove(processor);
                OnProcessorRemoved(processor);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            foreach (var processor in processors)
            {
                if (processor.Enabled)
                {
                    using (Profiler.Begin(processor.UpdateProfilingKey, "Entities: {0}", enabledEntities.Count))
                    {
                        processor.Update(gameTime);
                    }
                }
            }
        }

        internal virtual void Draw(RenderContext context)
        {
            foreach (var processor in processors)
            {
                if (processor.Enabled)
                {
                    using (Profiler.Begin(processor.DrawProfilingKey))
                    {
                        processor.Draw(context);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the entity.
        /// If the <see cref="Entity" /> has a parent, its parent should be added (or <see cref="TransformComponent.Children" />) should be used.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <exception cref="System.ArgumentException">Entity shouldn't have a parent.;entity</exception>
        internal void Add(Entity entity)
        {
            // Entity can't be a root because it already has a parent?
            if (entity.Transform != null && entity.Transform.Parent != null)
                throw new ArgumentException("Entity shouldn't have a parent.", "entity");

            InternalAddEntity(entity);
        }

        /// <summary>
        /// Sets the enable state of this entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="enabled">if set to <c>true</c>, entity is [enabled].</param>
        /// <exception cref="System.InvalidOperationException">Entity is not part of this SceneInstance.</exception>
        public void SetEnabled(Entity entity, bool enabled = true)
        {
            List<EntityProcessor> entityProcessors;
            if (!entities.TryGetValue(entity, out entityProcessors))
                throw new InvalidOperationException("Entity is not part of this SceneInstance.");

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

                foreach (var processor in entityProcessors)
                {
                    processor.SetEnabled(entity, enabled);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified entity is enabled.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the specified entity is enabled; otherwise, <c>false</c>.</returns>
        public bool IsEnabled(Entity entity)
        {
            return enabledEntities.Contains(entity);
        }

        /// <summary>
        /// Enables the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Enable(Entity entity)
        {
            SetEnabled(entity, true);
        }

        /// <summary>
        /// Disables the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Disable(Entity entity)
        {
            SetEnabled(entity, false);
        }

        /// <summary>
        /// Removes the entity from the <see cref="EntityManager" />.
        /// It works weither entity has a parent or not.
        /// In conjonction with <see cref="HierarchicalSystem" />, it will remove children entities as well.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Remove(Entity entity)
        {
            InternalRemoveEntity(entity, true);
        }

        /// <summary>
        /// Removes all entities from the <see cref="EntityManager"/>.
        /// </summary>
        protected internal virtual void Reset()
        {
            // TODO: Not sure this method is correctly implemented
            // TODO: Check that we are correctly removing all indirect collection watchers (in processors...etc.)

            foreach (var entity in entities.Keys.ToList())
            {
                InternalRemoveEntity(entity, true);
            }
            var previousProcessors = processors.ToArray();
            foreach (var processor in previousProcessors)
            {
                RemoveProcessor(processor);
            }

            processorTypes.Clear();
            componentTypes.Clear();

            enabledEntities.Clear();
            entities.Clear();
        }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T GetProcessor<T>() where T : EntityProcessor
        {
            // TODO: Cache in a Dictionary
            for (int i = 0; i < processors.Count; i++)
            {
                var system = processors[i] as T;
                if (system != null)
                    return system;
            }

            return null;
        }

        private int addEntityLevel = 0;

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        internal void InternalAddEntity(Entity entity)
        {
            // Already added?
            if (entities.ContainsKey(entity))
                return;

            if (entity.Manager != null)
            {
                throw new InvalidOperationException("Cannot an an entity to this entity manager when it is already used by another entity manager");
            }

            entity.Manager = this;


            addEntityLevel++;

            var entityProcessors = new List<EntityProcessor>();
            entities.Add(entity, entityProcessors);

            enabledEntities.Add(entity);
            entity.AddReferenceInternal();

            // Check which processor want this entity
            CheckEntityWithProcessors(entity, entityProcessors, false);

            // Grab the list of new processors to registers
            AutoRegisterProcessors(entity);

            addEntityLevel--;

            // Register all new processors
            RegisterNewProcessors();

            OnEntityAdded(entity);
        }

        private void RegisterNewProcessors()
        {
            // Auto-register all new processors
            if (addEntityLevel == 0)
            {
                // Add all new processors
                foreach (var newProcessor in newProcessors)
                {
                    processors.Add(newProcessor);
                }
                // Make sure they are always sorted
                if (newProcessors.Count > 0)
                {
                    processors.Sort(EntityProcessorComparer.Default);
                }

                // Notify
                foreach (var newProcessor in newProcessors)
                {
                    OnProcessorAdded(newProcessor);
                }
                newProcessors.Clear();
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

            // Notify Processors this entity has been removed
            CheckEntityWithProcessors(entity, entityProcessors, true);

            entity.ReleaseInternal();

            entity.Manager = null;

            OnEntityRemoved(entity);
        }

        private void AutoRegisterProcessors(Entity entity)
        {
            foreach (var component in entity.Components)
            {
                RegisterComponentType(component.GetType());
            }
        }

        private void RegisterComponentType(Type componentType)
        {
            if (componentType == null) throw new ArgumentNullException("componentType");

            if (componentTypes.Contains(componentType))
            {
                return;
            }

            componentTypes.Add(componentType);

            OnComponentTypeAdded(componentType);

            // Automatically create processors for the given component type
            RegisterProcessors(componentType);
        }

        private void RegisterProcessors(Type type)
        {
            var processorAttributes = type.GetTypeInfo().GetCustomAttributes<DefaultEntityComponentProcessorAttribute>();
            foreach (var processorAttributeType in processorAttributes)
            {
                var processorType = AssemblyRegistry.GetType(processorAttributeType.TypeName);
                if (processorType == null)
                {
                    continue;
                }

                // Filter using ExecutionMode
                if ((ExecutionMode & processorAttributeType.ExecutionMode) != ExecutionMode.None)
                    RegisterProcessorType(processorType);
            }
        }

        private void RegisterProcessorType(Type processorType)
        {
            // TODO: Log an error?
            if (!typeof(EntityProcessor).GetTypeInfo().IsAssignableFrom(processorType.GetTypeInfo()))
            {
                return;
            }

            if (!processorTypes.Contains(processorType))
            {
                processorTypes.Add(processorType);
                var processor = (EntityProcessor)Activator.CreateInstance(processorType);

                foreach (var type in processor.RequiredTypes)
                {
                    RegisterComponentType(type);
                }

                RegisterProcessors(processorType);

                newProcessors.Add(processor);
            }            
        }

        private void OnProcessorAdded(EntityProcessor processor)
        {
            processorTypes.Add(processor.GetType());

            processor.EntityManager = this;
            processor.Services = Services;
            processor.OnSystemAdd();

            foreach (var entity in entities)
            {
                processor.EntityCheck(entity.Key, entity.Value);
            }
        }

        private void OnProcessorRemoved(EntityProcessor processor)
        {
            processor.OnSystemRemove();
            processor.Services = null;
            processor.EntityManager = null;
        }

        internal void NotifyComponentChanged(Entity entity, int index, EntityComponent newValue, EntityComponent oldValue)
        {
            // No real update   
            if (oldValue == newValue)
                return;

            var entityProcessors = entities[entity];

            AutoRegisterProcessors(entity);
            RegisterNewProcessors();

            CheckEntityWithProcessors(entity, entityProcessors, false);

            // Notify component changes
            OnComponentChanged(new EntityComponentEventArgs(entity, index, oldValue, newValue));
        }

        private void CheckEntityWithProcessors(Entity entity, List<EntityProcessor> entityProcessors, bool forceRemove)
        {
            foreach (EntityProcessor system in processors)
            {
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
                    OnProcessorAdded((EntityProcessor)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnProcessorRemoved((EntityProcessor)e.Item);
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

        protected virtual void OnComponentTypeAdded(Type obj)
        {
            var handler = ComponentTypeAdded;
            if (handler != null) handler(this, obj);
        }

        protected virtual void OnEntityAdded(Entity e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnEntityRemoved(Entity e)
        {
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnComponentChanged(EntityComponentEventArgs e)
        {
            var handler = ComponentChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Gets the internal association entities -> processor (only used by tests)
        /// </summary>
        /// <returns>The map between entity -> processors</returns>
        internal List<EntityProcessor> GetProcessorsByEntity(Entity entity)
        {
            List<EntityProcessor> localProcessors;
            entities.TryGetValue(entity, out localProcessors);
            return localProcessors;
        }

        private class EntityProcessorComparer : Comparer<EntityProcessor>
        {
            public new static readonly EntityProcessorComparer Default = new EntityProcessorComparer();

            public override int Compare(EntityProcessor x, EntityProcessor y)
            {
                return x.Order.CompareTo(y.Order);
            }
        }
    }
}